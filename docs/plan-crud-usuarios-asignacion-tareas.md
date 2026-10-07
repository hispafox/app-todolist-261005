# Plan: cierre de usuarios, asignación opcional a tareas y Serilog

## 1. Resumen y objetivo

Terminar y verificar el catálogo local de usuarios y la asignación opcional **ya implementados en el árbol de trabajo**, y añadir registro estructurado backend con Serilog. No son cuentas de acceso: no habrá login, contraseñas, tokens, roles ni autorización. No rehacer capas existentes.

Este documento es un contrato de trabajo, no una implementación. Revisión del 07/10/2026: prevalece sobre [plan-asignacion-tareas-usuarios.md](./plan-asignacion-tareas-usuarios.md), que se conserva como antecedente y contiene un estado anterior a la implementación. Se mantienen los contratos del plan CRUD previo y se incorporan el estado real y la ampliación explícita de Serilog.

Entregable de esta intervención: únicamente este archivo. Sin comandos, implementación, agentes adicionales, commit ni push. `labs/GHCOPTL-M7.4-lab.md` es referencia de responsabilidades, no autorización para ejecutar su ciclo de publicación.

## 2. Estado actual confirmado

Inspección realizada sobre el código y los documentos actuales:

| Superficie | Estado confirmado |
|---|---|
| Análisis | [analisis-diseño.md](./analisis-diseño.md) ya incluye usuarios locales, homónimos, asignación opcional, borrado protegido y Minimal API. Falta incorporar Serilog. |
| Dominio | Existen `TodoUser` con `Id`, `Name`, `TodoItems`, y `TodoItem.UserId` nullable con navegación `User`. `TodoCategory` permanece. Terminado en código. |
| Persistencia | `TodoDbContext` expone los tres catálogos, requiere el nombre del usuario y configura FK de usuario con `DeleteBehavior.Restrict`. |
| DTOs | `ApiDtos.cs` ya contiene `UserRequest`, `UserResponse`, `TodoRequest.UserId = null` al final y `TodoResponse.UserId/UserName`. Terminado en código. |
| HTTP | `Program.cs` contiene los cinco endpoints `/api/users`, además de tareas/categorías y los mapeos completos. Son Minimal API; no existe carpeta `Controllers` en el proyecto inspeccionado. |
| Reglas y acceso a datos | `TodoService.cs` tiene CRUD de usuarios, Trim, homónimos, orden nombre/ID, validación antes de mutación, Include de usuario y tratamiento de FK SQLite 787 en asignación/borrado. No hay interfaces, repositorios ni capa `LogicaNegocio`. |
| Frontend | `App.tsx` ya carga usuarios con tareas/categorías, tiene CRUD, confirmación de borrado, selectores con nombre/ID, «Sin asignar», reintento de carga y feedback; los tres envíos de tarea (alta, edición y completar) incluyen `userId`. Renombrar actualiza nombres en tareas y borrar limpia selecciones. `isSavingUser` protege operaciones de usuarios, no las mutaciones de tareas. |
| Tests | `TodoServiceTests.cs` ya incluye homónimos, nombres inválidos, renombrado, borrado en uso/libre, asignar/cambiar/quitar y IDs inválidos. Todos usan EF InMemory; no acreditan FK, migración, carreras ni HTTP. No hay runner frontend en `package.json`. |
| Migraciones | Existen `20261007100800_AddTodoUsers.cs`, su Designer y snapshot coherentes: tabla `TodoUsers`, columna nullable, índice y FK restrictiva. No generar otra `AddTodoUsers`. Aplicación a la SQLite local no confirmada. |
| Registro | `TodoApi.csproj` no referencia Serilog. `Program.cs` no lo integra ni tiene manejador global de fallos. Ambos appsettings solo configuran `Logging` de ASP.NET. `.gitignore` excluye SQLite/bin/obj, no logs. |

No se han ejecutado comandos, builds, pruebas ni la aplicación. «Terminado en código» significa presente e inspeccionado, **no verificado en ejecución**.

`main` se toma como confirmada por el usuario; no se ha consultado Git en esta intervención. Se ha leído la baseline `C:\Users\hispa\.copilot\session-state\040bf0b1-61ac-458d-8e9c-d3ff37711e21\files\baseline-usuarios-serilog-status.txt` y el comienzo de `baseline-usuarios-serilog.patch` en el mismo directorio. El status contiene modificaciones previas en usuarios, UI y documentos, y archivos ajenos (skills, agentes, manual, previews, etc.). El patch no sustituye el inventario de archivos no seguidos.

