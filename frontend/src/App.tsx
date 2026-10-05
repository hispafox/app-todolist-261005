import { useEffect, useMemo, useState, type FormEvent } from 'react'
import './App.css'

const API_URL = '/api/todos'

type TodoItem = {
  id: number
  title: string
  isCompleted: boolean
  createdAt: string
}

function App() {
  const [todos, setTodos] = useState<TodoItem[]>([])
  const [title, setTitle] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [draftTitle, setDraftTitle] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)

  const completedCount = useMemo(
    () => todos.filter((todo) => todo.isCompleted).length,
    [todos],
  )

  const loadTodos = async (): Promise<void> => {
    try {
      setIsLoading(true)
      const response = await fetch(API_URL)
      if (!response.ok) {
        throw new Error('No se pudieron cargar las tareas.')
      }

      const data = (await response.json()) as TodoItem[]
      setTodos(data)
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al cargar las tareas.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadTodos().catch(() => setError('Error al cargar las tareas.'))
  }, [])

  const handleAddTodo = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const trimmedTitle = title.trim()
    if (!trimmedTitle) {
      setError('El título no puede estar vacío.')
      return
    }

    try {
      const response = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title: trimmedTitle, isCompleted: false }),
      })

      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo crear la tarea.')
      }

      const createdTodo = (await response.json()) as TodoItem
      setTodos((currentTodos) => [createdTodo, ...currentTodos])
      setTitle('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo crear la tarea.')
    }
  }

  const handleToggleTodo = async (todo: TodoItem) => {
    try {
      const response = await fetch(`${API_URL}/${todo.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          id: todo.id,
          title: todo.title,
          isCompleted: !todo.isCompleted,
        }),
      })

      if (!response.ok) {
        throw new Error('No se pudo actualizar la tarea.')
      }

      const updatedTodo = (await response.json()) as TodoItem
      setTodos((currentTodos) =>
        currentTodos.map((item) =>
          item.id === updatedTodo.id ? updatedTodo : item,
        ),
      )
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo actualizar la tarea.')
    }
  }

  const handleDeleteTodo = async (id: number) => {
    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: 'DELETE',
      })

      if (!response.ok) {
        throw new Error('No se pudo eliminar la tarea.')
      }

      setTodos((currentTodos) => currentTodos.filter((todo) => todo.id !== id))
      setError('')

      if (editingId === id) {
        setEditingId(null)
        setDraftTitle('')
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo eliminar la tarea.')
    }
  }

  const handleEditStart = (todo: TodoItem) => {
    setEditingId(todo.id)
    setDraftTitle(todo.title)
    setError('')
  }

  const handleSaveEdit = async (id: number) => {
    const trimmedTitle = draftTitle.trim()
    if (!trimmedTitle) {
      setError('El título no puede estar vacío.')
      return
    }

    try {
      const todo = todos.find((item) => item.id === id)
      if (!todo) {
        return
      }

      const response = await fetch(`${API_URL}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          id,
          title: trimmedTitle,
          isCompleted: todo.isCompleted,
        }),
      })

      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo guardar la edición.')
      }

      const updatedTodo = (await response.json()) as TodoItem
      setTodos((currentTodos) =>
        currentTodos.map((item) =>
          item.id === updatedTodo.id ? updatedTodo : item,
        ),
      )
      setEditingId(null)
      setDraftTitle('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo guardar la edición.')
    }
  }

  return (
    <main className="app-shell">
      <section className="todo-card">
        <header className="app-header">
          <div>
            <p className="eyebrow">Tareas</p>
            <h1>Mi lista</h1>
          </div>
          <span className="badge">{completedCount}/{todos.length} completadas</span>
        </header>

        <form className="todo-form" onSubmit={handleAddTodo}>
          <input
            type="text"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            placeholder="Añadir una tarea..."
            aria-label="Nueva tarea"
          />
          <button type="submit">Añadir</button>
        </form>

        {error && <p className="error-message">{error}</p>}

        {isLoading ? (
          <p className="empty-state">Cargando tareas...</p>
        ) : todos.length === 0 ? (
          <p className="empty-state">No hay tareas aún. ¡Añade la primera!</p>
        ) : (
          <ul className="todo-list">
            {todos.map((todo) => (
              <li key={todo.id} className={`todo-item ${todo.isCompleted ? 'completed' : ''}`}>
                <label className="todo-toggle">
                  <input
                    type="checkbox"
                    checked={todo.isCompleted}
                    onChange={() => handleToggleTodo(todo)}
                  />
                  <span className="checkmark" aria-hidden="true" />
                </label>

                {editingId === todo.id ? (
                  <div className="todo-edit">
                    <input
                      type="text"
                      value={draftTitle}
                      onChange={(event) => setDraftTitle(event.target.value)}
                      autoFocus
                    />
                    <div className="todo-actions">
                      <button type="button" className="secondary" onClick={() => handleSaveEdit(todo.id)}>
                        Guardar
                      </button>
                      <button
                        type="button"
                        className="ghost"
                        onClick={() => {
                          setEditingId(null)
                          setDraftTitle('')
                        }}
                      >
                        Cancelar
                      </button>
                    </div>
                  </div>
                ) : (
                  <>
                    <div className="todo-content">
                      <span className="todo-title">{todo.title}</span>
                      <small>{new Date(todo.createdAt).toLocaleDateString()}</small>
                    </div>
                    <div className="todo-actions">
                      <button type="button" className="secondary" onClick={() => handleEditStart(todo)}>
                        Editar
                      </button>
                      <button type="button" className="danger" onClick={() => handleDeleteTodo(todo.id)}>
                        Eliminar
                      </button>
                    </div>
                  </>
                )}
              </li>
            ))}
          </ul>
        )}
      </section>
    </main>
  )
}

export default App
