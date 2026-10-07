import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import './App.css'

const API_URL = '/api/todos'
const CATEGORIES_URL = '/api/categories'
const USERS_URL = '/api/users'

type TodoCategory = {
  id: number
  name: string
}

type TodoUser = {
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
  userId: number | null
  userName: string | null
}

function App() {
  const [todos, setTodos] = useState<TodoItem[]>([])
  const [categories, setCategories] = useState<TodoCategory[]>([])
  const [users, setUsers] = useState<TodoUser[]>([])
  const [title, setTitle] = useState('')
  const [selectedCategoryId, setSelectedCategoryId] = useState('')
  const [selectedUserId, setSelectedUserId] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [draftTitle, setDraftTitle] = useState('')
  const [draftCategoryId, setDraftCategoryId] = useState('')
  const [draftUserId, setDraftUserId] = useState('')
  const [categoryName, setCategoryName] = useState('')
  const [editingCategoryId, setEditingCategoryId] = useState<number | null>(null)
  const [draftCategoryName, setDraftCategoryName] = useState('')
  const [userName, setUserName] = useState('')
  const [editingUserId, setEditingUserId] = useState<number | null>(null)
  const [draftUserName, setDraftUserName] = useState('')
  const [isSavingUser, setIsSavingUser] = useState(false)
  const [isSavingTodo, setIsSavingTodo] = useState(false)
  const isSavingTodoRef = useRef(false)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState(false)

  const completedCount = useMemo(
    () => todos.filter((todo) => todo.isCompleted).length,
    [todos],
  )

  const beginTodoSave = (): boolean => {
    if (isSavingTodoRef.current) {
      return false
    }

    isSavingTodoRef.current = true
    setIsSavingTodo(true)
    return true
  }

  const endTodoSave = (): void => {
    isSavingTodoRef.current = false
    setIsSavingTodo(false)
  }

  const loadTodos = async (): Promise<void> => {
    try {
      const [todosResponse, categoriesResponse, usersResponse] = await Promise.all([
        fetch(API_URL),
        fetch(CATEGORIES_URL),
        fetch(USERS_URL),
      ])
      if (!todosResponse.ok) {
        throw new Error('No se pudieron cargar las tareas.')
      }
      if (!categoriesResponse.ok) {
        throw new Error('No se pudieron cargar las categorías.')
      }
      if (!usersResponse.ok) {
        throw new Error('No se pudieron cargar los usuarios.')
      }

      const [todoData, categoryData, userData] = await Promise.all([
        todosResponse.json() as Promise<TodoItem[]>,
        categoriesResponse.json() as Promise<TodoCategory[]>,
        usersResponse.json() as Promise<TodoUser[]>,
      ])
      setTodos(todoData)
      setCategories(categoryData)
      setUsers(userData)
      setError('')
      setLoadError(false)
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al cargar las tareas.')
      setLoadError(true)
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

    if (!beginTodoSave()) {
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
          userId: selectedUserId ? Number(selectedUserId) : null,
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
      setSelectedUserId('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo crear la tarea.')
    } finally {
      endTodoSave()
    }
  }

  const handleToggleTodo = async (todo: TodoItem) => {
    if (!beginTodoSave()) {
      return
    }

    try {
      const response = await fetch(`${API_URL}/${todo.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          id: todo.id,
          title: todo.title,
          isCompleted: !todo.isCompleted,
          categoryId: todo.categoryId,
          userId: todo.userId,
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
    } finally {
      endTodoSave()
    }
  }

  const handleDeleteTodo = async (id: number) => {
    if (!beginTodoSave()) {
      return
    }

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
        setDraftUserId('')
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo eliminar la tarea.')
    } finally {
      endTodoSave()
    }
  }

  const handleEditStart = (todo: TodoItem) => {
    setEditingId(todo.id)
    setDraftTitle(todo.title)
    setDraftCategoryId(todo.categoryId?.toString() ?? '')
    setDraftUserId(todo.userId?.toString() ?? '')
    setError('')
  }

  const handleSaveEdit = async (id: number) => {
    const trimmedTitle = draftTitle.trim()
    if (!trimmedTitle) {
      setError('El título no puede estar vacío.')
      return
    }

    const todo = todos.find((item) => item.id === id)
    if (!todo || !beginTodoSave()) {
      return
    }

    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          id,
          title: trimmedTitle,
          isCompleted: todo.isCompleted,
          categoryId: draftCategoryId ? Number(draftCategoryId) : null,
          userId: draftUserId ? Number(draftUserId) : null,
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
      setDraftUserId('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo guardar la edición.')
    } finally {
      endTodoSave()
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

  const handleAddUser = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const name = userName.trim()
    if (!name) {
      setError('El nombre del usuario no puede estar vacío.')
      return
    }

    setIsSavingUser(true)
    try {
      const response = await fetch(USERS_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name }),
      })
      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo crear el usuario.')
      }

      const user = (await response.json()) as TodoUser
      setUsers((current) =>
        [...current, user].sort((a, b) => a.name.localeCompare(b.name) || a.id - b.id),
      )
      setUserName('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo crear el usuario.')
    } finally {
      setIsSavingUser(false)
    }
  }

  const handleSaveUser = async (id: number) => {
    const name = draftUserName.trim()
    if (!name) {
      setError('El nombre del usuario no puede estar vacío.')
      return
    }

    setIsSavingUser(true)
    try {
      const response = await fetch(`${USERS_URL}/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name }),
      })
      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo guardar el usuario.')
      }

      const user = (await response.json()) as TodoUser
      setUsers((current) =>
        current.map((item) => item.id === user.id ? user : item)
          .sort((a, b) => a.name.localeCompare(b.name) || a.id - b.id),
      )
      setTodos((current) =>
        current.map((todo) => todo.userId === user.id
          ? { ...todo, userName: user.name }
          : todo),
      )
      setEditingUserId(null)
      setDraftUserName('')
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo guardar el usuario.')
    } finally {
      setIsSavingUser(false)
    }
  }

  const handleDeleteUser = async (id: number) => {
    setIsSavingUser(true)
    try {
      const response = await fetch(`${USERS_URL}/${id}`, { method: 'DELETE' })
      if (!response.ok) {
        const payload = (await response.json().catch(() => ({}))) as { message?: string }
        throw new Error(payload.message || 'No se pudo eliminar el usuario.')
      }

      setUsers((current) => current.filter((user) => user.id !== id))
      setSelectedUserId((current) => Number(current) === id ? '' : current)
      setDraftUserId((current) => Number(current) === id ? '' : current)
      if (editingUserId === id) {
        setEditingUserId(null)
        setDraftUserName('')
      }
      setError('')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'No se pudo eliminar el usuario.')
    } finally {
      setIsSavingUser(false)
    }
  }

  return (
    <main className="app-shell">
      <div className="app-container">
        <header className="brand-bar">
          <a className="brand" href="/" aria-label="Nexo, inicio">
            <span className="brand-mark" aria-hidden="true">N</span>
            <span className="brand-copy">
              <strong>NEXO</strong>
              <small>GESTIÓN DE TRABAJO</small>
            </span>
          </a>
          <span className="workspace-label">
            <span className="status-dot" aria-hidden="true" />
            ESPACIO PERSONAL
          </span>
        </header>

        <section className="dashboard">
          <header className="dashboard-heading">
            <div>
              <p className="eyebrow">PANEL DE PRODUCTIVIDAD</p>
              <h1>Tu trabajo, en orden.</h1>
              <p className="dashboard-description">
                Organiza tus prioridades y avanza con claridad.
              </p>
            </div>
            <div className="completion-summary" aria-live="polite">
              <span className="completion-number">{completedCount}<span>/{todos.length}</span></span>
              <span className="completion-label">tareas completadas</span>
            </div>
          </header>

          <div className="workspace-grid">
            <section className="todo-card" aria-labelledby="tasks-heading">
              <div className="section-heading">
                <div>
                  <p className="eyebrow">SEGUIMIENTO</p>
                  <h2 id="tasks-heading">Mis tareas</h2>
                </div>
                <span className="task-total">{todos.length} en total</span>
              </div>

              <form className="todo-form" onSubmit={handleAddTodo}>
                <input
                  type="text"
                  value={title}
                  onChange={(event) => setTitle(event.target.value)}
                  placeholder="¿Qué necesitas hacer?"
                  aria-label="Nueva tarea"
                  disabled={isSavingTodo}
                />
                <select
                  value={selectedCategoryId}
                  onChange={(event) => setSelectedCategoryId(event.target.value)}
                  aria-label="Categoría de la nueva tarea"
                  disabled={isLoading || isSavingTodo}
                >
                  <option value="">Sin categoría</option>
                  {categories.map((category) => (
                    <option key={category.id} value={category.id}>{category.name}</option>
                  ))}
                </select>
                <select
                  value={selectedUserId}
                  onChange={(event) => setSelectedUserId(event.target.value)}
                  aria-label="Usuario asignado a la nueva tarea"
                  disabled={isLoading || isSavingTodo}
                >
                  <option value="">Sin asignar</option>
                  {users.map((user) => (
                    <option key={user.id} value={user.id}>{user.name} (ID: {user.id})</option>
                  ))}
                </select>
                <button type="submit" disabled={isLoading || isSavingTodo}>
                  <span aria-hidden="true">+</span> Añadir tarea
                </button>
              </form>

              {error && <p className="error-message" role="alert">{error}</p>}
              {loadError && (
                <button
                  type="button"
                  className="retry-button"
                  disabled={isLoading}
                  onClick={() => {
                    setIsLoading(true)
                    void loadTodos()
                  }}
                >
                  {isLoading ? 'Cargando...' : 'Reintentar carga'}
                </button>
              )}

              {isLoading ? (
                <p className="empty-state">Cargando tareas...</p>
              ) : loadError ? (
                <p className="empty-state">No se pudo cargar la lista de tareas.</p>
              ) : todos.length === 0 ? (
                <p className="empty-state">Todavía no hay tareas. Añade la primera para empezar.</p>
              ) : (
                <ul className="todo-list">
                  {todos.map((todo) => (
                    <li key={todo.id} className={`todo-item ${todo.isCompleted ? 'completed' : ''}`}>
                      <label className="todo-toggle">
                        <input
                          type="checkbox"
                          checked={todo.isCompleted}
                          onChange={() => handleToggleTodo(todo)}
                          disabled={isSavingTodo}
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
                            disabled={isSavingTodo}
                          />
                          <select
                            value={draftCategoryId}
                            onChange={(event) => setDraftCategoryId(event.target.value)}
                            aria-label={`Categoría de ${todo.title}`}
                            disabled={isSavingTodo}
                          >
                            <option value="">Sin categoría</option>
                            {categories.map((category) => (
                              <option key={category.id} value={category.id}>{category.name}</option>
                            ))}
                          </select>
                          <select
                            value={draftUserId}
                            onChange={(event) => setDraftUserId(event.target.value)}
                            aria-label={`Usuario asignado a ${todo.title}`}
                            disabled={isSavingTodo}
                          >
                            <option value="">Sin asignar</option>
                            {users.map((user) => (
                              <option key={user.id} value={user.id}>{user.name} (ID: {user.id})</option>
                            ))}
                          </select>
                          <div className="todo-actions">
                            <button
                              type="button"
                              className="secondary"
                              onClick={() => handleSaveEdit(todo.id)}
                              disabled={isSavingTodo}
                            >
                              {isSavingTodo ? 'Guardando...' : 'Guardar'}
                            </button>
                            <button
                              type="button"
                              className="ghost"
                              onClick={() => {
                                setEditingId(null)
                                setDraftTitle('')
                                setDraftCategoryId('')
                                setDraftUserId('')
                              }}
                              disabled={isSavingTodo}
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
                              {todo.categoryName && <span className="todo-category">{todo.categoryName}</span>}
                              <span className="todo-user">{todo.userName ?? 'Sin asignar'}</span>
                            </small>
                          </div>
                          <div className="todo-actions">
                            <button
                              type="button"
                              className="secondary"
                              onClick={() => handleEditStart(todo)}
                              disabled={isSavingTodo}
                            >
                              Editar
                            </button>
                            <button
                              type="button"
                              className="danger"
                              onClick={() => handleDeleteTodo(todo.id)}
                              disabled={isSavingTodo}
                            >
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

            <div className="catalog-column">
            <section className="category-section" aria-labelledby="categories-heading">
              <div className="section-heading">
                <div>
                  <p className="eyebrow">ORGANIZACIÓN</p>
                  <h2 id="categories-heading">Categorías</h2>
                </div>
                <span className="category-count">{categories.length}</span>
              </div>
              <form className="category-form" onSubmit={handleAddCategory}>
                <input
                  type="text"
                  value={categoryName}
                  onChange={(event) => setCategoryName(event.target.value)}
                  placeholder="Nombre de categoría"
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
                          <span className="category-name">{category.name}</span>
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
                <p className="empty-state">Crea categorías para agrupar tus tareas.</p>
              )}
            </section>
            <section className="category-section" aria-labelledby="users-heading">
              <div className="section-heading">
                <div>
                  <p className="eyebrow">EQUIPO</p>
                  <h2 id="users-heading">Usuarios</h2>
                </div>
                <span className="category-count">{users.length}</span>
              </div>
              <form className="category-form" onSubmit={handleAddUser}>
                <input
                  type="text"
                  value={userName}
                  onChange={(event) => setUserName(event.target.value)}
                  placeholder="Nombre de usuario"
                  aria-label="Nombre del nuevo usuario"
                  disabled={isSavingUser || isLoading}
                />
                <button type="submit" disabled={isSavingUser || isLoading}>Crear usuario</button>
              </form>
              {isLoading ? (
                <p className="empty-state">Cargando usuarios...</p>
              ) : loadError ? (
                <p className="empty-state">No se pudo cargar el catálogo de usuarios.</p>
              ) : users.length > 0 ? (
                <ul className="category-list">
                  {users.map((user) => (
                    <li key={user.id} className="category-item">
                      {editingUserId === user.id ? (
                        <>
                          <input
                            type="text"
                            value={draftUserName}
                            onChange={(event) => setDraftUserName(event.target.value)}
                            aria-label={`Nuevo nombre para ${user.name}, ID ${user.id}`}
                            disabled={isSavingUser || isLoading}
                          />
                          <button
                            type="button"
                            className="secondary"
                            onClick={() => handleSaveUser(user.id)}
                            disabled={isSavingUser || isLoading}
                          >
                            Guardar
                          </button>
                          <button
                            type="button"
                            className="ghost"
                            onClick={() => {
                              setEditingUserId(null)
                              setDraftUserName('')
                            }}
                            disabled={isSavingUser || isLoading}
                          >
                            Cancelar
                          </button>
                        </>
                      ) : (
                        <>
                          <span className="category-name">{user.name} (ID: {user.id})</span>
                          <div className="todo-actions">
                            <button
                              type="button"
                              className="secondary"
                              onClick={() => {
                                setEditingUserId(user.id)
                                setDraftUserName(user.name)
                              }}
                              disabled={isSavingUser || isLoading}
                            >
                              Renombrar
                            </button>
                            <button
                              type="button"
                              className="danger"
                              onClick={() => {
                                if (window.confirm('¿Eliminar este usuario? Si tiene tareas asignadas, primero debes reasignarlas o desasignarlas.')) {
                                  void handleDeleteUser(user.id)
                                }
                              }}
                              disabled={isSavingUser || isLoading}
                            >
                              Eliminar
                            </button>
                          </div>
                        </>
                      )}
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="empty-state">Crea usuarios para poder asignarlos a tareas.</p>
              )}
            </section>
            </div>
          </div>
        </section>

        <footer className="app-footer">
          <span>NEXO <span aria-hidden="true">·</span> PRODUCTIVIDAD PERSONAL</span>
          <span>Un paso a la vez.</span>
        </footer>
      </div>
    </main>
  )
}

export default App