Preservar **todos** esos cambios y archivos, incluidos los no seguidos. No reset/clean/restore, cambio de rama, regeneración de UI/migraciones, aplicación inversa de la baseline ni publicación. El implementador debe comparar su delta con ambas baselines y con el estado al iniciar su trabajo; los cambios nuevos ajenos también se preservan. Esta revisión solo modifica el plan pertinente, sin editar el antecedente.

## 3. Requisitos de la petición

**Confirmados:**

- CRUD completo de usuarios persistidos en SQLite.
- Asignación opcional de un usuario a una tarea; un usuario puede tener varias tareas.
- Sin login ni funcionalidad de autenticación.
- Mantener las funciones existentes de tareas y categorías.
- Integrar Serilog en el host backend, configuración y registros estructurados HTTP/fallos, con destino local y sin datos personales, secretos ni cuerpos.
- Completar lo que falte y validar lo ya escrito, sin rediseñar ni ampliar el producto.

**Decisiones de continuidad:** las antiguas propuestas siguientes ya están reflejadas en análisis y código; se conservan, no se reabren como bloqueos:

| Tema | Decisión de este plan | Estado |
|---|---|---|
| Datos del usuario | Solo `Id` y `Name`; sin email, apellidos separados ni metadatos. | Implementado; conservar. |
| Homónimos | Permitir nombres repetidos; la identidad es el ID, no el nombre. | Implementado; conservar. |
| Edición de asignación | Permitir asignar, cambiar y quitar el usuario en la edición de tareas. | Implementado; conservar. |
| Borrado en uso | Rechazar con 409 mientras tenga tareas; desasignar o reasignar primero. | Implementado; conservar. |
| Orden del catálogo | Nombre y, como desempate, ID. | Implementado; conservar. |
| Destino Serilog | Consola local, un evento JSON por línea. Sin fichero ni servicio externo. | Decisión directa de esta revisión. |

No hay decisiones funcionales verdaderamente bloqueantes. Quedan verificaciones técnicas (restauración de versión compatible, migración aplicada y resultados de pruebas), no preguntas de producto. No hace falta directorio externo ni datos iniciales.

Fuera de alcance: cuentas, sesiones, permisos, propietario autenticado de tareas, múltiples asignados, filtros por usuario, paginación, servicios externos, notificaciones y refactorización general de arquitectura. Tampoco dashboards, Seq/Elastic/Application Insights, auditoría de actividad personal, trazas de payload, nuevos frameworks de pruebas o entregables Word.

## 4. Modelo de datos y restricciones

Modelo ya presente que debe conservarse y verificarse:

| Entidad | Campo | Tipo | Restricción |
|---|---|---|---|
| `TodoUser` | `Id` | `int` | PK generada por SQLite. |
| `TodoUser` | `Name` | `string` | Obligatorio y no vacío tras `Trim()`. No único. |
| `TodoUser` | `TodoItems` | `ICollection<TodoItem>` | Navegación inversa, no se serializa como contrato HTTP. |
| `TodoItem` | `UserId` | `int?` | FK opcional a `TodoUser.Id`. |
| `TodoItem` | `User` | `TodoUser?` | Navegación opcional. |

- Conservar `DbSet<TodoUser> TodoUsers`, nombre requerido, relación uno a muchos con `DeleteBehavior.Restrict` e índice sobre `TodoItems.UserId`. No borrar tareas en cascada.
- Reutilizar `20261007100800_AddTodoUsers` y su snapshot/Designer existentes. No duplicar ni modificar migraciones aplicadas.
- La migración crea `TodoUsers`, añade `UserId` nullable y su FK. Todas las tareas previas quedan con `UserId = null`; no alterar títulos, estado, fechas ni categorías.
- El catálogo puede comenzar vacío. No sembrar usuarios ficticios ni un usuario por defecto.
- Aplicar la migración explícitamente mediante el flujo existente; el arranque actual no llama a `Database.Migrate()`.
- No añadir límites de longitud o unicidad no acordados. Si se decide alguno, reflejarlo primero aquí y después en DTO, servicio y UI.
- Serilog no añade tablas, relaciones ni migraciones. Consola no tiene persistencia entre sesiones ni rotación propia; no prometer un histórico. No se crean archivos de log y **no hace falta cambiar `.gitignore`** para la decisión elegida. Si después se solicita fichero, deberá revisarse este contrato antes: carpeta local ignorada, límite de tamaño, rotación y retención acotadas, permisos y exclusión de Git; no activar un sink File ahora.

