# Plan: sistema de categorías para tareas

## 1. Resumen y objetivo

Este plan define la implementación del sistema de categorías para la aplicación de tareas. La finalidad es permitir organizar tareas por categoría reutilizable, conservar la relación con SQLite y mantener la experiencia del usuario consistente entre backend y frontend sin introducir complejidad innecesaria.

El objetivo funcional es el siguiente:

- Crear, listar, consultar, renombrar y eliminar categorías.
- Asociar una tarea a una categoría opcional.
- Mantener la categoría visible al listar o consultar una tarea.
- Rechazar nombres duplicados o categorías en uso al intentar eliminarlas.
- Mantener compatibilidad con las tareas ya almacenadas y con la API actual.

Este documento no implementa código; define el contrato de trabajo que el desarrollador debe seguir.

## 2. Estado actual confirmado

El repositorio ya contempla una app Todo App basada en:

- .NET 10 + ASP.NET Core Minimal API
- Entity Framework Core + SQLite
- React + TypeScript + Vite
- estructura de capas clara con backend y frontend separados

El análisis del producto confirma que la categoría es una entidad reutilizable y opcional, con relación uno a muchos entre `TodoCategory` y `TodoItem`. La clave foránea es nullable para permitir tareas sin categoría. Además, la API debe impedir el borrado de una categoría mientras siga asociada a tareas, y debe conservar tareas ya existentes sin categoría.

Se considera confirmado que la aplicación debe:

- mantener las rutas existentes de tareas;
- ampliar el contrato de creación y edición de tareas con `categoryId`;
- devolver errores HTTP claros para validaciones y conflictos;
- usar SQLite con migraciones para evolucionar el esquema sin romper datos existentes.

## 3. Requisitos de la petición

### Requisitos funcionales

- Crear categorías con un nombre válido.
- Normalizar el nombre de la categoría: sin espacios sobrantes y sin duplicados por diferencia de mayúsculas.
- Listar categorías disponibles.
- Obtener una categoría concreta por id.
- Actualizar el nombre de una categoría.
- Eliminar una categoría únicamente si no tiene tareas asociadas.
- Permitir que una tarea tenga cero o una categoría.
- Mostrar la categoría asociada en la interfaz de tareas.

### Restricciones y supuestos

- La relación debe ser opcional para no romper tareas existentes.
- No se añadirá autenticación ni gestión de usuarios.
- No se incorporan colores, prioridades ni metadatos adicionales de categoría a esta iteración.
- La validación debe estar consistente entre backend y base de datos.
- La API debe responder con errores explícitos para recursos inexistentes, entradas inválidas y conflictos de integridad.

## 4. Modelo de datos y restricciones

### Entidad `TodoItem`

| Campo | Tipo | Restricción |
|---|---|---|
| `Id` | `int` | Identificador único |
| `Title` | `string` | Obligatorio, no vacío |
| `IsCompleted` | `bool` | Estado de la tarea |
| `CreatedAt` | `DateTime` | Fecha de creación |
| `CategoryId` | `int?` | Clave foránea opcional |
| `Category` | `TodoCategory?` | Navegación opcional |

### Entidad `TodoCategory`

| Campo | Tipo | Restricción |
|---|---|---|
| `Id` | `int` | Identificador único |
| `Name` | `string` | Obligatorio, único e insensible a mayúsculas |
| `TodoItems` | `ICollection<TodoItem>` | Colección de tareas asociadas |

### Regla de integridad

- Una categoría puede tener muchas tareas.
- Cada tarea puede tener una sola categoría.
- La clave foránea debe impedir el borrado de categorías con tareas asociadas.
- La migración debe conservar tareas existentes como sin categoría cuando la relación sea nueva.

## 5. DTOs y contratos de entrada/salida

Los contratos deben mantener una separación clara entre la API y el modelo de dominio.

### DTOs de entrada

- `CreateTodoRequest`
  - `title`: string
  - `isCompleted`: bool
  - `categoryId`: int? (opcional)

- `UpdateTodoRequest`
  - `title`: string
  - `isCompleted`: bool
  - `categoryId`: int? (opcional)

- `CreateCategoryRequest`
  - `name`: string

- `UpdateCategoryRequest`
  - `name`: string

### DTOs de salida

- `TodoDto`
  - `id`, `title`, `isCompleted`, `createdAt`, `categoryId`, `categoryName`

- `CategoryDto`
  - `id`, `name`

### Reglas de contrato

- `categoryId` puede omitirse o ser `null`.
- `categoryName` debe devolver el nombre de la categoría si existe.
- Los nombres deben normalizarse antes de persistir.
- Las respuestas de error deben indicar si el problema es de validación, recurso no encontrado o conflicto de integridad.

## 6. Endpoints y códigos HTTP

| Verbo | Ruta | Resultado esperado |
|---|---|---|
| GET | `/api/categories` | 200 + array de categorías |
| GET | `/api/categories/{id}` | 200 + categoría o 404 |
| POST | `/api/categories` | 201 + categoría creada |
| PUT | `/api/categories/{id}` | 200 + categoría actualizada |
| DELETE | `/api/categories/{id}` | 204 si se elimina, 409 si está en uso |
| GET | `/api/todos` | 200 + tareas con categoría incluida |
| GET | `/api/todos/{id}` | 200 + tarea o 404 |
| POST | `/api/todos` | 201 + tarea creada |
| PUT | `/api/todos/{id}` | 200 + tarea editada |
| DELETE | `/api/todos/{id}` | 204 |

