# Análisis y diseño — Todo App

Este documento es la referencia única de producto, análisis y diseño de la aplicación. Consolida los requisitos funcionales y técnicos; los documentos que sirvieron como fuentes se conservan en `docs/archivo/`.

## 1. Objetivo del proyecto

Todo App es una aplicación web educativa para organizar tareas personales, académicas o profesionales. Permite crear, consultar, completar, editar y eliminar tareas, y las conserva en SQLite. El diseño incorpora categorías reutilizables y opcionales para organizar las tareas.

## 2. Stack tecnológico

| Tecnología | Versión | Rol |
|---|---|---|
| .NET / ASP.NET Core | 10 | API REST mediante Minimal API |
| Entity Framework Core | 10.0.12 | ORM, acceso a datos y migraciones |
| SQLite | — | Base de datos local persistente |
| React | 19.2.8 | Interfaz web con componentes y hooks |
| TypeScript | 5.9 | Tipado estático del frontend |
| Vite | 8.3.0 | Servidor de desarrollo y compilación del frontend |
| xUnit | 2.9.2 | Pruebas unitarias del backend |
| FluentAssertions | 7.2.0 | Aserciones legibles en pruebas |
| EF Core InMemory | 10.0.12 | Persistencia aislada para pruebas unitarias |

## 3. Arquitectura de capas

La solución separa el frontend web del backend API. El cliente React realiza peticiones HTTP a ASP.NET Core; los endpoints delegan las operaciones de dominio y persistencia a servicios; los servicios utilizan `TodoDbContext` y Entity Framework Core para acceder a SQLite. Las migraciones versionan los cambios del esquema.

```text
Frontend React + TypeScript
          |
          | HTTP / JSON
          v
ASP.NET Core Minimal API
          |
          v
Servicios de aplicación
          |
          v
Entity Framework Core -> SQLite
```

Estructura relevante:

```text
backend/
  TodoApi/
    Data/          # DbContext
    Migrations/    # Evolución del esquema
    Models/        # Entidades de dominio
    Services/      # Lógica de tareas y categorías
    Program.cs     # Dependencias y endpoints
  TodoApi.Tests/   # Pruebas unitarias
frontend/
  src/             # Aplicación React y tipos de API
docs/
  archivo/         # Documentos fuente conservados como histórico
```

## 4. Modelo de datos

### `TodoItem`

| Campo | Tipo | Descripción |
|---|---|---|
| `Id` | `int` | Identificador único de la tarea. |
| `Title` | `string` | Título obligatorio y no vacío. |
| `IsCompleted` | `bool` | Indica si está completada. |
| `CreatedAt` | `DateTime` | Fecha de creación en UTC. |
| `CategoryId` | `int?` | Clave foránea opcional de la categoría. |
| `Category` | `TodoCategory?` | Navegación opcional a la categoría. |

### `TodoCategory`

| Campo | Tipo | Descripción |
|---|---|---|
| `Id` | `int` | Identificador único de la categoría. |
| `Name` | `string` | Nombre obligatorio y único sin distinguir mayúsculas; se recortan los espacios exteriores. |
| `TodoItems` | `ICollection<TodoItem>` | Tareas asociadas a la categoría. |

La relación es uno a muchos: una categoría puede tener varias tareas y cada tarea puede no tener categoría o estar asociada a una sola. La clave foránea debe impedir el borrado de una categoría que aún tenga tareas. Una migración debe conservar las tareas ya almacenadas y dejarlas inicialmente sin categoría.

## 5. Endpoints API REST

La siguiente tabla describe el contrato objetivo del producto. Las rutas de tareas existentes se mantienen y se añaden las de categorías.

| Verbo | Ruta | Descripción | Respuesta OK |
|---|---|---|---|
| GET | `/api/todos` | Listar tareas con su categoría si existe. | 200 + array |
| GET | `/api/todos/{id}` | Obtener una tarea por identificador. | 200 + tarea |
| POST | `/api/todos` | Crear una tarea; `categoryId` es opcional o `null`. | 201 + tarea |
| PUT | `/api/todos/{id}` | Editar tarea y asignar, cambiar o quitar su categoría. | 200 + tarea |
| DELETE | `/api/todos/{id}` | Eliminar una tarea. | 204 |
| GET | `/api/categories` | Listar categorías. | 200 + array |
| GET | `/api/categories/{id}` | Obtener una categoría por identificador. | 200 + categoría |
| POST | `/api/categories` | Crear una categoría. | 201 + categoría |
| PUT | `/api/categories/{id}` | Cambiar el nombre de una categoría. | 200 + categoría |
| DELETE | `/api/categories/{id}` | Eliminar una categoría sin tareas asociadas. | 204 |

Las rutas por identificador devuelven 404 si no existe el recurso. Las entradas inválidas, como un título o nombre vacío o una categoría inexistente, devuelven 400. Los nombres de categoría duplicados y los intentos de borrar categorías en uso devuelven 409. Los errores no deben dejar datos parcialmente modificados.

Ejemplo de creación de tarea:

```json
{
  "title": "Terminar el proyecto final",
  "isCompleted": false,
  "categoryId": 1
}
```

`categoryId` puede omitirse o ser `null` para crear una tarea sin categoría.

## 6. Decisiones de diseño