## 5. DTOs y contratos de entrada/salida

Mantener los records ya implementados en [ApiDtos.cs](../backend/TodoApi/Dtos/ApiDtos.cs) y JSON camelCase:

| Contrato | Forma objetivo |
|---|---|
| `UserRequest` | `string? Name` |
| `UserResponse` | `int Id`, `string Name` |
| `TodoRequest` ampliado | Campos actuales y `int? UserId = null`, añadido al final para conservar argumentos posicionales existentes. |
| `TodoResponse` ampliado | Campos actuales y `int? UserId`, `string? UserName`. |
| Errores de negocio | `{ "message": "Mensaje en español" }`, siguiendo el patrón actual. |

Ejemplo de alta o actualización de usuario:

```json
{ "name": "Ana García" }
```

Ejemplo de alta de tarea con usuario:

```json
{
  "title": "Preparar informe",
  "isCompleted": false,
  "categoryId": null,
  "userId": 4
}
```

- POST con `userId` omitido o `null` crea una tarea sin asignar.
- PUT mantiene la semántica de sustitución actual: `userId` omitido o `null` deja la tarea sin asignar. No introduce semántica PATCH.
- Todo cliente que quiera conservar la asignación debe enviar el ID actual en cada PUT, incluido el botón de completar. Los clientes anteriores seguirán aceptándose, pero un PUT suyo sin este campo desasignará la tarea; documentar este efecto.
- Las respuestas de listar, obtener, crear y actualizar tareas deben incluir siempre ambos nuevos campos; para tareas sin asignar serán `null`.
- No devolver entidades con navegaciones completas ni crear ciclos de serialización.
- Mantener el comportamiento de `CreatedAt`: en alta se conserva el contrato existente; actualizar no modifica la fecha.
- Tipos React existentes: `TodoUser = { id: number; name: string }`; `TodoItem` incluye `userId: number | null` y `userName: string | null`.
- Los tipos React anteriores ya existen; no generar otra capa cliente. Serilog no modifica DTOs ni incluye entidades serializadas en eventos.
- Contrato de log seguro: `EventType`, `SourceContext`, `RequestMethod`, `RouteTemplate`, `StatusCode`, `ElapsedMs`, `TraceId` generado por el servidor y, solo en fallo inesperado, `ExceptionType`. `RequestMethod` se limita a verbos HTTP conocidos o al literal `OTHER`, sin copiar texto arbitrario. `RouteTemplate` será la plantilla de endpoint (por ejemplo `/api/users/{id:int}`), no la URL/Path real; para rutas sin endpoint usar el literal `unmatched`. No registrar IDs personales, nombres, títulos, categorías, querystring, cabeceras, cookies, cuerpos, conexiones, objetos request/response ni `Exception.Message`, `Exception.ToString()` o la excepción completa.

## 6. Endpoints y códigos HTTP

Endpoints ya implementados que deben verificarse en la Minimal API existente, sin crear controladores:

| Verbo | Ruta | Éxito | Errores esperados |
|---|---|---|---|
| GET | `/api/users` | 200 + array de `UserResponse`, incluido `[]`. | Fallos inesperados: 5xx. |
| GET | `/api/users/{id:int}` | 200 + `UserResponse`. | 404 si no existe. |
| POST | `/api/users` | 201 + usuario y `Location: /api/users/{id}`. | 400 por nombre nulo, vacío o solo espacios. |
| PUT | `/api/users/{id:int}` | 200 + usuario actualizado. | 404 si no existe; 400 por nombre inválido. |
| DELETE | `/api/users/{id:int}` | 204 sin cuerpo. | 404 si no existe; 409 si tiene tareas asignadas. |
| GET | `/api/todos` | 200 + tareas con datos de usuario. | Conservar comportamiento existente. |
| GET | `/api/todos/{id:int}` | 200 + tarea con datos de usuario. | 404 si no existe. |
| POST | `/api/todos` | 201 + tarea con datos de usuario. | 400 por usuario inexistente o por validaciones actuales. |
| PUT | `/api/todos/{id:int}` | 200 + tarea con datos de usuario. | 404 si no existe la tarea; 400 por usuario inexistente o validaciones actuales. |
| DELETE | `/api/todos/{id:int}` | 204. | 404 si no existe; no borra al usuario. |

