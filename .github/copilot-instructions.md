# Instrucciones de GitHub Copilot para este proyecto

## Visión general del proyecto

Este repositorio es una aplicación de lista de tareas (to-do app) construida con:

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- React + Vite
- JavaScript / JSX

El objetivo principal es mantener una solución simple, clara y fácil de seguir para aprender y desarrollar con GitHub Copilot sin introducir complejidad innecesaria.

## Principios generales

- Prioriza soluciones simples, legibles y mantenibles.
- Evita abstracciones innecesarias, patrones complejos o dependencias extra no justificadas.
- Mantén la arquitectura de la app clara: backend y frontend separados.
- Usa nombres descriptivos y consistentes en español o en el idioma que ya esté establecido en el proyecto.
- Cuando haya varias soluciones válidas, elige la más directa y menos frágil.
- No introduzcas overengineering para una app de esta escala.
- Para la creación y publicación del repositorio, usa GitHub CLI (`gh`) siempre que sea posible: `gh auth login`, `gh auth status`, `gh repo create`, `gh repo view` y `gh repo set-default` para mantener el flujo de trabajo consistente con GitHub.


## Idioma y estilo de respuesta

- Responde siempre en español si no se indica lo contrario.
- Explica soluciones en un tono claro, técnico y práctico.
- Haz comentarios solo cuando aporten valor real; evita comentarios redundantes.
- Usa nombres de variables, métodos, clases y archivos que expresen intención.

## Convenciones del backend (.NET / C#)

- Usa C# moderno y sintaxis clara y idiomática.
- Sigue convenciones de PascalCase para tipos, propiedades y métodos públicos.
- Usa camelCase para variables locales y parámetros.
- Empieza los nombres de los modelos con entidad clara, por ejemplo `TodoItem`, `TodoCreateRequest`, `TodoUpdateRequest`.
- Evita duplicar lógica; extrae utilidades pequeñas y reutilizables cuando realmente ayuden.
- Para la API REST, usa controladores o endpoints bien organizados y rutas consistentes:
  - `GET /api/todos`
  - `GET /api/todos/{id}`
  - `POST /api/todos`
  - `PUT /api/todos/{id}`
  - `DELETE /api/todos/{id}`
- Usa `async`/`await` en operaciones de acceso a datos.
- Usa `DbContext` y Entity Framework Core para el acceso a SQLite.
- No escribas SQL manual si el ORM puede resolverlo.
- Maneja errores de validación y de base de datos con mensajes claros y consistentes.
- Mantén el modelo de datos simple: `Id`, `Title`, `IsCompleted`, `CreatedAt` (o equivalente).
- Al crear migraciones, sigue la estructura del proyecto y no mezcles cambios no relacionados.

## Convenciones del frontend (React + Vite)

- Usa componentes funcionales con hooks.
- Mantén las partes de UI separadas por responsabilidad.
- Nombrea los componentes con PascalCase y los archivos con nombre descriptivo.
- Usa `useState`, `useEffect` y patrones simples para manejar el estado local.
- Las llamadas a la API deben hacerse de forma consistente, preferiblemente con `fetch` o `axios`.
- Mantén los datos del formulario y de la lista en un flujo sencillo y fácil de depurar.
- Evita dependencias pesadas si una solución nativa es suficiente.
- Si hay validación de formularios, hazla simple y útil.

## Estructura recomendada

- Backend:
  - `Controllers/`
  - `Data/`
  - `Models/`
  - `Program.cs`
  - `appsettings.json`
- Frontend:
  - `src/`
  - `public/`
  - `package.json`
  - `vite.config.*`
  - `index.html`

La estructura debe ser clara y predecible, sin introducir capas innecesarias.

## Reglas de calidad y validación

- Mantén el código bien formateado y consistente con el estilo del proyecto.
- No generes código muerto ni funcionalidades no pedidas.
- Añade validación básica para las entradas del usuario.
- Cuando cambies la API, asegúrate de que el frontend sigue funcionando con los mismos contratos.
- Para cambios importantes, valida con compilación o pruebas relevantes antes de dar por terminado el trabajo.
- En el backend, usa `dotnet build` cuando proceda.
- En frontend, usa `npm install` y `npm run build`/`npm run dev` para verificar el flujo más habitual.

## Reglas de negocio para la app de tareas

- Los usuarios deben poder:
  - listar tareas
  - crear tareas
  - marcar tareas como completadas
  - editar tareas
  - eliminar tareas
- La UI debe ser intuitiva y clara.
- La persistencia debe mantenerse real y fiable con SQLite/Entity Framework.
- Las tareas deben seguir una lógica sencilla y predecible: estado, texto y fecha de creación.

## Reglas de seguridad y prudencia

- Nunca subas secretos, claves, tokens, cadenas de conexión ni archivos `.env` con datos sensibles.
- No hagas suposiciones de seguridad que no sean necesarias para un proyecto de aprendizaje.
- Si se requiere autenticación o autorización, impleméntala de forma mínima y explícita.
- Cuando el flujo del proyecto implique crear o publicar un repositorio, usa la GitHub CLI (`gh`) para autenticar y validar la conexión antes de hacer push o crear repositorios remotos.
- Recomendación de uso: `gh auth login` para iniciar sesión, `gh auth status` para comprobar autenticación y `gh repo create` para generar repositorios públicos o privados según corresponda.

## Reglas de contribución

- Haz cambios pequeños y enfocados.
- Si el cambio afecta a backend y frontend, mantén ambos lados consistentes.
- Resuelve un problema por vez y no mezcles refactors no relacionados con la tarea.
- Cuando propongas una solución, explica el porqué del enfoque elegido.

## Actividad esperada de Copilot

Cuando trabajes en este repositorio, GitHub Copilot debería:

- sugerir soluciones alineadas con .NET 10 + React + SQLite
- mantener la app simple y práctica
- evitar complejidades no requeridas
- respetar el idioma del proyecto y la estructura del repositorio
- sugerir cambios coherentes entre backend y frontend
- preferir buenas prácticas reales pero sin sobre-diseñar

## Resumen breve

Este proyecto se desarrolla con una mentalidad de aprendizaje práctico: funcional, claro, directo y sostenido por buenas prácticas sin exagerar. La prioridad es que la app funcione bien y sea fácil de entender, tanto para humanos como para Copilot.