### Códigos de error esperados

- 400: entrada inválida, título vacío, nombre duplicado, categoría inexistente
- 404: recurso no encontrado
- 409: conflicto por nombre duplicado o categoría en uso

La lógica de manejo de errores debe impedir escrituras parciales y mantener la semántica existente de la API.

## 7. Lógica de negocio y validaciones

La lógica debe centralizarse en la capa de servicio y/o lógica de negocio para evitar que el controlador haga validaciones de dominio.

### Validaciones de dominio

- `Title` y `Name` no pueden quedar vacíos tras normalizar.
- No pueden existir dos categorías con el mismo nombre ignorando mayúsculas.
- No se puede borrar una categoría que esté asignada a tareas.
- El `categoryId` enviado al crear o editar tarea debe apuntar a una categoría existente.

### Comportamiento esperado

- La normalización de nombre debe hacer `Trim()` antes de comparar o guardar.
- La comparación de duplicados debe ser insensible a mayúsculas/minúsculas.
- La eliminación de categorías debe hacerse solo si la colección de tareas asociadas está vacía.
- La edición de tareas debe poder asignar, cambiar o quitar la categoría sin destruir el resto de datos.

## 8. Capas afectadas y archivos clave

### Backend

- `backend/TodoApi/Models/TodoItem.cs`
- `backend/TodoApi/Models/TodoCategory.cs`
- `backend/TodoApi/Data/TodoDbContext.cs`
- `backend/TodoApi/Dtos/ApiDtos.cs`
- `backend/TodoApi/Services/TodoService.cs`
- `backend/TodoApi/Program.cs`
- `backend/TodoApi/Migrations/`

### Frontend

- `frontend/src/App.tsx`
- `frontend/src/App.css`
- tipos de respuesta y formularios reutilizados en la UI

### Pruebas

- `backend/TodoApi.Tests/TodoServiceTests.cs`
- pruebas de integración o de servicio para validación de categorías y restricciones

## 9. Pruebas y criterios de aceptación

### Pruebas técnicas recomendadas

- Crear una categoría con nombre válido.
- Intentar crear una categoría duplicada con distinto formato de caja, y comprobar que devuelve 409.
- Eliminar una categoría sin tareas y comprobar `204`.
- Intentar eliminar una categoría con tareas y comprobar `409`.
- Crear una tarea con `categoryId` válido y confirmarlo en la respuesta.
- Crear una tarea con `categoryId` no existente y comprobar `400`.
- Editar una tarea para cambiar la categoría, quitarla o asignarla a otra.
- Aplicar la migración sobre datos existentes y confirmar que las tareas previas permanecen sin categoría si el campo es opcional.

### Criterios de aceptación del cambio

- La categoría se guarda de forma consistente en SQLite.
- El listado de tareas refleja el nombre de la categoría cuando exista.
- La interfaz permite crear y editar tareas con categoría.
- La eliminación de categoría está protegida cuando tiene tareas relacionadas.
- Se mantienen las tareas existentes y su comportamiento actual sin cambios no previstos.

## 10. Skills a invocar

| Skill | Estado | Orden | Justificación |
|---|---|---:|---|
| `diseno-analisis` | Requerido | 1 | Define el objetivo, el alcance y la arquitectura inicial del cambio. |
| `modelo` | Requerido | 2 | Crea o ajusta `TodoCategory` y la relación con `TodoItem`. |
| `dto` | Requerido | 3 | Define los contratos de entrada y salida para tareas y categorías. |
| `base-de-datos` | Requerido | 4 | Añade el contexto, relación EF Core y migración SQLite. |
| `logica-negocio` | Requerido | 5 | Centraliza la validación de existencia, duplicados y restricciones. |
| `validaciones` | Requerido | 6 | Refuerza las reglas de dominio en DTOs y capa de negocio. |
| `servicio` | Requerido | 7 | Orquesta mapeos, validaciones y persistencia en la capa de servicio. |
| `controlador` | Requerido | 8 | Expone los endpoints REST con errores HTTP acordes. |
| `frontend-react` | Requerido | 9 | Conecta la interfaz con la API para crear, mostrar y editar categorías. |
| `ui-ux-pro-max` | N/A | — | No requiere un patrón de diseño especial para esta funcionalidad básica. |
| `nueva-feature` | N/A | — | El cambio es una feature acotada y se implementa siguiendo los skills específicos de cada capa. |
| `actualizar-documentacion` | Opcional | — | Solo si el análisis o la documentación de producto se desajusta tras la implementación. |

La secuencia anterior es obligatoria porque cada capa depende de la anterior: sin el modelo no hay persistencia, sin DTO no hay contrato, sin lógica no hay validación, sin servicio no hay orquestación y sin controlador no hay API pública. El orden refleja la dependencia real del sistema y evita decisiones a ciegas.