- Un `userId` entero no nulo debe corresponder a un usuario existente; 0, negativos o IDs desconocidos producen 400 en alta/edición de tareas.
- JSON mal formado y tipos incompatibles se rechazan mediante el binding de ASP.NET Core, sin guardar datos.
- Mantener la prioridad actual: en PUT se comprueba primero la existencia del recurso y después los datos.
- No hay 401/403 de negocio porque no se introduce autenticación.
- Las rutas y códigos de categorías no cambian. Los nombres repetidos de usuario no producen 409 bajo este contrato.
- Fallos inesperados: 500 con `{ "message": "Se ha producido un error interno." }` si la respuesta no ha empezado, sin stack ni detalles SQLite; no convertirlos en 400/409 ni éxito. Si ya comenzó la respuesta, no escribir otro cuerpo/estado: registrar fallo y propagar/abortar según ASP.NET. Las cancelaciones por desconexión no son errores de negocio ni deben producir un falso 500.
- No añadir endpoint de logs ni ruta pública para provocar fallos. El registro no altera 201/Location, 204, 400, 404 ni 409.

## 7. Lógica de negocio y validaciones

Seguir el precedente local: conservar `TodoService`, sin introducir repositorios, interfaces o capas nuevas. Los puntos 1–10 siguientes ya tienen implementación; son contrato de regresión y se corrigen **solo si falla la validación**:

1. Conservar métodos `GetUsersAsync`, `GetUserByIdAsync`, `CreateUserAsync`, `UpdateUserAsync` y `DeleteUserAsync` y sus resultados explícitos.
2. Normalizar el nombre con `Trim()` y rechazar `null`, vacío y espacios antes de modificar entidades. Mensaje: «El nombre del usuario es obligatorio.».
3. Consultar usuarios ordenados por nombre e ID. Actualizar el nombre sin cambiar ID ni asignaciones.
4. En `CreateAsync` y `UpdateAsync`, resolver el usuario nullable y validar su existencia antes de añadir o modificar la tarea. Mensaje: «El usuario seleccionado no existe.».
5. En actualización, validar título, categoría y usuario antes de mutar cualquier campo. Una referencia inválida no puede cambiar título, estado, categoría ni asignación.
6. Guardar `UserId` y la navegación coherentemente. Al desasignar, establecer ambos a `null`.
7. Incluir usuario junto a categoría en `GetAllAsync` y `GetByIdAsync`; producir una respuesta completa también después de crear/editar. Un cambio de nombre debe verse en la siguiente consulta sin copiar nombres a la tabla de tareas.
8. Antes del borrado, comprobar `AnyAsync(todo => todo.UserId == id)`. Si está en uso, devolver 409 y el mensaje «No se puede eliminar un usuario que tiene tareas asignadas.».
9. La FK debe proteger también frente a una asignación concurrente entre la comprobación y el borrado. Traducir únicamente una violación SQLite de FK verificada y relacionada con esta operación al conflicto correspondiente; no capturar todos los errores de persistencia como si fueran validaciones. Una eliminación fallida no debe dejar una entidad pendiente de borrado reutilizable en el contexto.
10. Ante un usuario eliminado concurrentemente durante el guardado de una asignación, conservar atomicidad y devolver un error explícito de referencia inválida tras comprobar la causa. Otros fallos de base de datos deben seguir siendo errores de servidor registrados, nunca éxitos ni listas vacías.

La regla de borrado obliga a desasignar o reasignar antes de eliminar. La UI debe explicarlo; no hacer cambios implícitos a tareas.

**Serilog pendiente, solución mínima elegida:**

