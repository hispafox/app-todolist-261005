# Plan: asignación de tareas a usuarios

## Objetivo

Permitir seleccionar y visualizar un usuario asignado a una tarea, conservar la asignación en SQLite y rechazar referencias a usuarios inexistentes, sin añadir autenticación ni gestión de usuarios que no haya sido aprobada.

Este documento planifica el cambio; no implementa código.

## Estado actual confirmado

- La aplicación usa ASP.NET Core Minimal API, EF Core y SQLite en el backend, y React + TypeScript + Vite en el frontend.
- No hay modelo, tabla, DTO, servicio, endpoint ni tipo de frontend para usuarios. El análisis del producto también excluye actualmente la gestión de usuarios.
- `TodoItem` incluye `CategoryId` y una navegación opcional a categoría. La solicitud y respuesta de tareas exponen `categoryId` y `categoryName`.
- Las tareas se crean y editan mediante `POST /api/todos` y `PUT /api/todos/{id}`. La API valida la existencia de la categoría y responde con un error 400 claro si no existe.
- Las tareas ya almacenadas no tienen usuario asignado. La aplicación carga tareas y categorías desde la API y ofrece formularios de creación y edición.
- El modelo de asignación pedido es de como máximo un usuario por tarea; no se plantea una relación de varios usuarios por tarea.

## Requisitos confirmados por la solicitud

- Persistir la asignación y conservarla al recargar la aplicación.
- Permitir seleccionar y visualizar el usuario asignado en la interfaz.
- Rechazar referencias a usuarios inexistentes con un error claro.
- Mantener coherentes persistencia, API y frontend.
- No alterar las funciones actuales de tareas ni añadir autenticación o gestión de usuarios fuera del alcance que se confirme.

## Decisiones pendientes antes de implementar

Estas decisiones no están especificadas en la solicitud y no deben tratarse como requisitos confirmados:

| Decisión | Recomendación para confirmar | Motivo |
|---|---|---|
| ¿La asignación es opcional u obligatoria? | **Opcional.** Las tareas existentes se mantienen sin asignar; también permite crear tareas sin depender de un catálogo inicial. | Es compatible con los datos actuales y reduce el riesgo de una migración con backfill. |
| ¿Se puede cambiar o quitar la asignación? | **Sí.** Permitir cambiarla en la edición y quitarla enviando `null`/sin selección. | Mantiene simple el contrato de edición y permite corregir la asignación. |
| ¿Qué fuente de usuarios utilizará el selector? | Confirmar una fuente/directorio ya existente y su contrato. Si no existe, aprobar explícitamente un catálogo mínimo de usuarios y las operaciones necesarias para consultarlo. | El repositorio no contiene usuarios; sin una fuente de opciones no puede ofrecerse un selector funcional. |
| ¿Qué ocurre al eliminar un usuario asignado? | **Rechazar el borrado mientras existan tareas asignadas** y devolver un error 409; no hay que borrar ni reasignar tareas implícitamente. | Protege la integridad y coincide con el comportamiento de categorías en uso. Requiere que exista una operación de borrado de usuario, que hoy no existe. |

La recomendación de asignación opcional es la opción más compatible con el estado actual, pero no sustituye la confirmación de quien solicita el cambio. Si se decide obligatoriedad, deberán definirse el usuario inicial para tareas existentes y la fuente de usuarios antes de crear una migración obligatoria.

## Alcance técnico previsto

### Persistencia y dominio

1. Una vez confirmada la fuente de usuarios, representar la referencia de usuario en el modelo y en EF Core.
2. Si la asignación es opcional, añadir una clave foránea nullable en la tarea y relación de uno a muchos: un usuario puede tener varias tareas asignadas y cada tarea cero o un usuario.
3. Si se aprueba una fuente local de usuarios, incluir su entidad y configuración mínima; no añadir roles, credenciales ni autenticación.
4. Crear una migración SQLite que conserve las tareas existentes como no asignadas cuando la relación sea opcional.
5. Aplicar la política acordada de eliminación mediante la clave foránea y la operación de servicio. La recomendación es restringir el borrado de un usuario en uso y evitar cascadas.

