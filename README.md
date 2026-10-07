# Todo App con .NET 10, SQLite y React + TypeScript

Este proyecto es una aplicación full-stack de lista de tareas diseñada para demostrar el uso de GitHub Copilot en un entorno moderno de desarrollo. La solución combina:

- Backend en .NET 10 con ASP.NET Core Web API
- Base de datos SQLite
- Entity Framework Core para acceso a datos
- Frontend en React + TypeScript con Vite
- Arquitectura simple y escalable para aprender conceptos reales de desarrollo

## Objetivo

El objetivo de esta app es permitir:

- Crear tareas
- Marcar tareas como completadas
- Editar tareas
- Eliminar tareas
- Organizar tareas con categorías opcionales
- Mantener un catálogo local de usuarios y asignarlos opcionalmente a tareas
- Consultar la lista de tareas desde una interfaz web

Es una base muy útil para practicar integración entre frontend y backend, manejo de persistencia, migraciones y despliegue local.

## Agente local para crear prompts

El agente [Prompt Engineer](.github/agents/prompt-engineer.agent.md) está disponible solo en este proyecto. Selecciónalo en el selector de agentes de GitHub Copilot Chat en VS Code y describe tu encargo.

Comprueba los cuatro pilares **Rol, Contexto, Tarea y Formato** y pregunta, una cuestión por turno, hasta completar la información necesaria. Después entrega un prompt optimizado listo para copiar al planificador o al implementador. Puedes empezar con: «Necesito ahora categorías para las tareas».

Es un agente conversacional con `tools: []`: no lee el repositorio por su cuenta, no modifica archivos ni ejecuta comandos. Adjunta los contratos o fragmentos relevantes si necesita conocer el estado actual del código.

## Agente local para planificar una funcionalidad

El agente [Planificador AppTodoList](.github/agents/planificador-apptodolist.agent.md) analiza la petición, cruza el contexto del repositorio y genera un documento de planificación en `docs/` antes de que se implemente nada. Selecciónalo en GitHub Copilot Chat y entrégale la petición del cambio; devolverá un plan con alcance, decisiones, capas afectadas y skills necesarios.

Su trabajo es escribir `docs/plan-<slug>.md` sin tocar código de producción ni ejecutar comandos. Lee primero el plano del equipo y la orquestación de skills, y después redacta el plan con las dependencias reales del proyecto.

## Agente local para implementar la funcionalidad

El agente [Desarrollador AppTodoList](.github/agents/desarrollador-apptodolist.agent.md) toma el plan generado por el planificador y lo convierte en cambios reales en el proyecto. Trabaja sobre backend y frontend, valida con compilación y corrige errores de implementación sin hacer commit ni push.

Debe seguir la arquitectura del repositorio, respetar el alcance del plan y mantener el contrato entre API y UI. Es el responsable de que la solución compile y cumpla las decisiones del documento de planificación.

## Agente local para verificar una implementación

El agente [Verificador AppTodoList](.github/agents/verificador-apptodolist.agent.md) compara la implementación con un plan concreto y devuelve **APROBADO** o **REVISAR** con evidencias. Invócalo indicando la ruta del plan; si no se la das, te la pedirá.

Puede leer el código y ejecutar builds o pruebas pertinentes, pero no tiene permiso para editar archivos. Señala los fallos y sugiere cómo corregirlos; las correcciones corresponden al desarrollador.

## Agente local para coordinar el ciclo de desarrollo

El agente [Orquestador AppTodoList](.github/agents/orquestador-apptodolist.agent.md) coordina el planificador, el desarrollador y el verificador. Invócalo con una petición de funcionalidad para iniciar el ciclo de planificación, implementación y verificación; solo crea el commit y hace push si el verificador termina con **APROBADO**.

El flujo normal trabaja en `main` y no crea issues, ramas ni pull requests. Si no estás en `main`, o hay cambios ajenos que no se puedan separar con seguridad, el orquestador se detiene sin publicar.

## Agente local para QA con Playwright