- Añadir una referencia directa `Serilog.AspNetCore` de línea compatible con .NET 10 (10.x), fijando una versión estable exacta tras verificar restore. Usar sus dependencias transitivas de configuración/consola y `Serilog.Formatting.Json.JsonFormatter`; no sumar sinks File/Async, enrichers ni `Serilog.Expressions`. Si el grafo restaurado exige una referencia para configuración o Console, añadir únicamente la imprescindible y documentar motivo/versión. No actualizar EF/.NET/React.
- Registrar Serilog con la integración de host disponible en esa versión, lectura de la sección `Serilog` y servicios, sustituyendo los providers de logging predeterminados para evitar duplicados. Configurar consola JSON, nivel base Information y overrides de framework prudentes, coherentes en `appsettings.json` y `appsettings.Development.json`. No habilitar sensitive data logging de EF.
- Usar un único middleware HTTP sencillo en `Program.cs` que emita la plantilla fija de finalización con los campos permitidos. No usar el evento predeterminado de `UseSerilogRequestLogging()` sin saneamiento: incluye Path real y puede adjuntar excepciones. Esta decisión evita filtros de datos a posteriori y nuevas abstracciones.
- Situarlo antes de CORS/HTTPS y de ejecución de endpoints; leer el endpoint resuelto al finalizar. Un evento de finalización por petición: Information para <400, Warning para 4xx, Error para 5xx/fallo inesperado. Medir duración con reloj monotónico. Correlacionar con identificador generado por ASP.NET, no copiar cabeceras de correlación arbitrarias.
- En el mismo límite HTTP capturar fallos inesperados, construir 500 genérico si es posible y emitir el evento Error con tipo de excepción, **sin pasar la excepción al logger**. No registrar dos veces el mismo fallo. Para respuesta iniciada, preservar la propagación requerida. Distinguir `OperationCanceledException` con `RequestAborted` para no clasificar una desconexión como fallo del servidor.
- Adoptar lista permitida de fuentes hacia el sink mediante el filtro por `SourceContext` de Serilog, sin paquete adicional: solo contextos propios `TodoApi.Http`/`TodoApi.Host` con plantillas fijas seguras. Excluir eventos de EF y diagnósticos de framework que contienen SQL, URLs, excepciones, rutas locales o payloads (incluidos los de Lifetime con ruta de contenido). Los eventos propios de host sustituyen el aviso de ciclo de vida. Los fallos de persistencia no traducidos se registran de forma segura en el límite HTTP; no se necesitan logs de servicio ni cambiar su constructor.
- Cubrir arranque/parada y fallo de host con mensajes fijos seguros y `ExceptionType` cuando proceda; disponer/cerrar el logger al terminar para vaciar el sink. Evitar DeveloperExceptionPage con detalles de errores en este contrato y comprobar tanto Development como no-Development.
- La consola es salida local sin archivos y sin envío externo. Si se redirige manualmente fuera de la app, la retención y seguridad de esa captura corresponden al operador; no incluirla en Git.

## 8. Capas afectadas y archivos clave

| Superficie | Archivos | Trabajo |
|---|---|---|
| Análisis y documentación | `docs/analisis-diseño.md`, `README.md`, este plan | Usuarios ya documentados; añadir registro seguro, configuración y límites de consola sin reescribir contenido ajeno. |
| Dominio/DTOs | `Models/TodoUser.cs`, `Models/TodoItem.cs`, `Dtos/ApiDtos.cs` | Terminados en código; conservar. |
| Datos | `Data/TodoDbContext.cs`, `Migrations/20261007100800_AddTodoUsers*`, snapshot | Conservar y verificar sobre copia aislada; no generar migración. |
| Reglas/servicio | `Services/TodoService.cs` | Terminado en código; corregir únicamente defectos demostrados. No inyectar logger por rutina. |
| HTTP/DI/configuración | `Program.cs`, `TodoApi.csproj`, ambos `appsettings*.json` | Usuarios terminados; integrar Serilog y límite seguro de errores. |
| Frontend | `frontend/src/App.tsx` | Verificar lo existente y cerrar bloqueo de envíos duplicados de tareas; conservar CSS, marca NEXO y HTML previos. |
| Tests | `backend/TodoApi.Tests/TodoServiceTests.cs`, tests SQLite adicionales si procede | Completar huecos sin reemplazar suite. SQLite ya llega por referencia al proyecto API; no requiere nuevo framework. |
| Gitignore | `.gitignore` | Revisado; N/A para consola. No editarlo sin introducir archivos de logs aprobados. |

Contrato de frontend: los puntos siguientes ya existen salvo el bloqueo de envíos duplicados de tareas, que falta. Verificarlos antes de cambiar código; no volver a construir catálogo/selectores:

- Conservar el mantenimiento de usuarios independiente del de categorías: listado, creación, edición con guardar/cancelar y borrado con confirmación.
- Cargar `/api/users` junto a los recursos actuales; distinguir errores de carga y permitir reintentar. No confundir una carga fallida con un catálogo vacío.
- Conservar selectores con opción «Sin asignar» en alta y edición; al editar, inicializar desde `userId`.
- En homónimos, mostrar nombre e ID en las opciones para diferenciarlos, sin prohibir nombres repetidos.
- Enviar `userId` en alta, edición y alternancia de completado. Limpiar borradores al guardar, cancelar o eliminar la tarea.
- Mostrar el usuario o «Sin asignar» en cada tarea. Tras renombrar usuario, actualizar sus nombres en tareas cargadas o recargar esos datos.
- Tras borrar un usuario sin tareas, retirar la opción y limpiar selecciones/borradores que apuntaban a él. Tras 409, conservar usuario y tareas.
- Si falla una operación, mantener el formulario y mostrar el error; actualizar listas solo tras una respuesta de éxito. Bloquear envíos duplicados mientras se guarda.
- Mantener etiquetas accesibles, mensajes `role="alert"` y diseño adaptable. No añadir librerías de formularios, router ni dependencias de UI.

Secuencia de dependencias: análisis → modelo → DTOs → persistencia → reglas y validaciones → servicio → HTTP → frontend → pruebas/verificación. Las reglas y la capa de servicio comparten el archivo existente; el orden no obliga a crear una arquitectura diferente.

**Pendientes concretos y orden de ejecución:**

1. Inventariar y preservar baseline/cambios actuales; confirmar que el delta autorizado no toca archivos ajenos.
2. Verificar migración/snapshot existentes y ampliar pruebas faltantes: recursos de usuario inexistentes, actualización inválida con asignación/categoría previas, persistencia con contexto nuevo y FK real.
3. Integrar Serilog, configuración JSON/host y tratamiento seguro de HTTP/fallos descritos arriba.
4. Añadir bloqueo de envíos duplicados en alta/guardar/completar tareas (estado de operación y botones deshabilitados; evitar segunda llamada mientras está pendiente), sin cambiar semántica PUT ni introducir librerías. Verificar preservación de borrador y feedback ante rechazo.
5. Ejecutar build/tests, prueba SQLite/migración, matriz HTTP, recorrido UI y controles de privacidad de logs; corregir solo incumplimientos.
6. Actualizar documentación técnica mínima de Serilog y dejar evidencias/delta frente a baseline. Sin commit/push.

**Riesgos y mitigación:** confundir código presente con funcionalidad probada (evidencias de sección 9); migración existente aún no aplicada (copia aislada y consulta de historial); InMemory oculta FK/carreras (SQLite); PUT de clientes antiguos desasigna (conservar y documentar contrato); diagnósticos de EF/framework o excepciones completas filtran datos (lista permitida y prueba de canarios); duplicación de providers/eventos (un solo pipeline); dependencia incompatible (restore/build sin actualizar stack); pérdida de cambios ajenos (delta por archivo, sin regeneración ni limpieza); consola sin histórico (limitación explícita, no sustituir por File sin petición).

## 9. Pruebas y criterios de aceptación

| Área | Comprobación verificable |
|---|---|
| CRUD usuarios | Crear normaliza espacios y devuelve ID; listar/obtener devuelve datos; editar conserva ID; borrar usuario sin tareas lo elimina; recursos inexistentes devuelven 404. |
| Nombre | Alta/edición rechazan nulo, vacío y espacios, sin persistir cambios. Homónimos se permiten bajo el contrato existente. |
| Asignación opcional | Crear con campo omitido o `null` funciona y devuelve `userId/userName = null`; catálogo vacío no impide crear tareas. |
| Asignación válida | Crear con usuario existente devuelve ID y nombre; consultar con un contexto nuevo confirma persistencia y navegación. |
| Edición | Asignar, cambiar y quitar usuario funciona; se mantienen fecha y categoría salvo cambios explícitos. PUT sin `userId` desasigna conforme al contrato. |
| Referencias inválidas | Alta/edición con ID desconocido, 0 o negativo devuelve 400 y no modifica ningún campo. |
| Borrado protegido | Usuario con tarea pendiente o completada devuelve 409; usuario y tareas permanecen. Tras desasignar todas sus tareas, se puede borrar. |
| Renombrado | La siguiente consulta de tareas y la UI muestran el nombre actualizado sin cambiar IDs ni perder asignaciones. |
| Regresión | CRUD de tareas/categorías funciona; completar/descompletar, editar título o categoría desde la UI conserva el usuario. Eliminar una tarea no borra usuario ni categoría. |
| Migración | Sobre una copia SQLite con datos previos, conserva número, IDs y campos anteriores de tareas/categorías; todas las tareas antiguas quedan sin usuario. Datos sobreviven al reinicio. |
| Errores y UX | Fallos de red, validación y conflicto son visibles; no se pierde el formulario ni se muestran cambios no confirmados. Cancelar no guarda. Selectores funcionan con teclado y en pantalla estrecha. |

