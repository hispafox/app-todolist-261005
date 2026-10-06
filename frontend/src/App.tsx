import { useEffect, useMemo, useState, type FormEvent } from 'react'
import './App.css'

const API_URL = '/api/todos'
const CATEGORIES_URL = '/api/categories'

type TodoCategory = {
  id: number
  name: string
}

type TodoItem = {
  id: number
  title: string
  isCompleted: boolean
  createdAt: string
  categoryId: number | null
  categoryName: string | null
}

function App() {
  const [todos, setTodos] = useState<TodoItem[]>([])
  const [categories, setCategories] = useState<TodoCategory[]>([])
  const [title, setTitle] = useState('')
  const [selectedCategoryId, setSelectedCategoryId] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [draftTitle, setDraftTitle] = useState('')
  const [draftCategoryId, setDraftCategoryId] = useState('')
  const [categoryName, setCategoryName] = useState('')
  const [editingCategoryId, setEditingCategoryId] = useState<number | null>(null)
  const [draftCategoryName, setDraftCategoryName] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)

  const completedCount = useMemo(
    () => todos.filter((todo) => todo.isCompleted).length,
    [todos],
  )

  const loadTodos = async (): Promise<void> => {
    try {
      const [todosResponse, categoriesResponse] = await Promise.all([
        fetch(API_URL),
        fetch(CATEGORIES_URL),
      ])
      if (!todosResponse.ok || !categoriesResponse.ok) {
        throw new Error('No se pudieron cargar las tareas.')
      }

      const [todoData, categoryData] = await Promise.all([
        todosResponse.json() as Promise<TodoItem[]>,
        categoriesResponse.json() as Promise<TodoCategory[]>,
      ])
      setTodos(todoData)
      setCategories(categoryData)
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
        body: JSON.stringify({
          title: trimmedTitle,
          isCompleted: false,
          categoryId: selectedCategoryId ? Number(selectedCategoryId) : null,
        }),
      })

      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo crear la tarea.')
      }

      const createdTodo = (await response.json()) as TodoItem
      setTodos((currentTodos) => [createdTodo, ...currentTodos])
      setTitle('')
      setSelectedCategoryId('')
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
          categoryId: todo.categoryId,
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
        setDraftCategoryId('')
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo eliminar la tarea.')
    }
  }

  const handleEditStart = (todo: TodoItem) => {
    setEditingId(todo.id)
    setDraftTitle(todo.title)
    setDraftCategoryId(todo.categoryId?.toString() ?? '')
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
          categoryId: draftCategoryId ? Number(draftCategoryId) : null,
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
      setDraftCategoryId('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo guardar la edición.')
    }
  }

  const handleAddCategory = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const name = categoryName.trim()
    if (!name) {
      setError('El nombre de la categoría no puede estar vacío.')
      return
    }

    try {
      const response = await fetch(CATEGORIES_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name }),
      })
      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo crear la categoría.')
      }

      const category = (await response.json()) as TodoCategory
      setCategories((current) => [...current, category].sort((a, b) => a.name.localeCompare(b.name)))
      setCategoryName('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo crear la categoría.')
    }
  }

  const handleSaveCategory = async (id: number) => {
    const name = draftCategoryName.trim()
    if (!name) {
      setError('El nombre de la categoría no puede estar vacío.')
      return
    }

    try {
      const response = await fetch(`${CATEGORIES_URL}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name }),
      })
      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo guardar la categoría.')
      }

      const category = (await response.json()) as TodoCategory
      setCategories((current) =>
        current.map((item) => item.id === category.id ? category : item)
          .sort((a, b) => a.name.localeCompare(b.name)),
      )
      setTodos((current) =>
        current.map((todo) => todo.categoryId === category.id
          ? { ...todo, categoryName: category.name }
          : todo),
      )
      setEditingCategoryId(null)
      setDraftCategoryName('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo guardar la categoría.')
    }
  }

  const handleDeleteCategory = async (id: number) => {
    try {
      const response = await fetch(`${CATEGORIES_URL}/${id}`, { method: 'DELETE' })
      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo eliminar la categoría.')
      }

      setCategories((current) => current.filter((category) => category.id !== id))
      setSelectedCategoryId((current) => Number(current) === id ? '' : current)
      setDraftCategoryId((current) => Number(current) === id ? '' : current)
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo eliminar la categoría.')
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
          <select
            value={selectedCategoryId}
            onChange={(event) => setSelectedCategoryId(event.target.value)}
            aria-label="Categoría de la nueva tarea"
          >
            <option value="">Sin categoría</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>{category.name}</option>
            ))}
          </select>
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
                    <select
                      value={draftCategoryId}
                      onChange={(event) => setDraftCategoryId(event.target.value)}
                      aria-label={`Categoría de ${todo.title}`}
                    >
                      <option value="">Sin categoría</option>
                      {categories.map((category) => (
                        <option key={category.id} value={category.id}>{category.name}</option>
                      ))}
                    </select>
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
                          setDraftCategoryId('')
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
                      <small>
                        {new Date(todo.createdAt).toLocaleDateString()}
                        {todo.categoryName && ` · ${todo.categoryName}`}
                      </small>
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

        <section className="category-section" aria-labelledby="categories-heading">
          <h2 id="categories-heading">Categorías</h2>
          <form className="category-form" onSubmit={handleAddCategory}>
            <input
              type="text"
              value={categoryName}
              onChange={(event) => setCategoryName(event.target.value)}
              placeholder="Nueva categoría..."
              aria-label="Nombre de la nueva categoría"
            />
            <button type="submit">Crear categoría</button>
          </form>
          {categories.length > 0 ? (
            <ul className="category-list">
              {categories.map((category) => (
                <li key={category.id} className="category-item">
                  {editingCategoryId === category.id ? (
                    <>
                      <input
                        type="text"
                        value={draftCategoryName}
                        onChange={(event) => setDraftCategoryName(event.target.value)}
                        aria-label={`Nuevo nombre para ${category.name}`}
                      />
                      <button type="button" className="secondary" onClick={() => handleSaveCategory(category.id)}>
                        Guardar
                      </button>
                      <button
                        type="button"
                        className="ghost"
                        onClick={() => {
                          setEditingCategoryId(null)
                          setDraftCategoryName('')
                        }}
                      >
                        Cancelar
                      </button>
                    </>
                  ) : (
                    <>
                      <span>{category.name}</span>
                      <div className="todo-actions">
                        <button
                          type="button"
                          className="secondary"
                          onClick={() => {
                            setEditingCategoryId(category.id)
                            setDraftCategoryName(category.name)
                          }}
                        >
                          Renombrar
                        </button>
                        <button type="button" className="danger" onClick={() => handleDeleteCategory(category.id)}>
                          Eliminar
                        </button>
                      </div>
                    </>
                  )}
                </li>
              ))}
            </ul>
          ) : (
            <p className="empty-state">Aún no hay categorías.</p>
          )}
        </section>
      </section>
    </main>
  )
}

export default App