- **Categorías reutilizables como entidad:** permiten mantener un catálogo común y asignar varias tareas a la misma categoría.
- **Asignación opcional:** no obliga a categorizar cada tarea y permite conservar datos existentes.
- **Borrado restringido para categorías en uso:** protege la relación y evita eliminar o reasignar tareas de forma implícita.
- **Nombre único normalizado:** se recortan espacios exteriores y se evita duplicar nombres por diferencias de mayúsculas; la validación debe ser consistente entre la API y la base de datos.
- **Sin color, prioridades ni metadatos de categoría en esta iteración:** limita el modelo a la organización solicitada.
- **SQLite y EF Core:** ofrecen persistencia local sencilla y migraciones sin infraestructura adicional.
- **Minimal API y servicios existentes:** siguen la estructura actual del backend sin añadir capas innecesarias.
- **Errores visibles y contratos HTTP explícitos:** frontend y API deben comunicar claramente validaciones, conflictos y recursos inexistentes.

## 7. Pendientes / Preguntas abiertas

- Decidir si se añadirá un filtro de tareas por categoría en una iteración posterior.
- Definir si el usuario podrá ordenar las categorías o si se mostrarán alfabéticamente.
- La autenticación, paginación, sincronización en la nube y notificaciones quedan fuera del alcance actual.

## 8. Visión, usuarios y alcance del producto

La aplicación resuelve la necesidad de organizar actividades mediante una lista ligera, persistente y fácil de entender. Está dirigida a personas que gestionan tareas personales, académicas o profesionales, como estudiantes, profesionales independientes o equipos pequeños.

El MVP incluye:

- crear, consultar, completar, editar y eliminar tareas;
- asignar o quitar una categoría al crear o editar una tarea;
- crear, consultar, renombrar y eliminar categorías;
- persistencia local con SQLite y consumo mediante API REST;
- una interfaz React sencilla, legible y adaptable a distintos tamaños de pantalla.

Quedan fuera del MVP la gestión de usuarios y roles, tareas compartidas, categorías avanzadas, prioridades, etiquetas, filtros sofisticados, sincronización externa, notificaciones y despliegue de producción.

## 9. Requisitos funcionales

| ID | Requisito | Criterios de aceptación |
|---|---|---|
| RF-01 | Crear tareas | Validar un título no vacío; persistirlo y mostrar la nueva tarea sin recargar toda la aplicación. |
| RF-02 | Consultar tareas | Cargar la lista desde la API y mostrar estado y categoría cuando esté asignada. |
| RF-03 | Completar tareas | Permitir alternar entre pendiente y completada; persistir y reflejar el cambio en la interfaz. |
| RF-04 | Editar tareas | Guardar cambios de título y categoría, manteniendo el estado y el resto de los datos. |
| RF-05 | Eliminar tareas | Eliminar de la base de datos y actualizar la lista visible. |
| RF-06 | Persistir información | Conservar tareas y categorías entre reinicios y evolucionar el esquema mediante migraciones. |
| RF-07 | Consumir la API | Usar peticiones HTTP válidas y mostrar errores de red o servidor sin dejar la UI en un estado incoherente. |
| RF-08 | Validar entradas | Rechazar títulos y nombres vacíos y referencias a categorías inexistentes, con feedback claro. |
| RF-09 | Gestionar categorías | Crear, listar, renombrar y borrar categorías; los nombres son obligatorios y únicos sin distinguir mayúsculas. |
| RF-10 | Asociar categorías | Permitir que una tarea no tenga categoría o tenga una existente; impedir el borrado de categorías con tareas asociadas. |

## 10. Historias y flujos principales

- **Crear una tarea:** el usuario introduce un título y, opcionalmente, selecciona una categoría; el backend valida y persiste la tarea y la interfaz actualiza la lista.
- **Completar una tarea:** el usuario cambia su estado; la API persiste el valor y la interfaz refleja el cambio.
- **Editar una tarea:** el usuario modifica título o categoría; los campos no editados se conservan.
- **Eliminar una tarea:** el usuario solicita el borrado y la tarea desaparece tras confirmarse la respuesta del backend.
- **Organizar tareas:** el usuario crea o renombra categorías y las asigna a tareas. Si intenta borrar una categoría en uso, recibe un mensaje claro y la información permanece intacta.
- **Gestionar errores:** ante un fallo de red, validación o servidor, la aplicación informa del problema y permite reintentar sin asumir que la operación tuvo éxito.

## 11. Requisitos no funcionales

- **Rendimiento:** las operaciones CRUD deben responder sin retrasos perceptibles en el entorno local previsto.
- **Usabilidad:** interfaz clara, legible, accesible a nivel básico y adaptable a pantallas habituales.
- **Seguridad básica:** validar las entradas antes de persistirlas; autenticación y autorización no forman parte del MVP local.
- **Mantenibilidad:** conservar la separación entre frontend y backend, usar migraciones y evitar dependencias y abstracciones innecesarias.
- **Fiabilidad:** una operación fallida no debe dejar la interfaz ni las relaciones de datos en un estado inconsistente.
- **Evolución:** añadir funcionalidades de forma incremental sin romper el flujo CRUD existente.

## 12. Criterios de éxito y riesgos

El producto cumple su objetivo cuando las operaciones principales de tareas funcionan desde la interfaz, las categorías se pueden administrar y asignar según las reglas indicadas, los datos sobreviven a reinicios y la API devuelve respuestas coherentes.

Riesgos principales:

- inconsistencias entre los contratos TypeScript y las respuestas de la API;
- cambios del modelo sin una migración correcta;
- validaciones de unicidad de categoría diferentes entre aplicación y SQLite;
- errores de configuración del entorno local o dependencias no instaladas.

Se asume un único entorno local de desarrollo, usuarios sin necesidad de autenticación y uso educativo o demostrativo.

## 13. Evolución prevista

Como posibles siguientes etapas, se pueden añadir filtros por categoría y estado, ordenamiento por fecha, prioridades, autenticación y pruebas de integración. Estas mejoras no forman parte del alcance actual y deben evaluarse antes de incorporarlas.