Pruebas automatizadas: conservar y ampliar la suite de servicios con xUnit/FluentAssertions. EF InMemory sirve para lógica, pero **no demuestra** FK, migraciones, persistencia entre contextos relacionales ni HTTP. Usar SQLite aislada (conexión en memoria abierta durante la prueba o archivo temporal) y contextos nuevos para esos casos. La traducción de FK ya existe: comprobar error extendido 787, ausencia de borrado en cascada, contexto limpio tras fallo y propagación de errores no relacionados. Para simular carreras, interponer un interceptor EF de pruebas que modifique la referencia desde otra conexión justo antes del guardado, sin hooks en producción.

Huecos mínimos de suite: GET/PUT/DELETE de usuario inexistente; PUT de recurso ausente con entrada inválida mantiene prioridad 404; borrar usuario con tarea completada; validar referencia inválida preservando título/estado/fecha/categoría/asignación existentes; renombrado y navegación comprobados con contexto nuevo. No duplicar los casos ya cubiertos.

Validación que ejecutará el implementador, desde la raíz:

```powershell
dotnet build .\backend\TodoApi\TodoApi.csproj
dotnet test .\backend\TodoApi.Tests\TodoApi.Tests.csproj
dotnet ef migrations list --project .\backend\TodoApi\TodoApi.csproj
```

No generar ninguna migración. Aplicar la existente con `dotnet ef database update --project .\backend\TodoApi\TodoApi.csproj --connection "Data Source=<ruta-absoluta-copia-de-prueba>"`. Preparar también una SQLite aislada en `AddTodoCategories`, sembrar tareas/categorías y aplicar `AddTodoUsers`: comparar IDs, títulos, estados, fechas y categoría antes/después, con `UserId = null` en datos previos. Consultar `__EFMigrationsHistory`; conservar intacta `todos.db` de trabajo y sus WAL/SHM. Registrar falta de herramienta EF o restauración como bloqueo técnico de verificación, no como resultado aprobado.

Desde `frontend`:

```powershell
npm exec tsc -- --noEmit
npm run lint
npm run build
```

El script actual de build ejecuta solo Vite, por lo que el chequeo TypeScript debe ser explícito. No instalar dependencias salvo que falten al validar. No se exige añadir un runner frontend para esta iteración: comprobar manualmente los flujos anteriores y los códigos/cuerpos HTTP, incluido `Location` y ausencia de cuerpo en 204.

**Matriz mínima de Serilog, API y seguridad (pendiente de ejecución):**

| Caso | Criterio verificable |
|---|---|
| Arranque/parada | Host usa Serilog, consola produce JSON parseable y cierra correctamente; cada evento propio aparece una vez, sin providers duplicados. Verificar Development y no-Development. |
| HTTP | Probar 200/201/204, 400 por usuario inválido, 404 y 409 por borrado en uso: un evento de finalización con método, plantilla de ruta, estado, duración no negativa y TraceId; niveles según sección 7. Location y cuerpos preservados. |
| Fallo inesperado | En instancia de prueba con SQLite sin esquema provocar lectura fallida: 500 genérico, evento Error correlacionado con ExceptionType, sin excepción completa, SQL ni conexión. Sin cambiar producción ni añadir endpoint de fallo. Si se lanza sin esquema no tocar la SQLite real. |
| Privacidad | Usar canarios sintéticos diferentes en nombre/título, querystring, cabecera Authorization/cookie y JSON inválido. Buscarlos en toda la consola capturada: ninguno debe aparecer. Probar además ruta desconocida con canario en Path (se registra `unmatched`) y error de binding. Nunca emplear secretos reales. |
| Regresión de manejo de errores | 400/404/409 siguen siendo rechazos esperados, no 500; un fallo de persistencia no relacionado no se transforma en 409/éxito. Cancelación y respuesta iniciada no generan otro cuerpo ni doble evento de error. |
| Sin fichero | No se crea carpeta/archivo de logs, ni hay sink remoto/File, cambios de gitignore o datos generados en el delta. Consola sin persistencia/rotación propia queda documentada. |
| UI pendiente | Doble clic/envío lento en alta, edición y completar tarea no crea llamadas duplicadas; guardado fallido mantiene borrador; completar y recargar conserva UserId; usuario borrado/renombrado se refleja correctamente. |
| Preservación | Comparar delta final con baseline e inventario inicial, incluidos no seguidos: solo cambios autorizados y ningún cambio previo perdido; no commit/push. |