### API y lógica de negocio

1. Extender el contrato de creación/edición de tarea con el identificador de usuario asignado y el de respuesta con los datos necesarios para visualizarlo (identificador y nombre, si el contrato de la fuente los ofrece).
2. Validar el identificador en el servicio antes de persistir. Para un usuario inexistente, responder 400 con un mensaje claro, siguiendo el patrón ya usado para una categoría inexistente.
3. Incluir los datos del usuario al consultar, crear o editar tareas, para que las respuestas representen la asignación persistida.
4. Exponer solamente la lectura mínima del catálogo que la interfaz necesite, y únicamente si se aprueba la fuente local. Si se confirma una API/directorio externo ya existente, usar su contrato y no duplicar la gestión.
5. Mantener las rutas y semántica de las operaciones actuales de tareas.

### Frontend

1. Ampliar los tipos TypeScript de tarea y usuario de acuerdo con el contrato confirmado.
2. Cargar las opciones de usuario desde la fuente aprobada y presentar un selector en el formulario de alta y edición.
3. Mostrar el usuario asignado en cada tarea y una representación explícita de “sin asignar” cuando la asignación sea opcional.
4. Enviar la asignación al crear/editar y permitir cambiarla o limpiarla según la decisión confirmada.
5. Mostrar los errores devueltos por la API sin actualizar localmente la tarea como si el guardado hubiese tenido éxito.

### Documentación y pruebas

- Actualizar `docs/analisis-diseño.md` para reflejar la decisión final, el modelo, los contratos y los casos de eliminación acordados.
- Extender las pruebas del servicio/API y la validación del frontend que ya existan; no cambiar pruebas o flujos de categorías salvo donde la integración lo requiera.

## Secuencia propuesta

1. Resolver las cuatro decisiones pendientes, especialmente la fuente de usuarios y si la asignación es opcional.
2. Actualizar el análisis y fijar el contrato de usuario/tarea antes de editar código.
3. Implementar el modelo y la migración compatibles con los datos existentes.
4. Implementar validación y respuesta de la API, incluidos los datos de usuario en todas las lecturas y mutaciones pertinentes.
5. Conectar el selector y la visualización en React, conservando intactas las acciones existentes.
6. Ejecutar las pruebas dirigidas y las comprobaciones de compilación descritas abajo.

## Validación prevista

### Backend y datos

- Crear una tarea sin usuario (si se confirma asignación opcional) y comprobar que sigue guardándose.
- Crear una tarea con un usuario existente; comprobar que la respuesta incluye la asignación y que sigue presente tras volver a leerla.
- Intentar crear y editar con un identificador inexistente; comprobar respuesta 400 con mensaje claro y que no se persiste un cambio parcial.
- Cambiar y quitar la asignación, si se confirma ese comportamiento.
- Aplicar la migración a una base de datos con tareas previas y verificar que no se pierden; comprobar que quedan sin asignar cuando la asignación sea opcional.
- Si se incorpora borrado de usuarios, comprobar que un usuario asignado no se puede borrar y que las tareas permanecen intactas.

### Operaciones existentes afectadas

- Crear, listar, obtener, editar, completar y eliminar tareas.
- Verificar que alternar el estado de completado no borra ni cambia la asignación.
- Verificar que las operaciones de categorías continúan funcionando y que ni la validación ni el borrado de categorías se ven afectados.

### Frontend

- Verificar carga y visualización del usuario asignado al abrir/recargar la aplicación.
- Verificar selección al crear, cambio al editar y eliminación de la selección si se confirma que puede quitarse.
- Verificar el feedback ante fallos de carga/guardado y que la interfaz no muestra un resultado no confirmado por la API.
- Ejecutar el build de producción de Vite/TypeScript.

## Validación ejecutada

No se ha ejecutado build ni pruebas: este entregable es únicamente el plan solicitado. La inspección del repositorio sí confirmó que no existe contrato ni modelo de usuarios y que las tareas/categorías usan los patrones descritos arriba.