El agente [QA AppTodoList](.github/agents/qa-apptodolist.agent.md) ejecuta pruebas de extremo a extremo con Playwright sobre la aplicación en marcha (backend + frontend), comprueba los flujos de la interfaz (crear, completar, editar y eliminar tareas) y genera capturas de pantalla para el manual de usuario en `docs/capturas/`. Devuelve un veredicto **PASA** o **FALLA** con evidencias y no modifica el código ni hace commits.

Está fuera del ciclo obligatorio del orquestador: lo invocas cuando quieres una verificación E2E o un juego de capturas. La suite vive en `frontend/e2e/` y se lanza con `npm run e2e` desde `frontend` (la configuración arranca el backend y el frontend automáticamente).

## Agente local para actualizar el manual de usuario

El agente [Actualizador del Manual AppTodoList](.github/agents/actualizador-manual-apptodolist.agent.md) mantiene al día el manual de usuario en Word (`docs/manual-usuario.docx`) con las funciones reales de la aplicación e inserta las capturas generadas por el agente de QA (`docs/capturas/`) en su sección correspondiente, con un diseño corporativo cuidado.

Se apoya en las skills [manual-usuario](.github/skills/manual-usuario/SKILL.md) y [docx](.github/skills/docx/SKILL.md). Está fuera del ciclo del orquestador, documenta sin tocar código y no hace commit salvo que se lo pidas. Si faltan capturas, pide primero al agente de QA que las regenere.

## Agente local para crear issues desde un plan

El agente [GitHub Issue Planner](.github/agents/github-issue-planner.agent.md) convierte un plan de `docs/` en uno o varios issues del repositorio. Selecciónalo en GitHub Copilot Chat e indica la ruta, por ejemplo: «Prepara los issues de `docs/plan-asignacion-tareas-usuarios.md`».

Lee el plan, consulta las labels y milestones existentes y comprueba duplicados. Antes de publicar, muestra los títulos, cuerpos, dependencias y metadatos para que apruebes la propuesta. No inventa decisiones pendientes ni crea labels o milestones nuevos sin aprobación. Solo asigna responsables si lo solicitas explícitamente.

Cada propuesta incluye un HTML autónomo en `docs/issue-previews/`, con tarjetas de issues, labels, milestone, dependencias, bloqueos y cuerpos íntegros desplegables. Puedes ver el diseño en [la plantilla ilustrativa](.github/agents/templates/github-issue-proposal.html). La aprobación se da en el chat sobre una revisión concreta; abrir el HTML no publica nada. Si cambia la propuesta, genera otra revisión y vuelve a solicitar aprobación.

Necesita GitHub CLI (`gh`) autenticada y permisos para crear issues (y metadatos, si apruebas crearlos). Si la autenticación o el sandbox impiden acceder a GitHub, informa del bloqueo. Después de publicar verifica los issues y devuelve sus enlaces. Solo crea el artefacto HTML local: no implementa el plan, no modifica código ni hace commits.

## Stack tecnológico

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- React
- TypeScript
- Vite
- C#
- TSX / TypeScript

## Estructura del proyecto

```text
app-todolist/
├── backend/
│   ├── TodoApi/
│   │   ├── Controllers/
│   │   ├── Data/
│   │   ├── Migrations/
│   │   ├── Models/
│   │   ├── Properties/
│   │   ├── appsettings.json
│   │   ├── Program.cs
│   │   └── TodoApi.csproj
│   └── TodoApi.sln
├── frontend/
│   ├── src/
│   ├── index.html
│   ├── package.json
│   ├── tsconfig.json
│   ├── tsconfig.node.json
│   ├── vite.config.ts
│   └── README.md
├── README.md
└── labs/
    └── ...
```

## Funcionalidades principales

- API REST para gestión de tareas
- CRUD de categorías y usuarios locales (sin autenticación)
- Persistencia con SQLite
- Entity Framework Core con migraciones
- Frontend React para consumir la API
- Interfaz intuitiva para gestionar tareas diarias
- Diseño ideal para practicar con GitHub Copilot en flujos reales de trabajo

## Requisitos previos

Antes de ejecutar el proyecto necesitas tener instalado:

- .NET 10 SDK
- Node.js 18+
- npm o yarn
- Git

Puedes comprobarlo con:

```bash
dotnet --version
node --version
npm --version
```

## Configuración del backend

1. Dirígete a la carpeta del backend:

```bash
cd backend/TodoApi
```

2. Instala los paquetes necesarios:

```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
```

3. Crea el contexto de base de datos y las entidades.

4. Genera la migración:

```bash
dotnet ef migrations add InitialCreate
```

5. Aplica la migración a SQLite:

```bash
dotnet ef database update
```

## Configuración del frontend

1. Crea la aplicación React con TypeScript:

```bash
cd ../..
cd frontend
npm create vite@latest . -- --template react-ts
```

2. Instala dependencias:

```bash
npm install
```

3. Instala Axios para consumir la API si se necesita:

```bash
npm install axios
```

## Ejecución del proyecto

### Backend

```bash
cd backend/TodoApi
dotnet run
```

La API queda disponible en:

```text
http://localhost:5062
https://localhost:7250
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

El frontend estará disponible en:

```text
http://localhost:5173
```

El proxy de Vite redirige las peticiones `/api` al backend en `http://localhost:5062`.

## Modelo de datos y API

El modelo de tareas, categorías, contratos de la API, requisitos funcionales y decisiones de diseño se mantienen en un único documento: [docs/analisis-diseño.md](docs/analisis-diseño.md).

## Registro del backend

ASP.NET Core usa Serilog para emitir eventos JSON estructurados por la consola local. Solo se aceptan eventos de host y peticiones de la aplicación; las rutas se registran como plantillas y no se incluyen SQL, excepciones completas, datos de tareas o usuarios, querystrings, cabeceras ni cuerpos. Los fallos inesperados reciben una respuesta 500 genérica cuando es posible. No se crean archivos de log ni se envían eventos a servicios externos; la consola no ofrece historial ni rotación propia.

## Flujo de trabajo recomendado

1. Definir el modelo de datos.
2. Crear el DbContext y configurar SQLite.
3. Añadir migraciones con Entity Framework.
4. Implementar la API REST en ASP.NET Core.
5. Consumir la API desde React.
6. Validar errores y mejorar la UX.
7. Probar el flujo completo de tareas.

## Buenas prácticas aplicadas

- Separación clara entre backend y frontend
- Persistencia con base de datos real
- Uso de migraciones para versionar el esquema
- Comunicación segura entre frontend y API
- Código limpio y mantenible para practicar con Copilot
- Frontend tipado con TypeScript para mejorar seguridad y mantenibilidad

## Posibles mejoras futuras

- Autenticación y autorización
- Filtros por estado de tarea
- Ordenado por prioridad o fecha
- Cambiar la base de datos a PostgreSQL o MySQL
- Añadir tests unitarios e integración
- Mejorar la UI con estilos personalizados

## Conclusión

Este proyecto es un ejemplo práctico de una aplicación moderna de lista de tareas que combina backend robusto, base de datos relacional y frontend dinámico. Sirve como base ideal para aprender arquitectura de software, buenas prácticas de desarrollo y uso de GitHub Copilot como asistente de programación.

## Documentación del producto

El análisis, diseño y requisitos consolidados de la aplicación están en [docs/analisis-diseño.md](docs/analisis-diseño.md). Los documentos fuente anteriores se conservan en [docs/archivo/](docs/archivo/).

### Manual de usuario en Word

El skill [manual-usuario](.github/skills/manual-usuario/SKILL.md) automatiza la creación o actualización del manual en español, basándose en las funcionalidades realmente implementadas y reutilizando el skill [docx](.github/skills/docx/SKILL.md).

Para ejecutarlo, pide a Copilot: «Usa manual-usuario para crear el manual de la aplicación en Word». La salida predeterminada es `docs/manual-usuario.docx`; también puedes indicar otra ruta. El flujo incluye comprobación del contenido y validación del documento generado.

## Licencia

Este proyecto se puede usar como base educativa. Si lo adaptas o lo compartes, te recomendamos mantener la referencia al original y documentar los cambios realizados.