Cierre: todos los criterios aplicables deben pasar y sus resultados quedar registrados por el implementador/verificador. Este plan no certifica builds ni pruebas.

## 10. Skills a invocar

Catálogo contrastado con [skills-orquestacion.md](./skills-orquestacion.md) y [ARQUITECTURA-AGENTES.md](./ARQUITECTURA-AGENTES.md). La tabla indica el uso previsto durante implementación, no ejecución realizada por el planificador. Se emplean los nombres disponibles actualmente.

| Skill | Estado | Orden | Justificación |
|---|---|---:|---|
| `nueva-feature` | N/A | — | Alcance cerrado en este plan; sin agentes ni ciclo automático adicional. |
| `disenoanalisis` (catálogo: `diseño-analisis`) | Aplicable | 1 | Incorporar registro seguro al análisis ya actualizado; no regenerarlo. Nombre de carpeta real confirmado. |
| `modelo` | N/A | — | Entidad y FK ya implementadas; inspección/validación, no regeneración. |
| `dto` | N/A | — | Contratos completos, sin cambios funcionales nuevos. |
| `base-de-datos` | Aplicable, verificación | 4 | Verificar/aplicar migración existente en SQLite aislada; no crear otra. |
| `logica-negocio` | N/A | — | Reglas presentes en servicio; no hay nueva regla ni capa a crear. |
| `validaciones` | Aplicable, verificación | 6 | Revisar atomicidad, referencia válida y fallos FK; corregir solo defectos demostrados. |
| `servicio` | N/A | — | CRUD y mapeos existentes; logging central no exige modificar constructor. |
| `controlador` | Aplicable, adaptado | 8 | Integración host/configuración y límite HTTP seguro en Program.cs Minimal API, nunca controllers. Serilog no tiene skill propio en catálogo. |
| `ui-ux-pro-max` | N/A | — | No se solicita rediseño; mantener UI existente y accesibilidad. |
| `frontend-react` | Aplicable, cierre mínimo | 10 | Proteger envíos de tareas y verificar CRUD/selectores ya implementados. |
| `tests-unitarios` | Aplicable | 11 | Completar huecos y SQLite con stack actual; además validación HTTP/logs manual. |
| `actualizar-documentacion` | N/A | — | Figura en el catálogo documental, pero no está disponible como skill invocable. Actualizar manualmente análisis/README en los pasos correspondientes. |
| `manual-usuario` | N/A | — | No se solicita entregar ni editar el manual Word en este encargo. |
| `docx`, `pdf`, `pptx`, `xlsx` | N/A | — | Sin entregables ofimáticos. |
| `commit-message` / `commit-message-authoring` | N/A | — | Nombre de catálogo/carpeta real; no commit/push en este encargo. |
| `project-audit`, `security-review`, `skill-scanner` | N/A | — | No se solicita una auditoría transversal ni revisión de skills. |
| `csharp-refactoring` | N/A | — | Es una feature, no una refactorización; conservar arquitectura. |
| `ponytail` | N/A | — | No está en catálogo ni carpeta local inspeccionados; la simplicidad se exige sin invocar un skill inexistente. |
| Resto de skills disponibles | N/A | — | No hay cambios de SDK, Python, Cosmos DB, agentes Foundry ni despliegue cloud. |

Las adaptaciones de lógica, servicio y controlador son necesarias porque el catálogo describe una arquitectura genérica distinta del código real. La dependencia entre responsabilidades se conserva sin imponer nuevas carpetas o capas.
