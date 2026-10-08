# app-todolist-agents

Plugin de GitHub Copilot que empaqueta el **equipo de agentes** y los **skills propios** de este
repositorio para desarrollar la aplicación de lista de tareas
(.NET 10 · ASP.NET Core · EF Core · SQLite · React · TypeScript + Vite).

El plugin usa el formato **legacy** de plugins de Copilot: un manifiesto `plugin.json` en la raíz
que apunta a los componentes en `agents/` y `skills/`.

## Qué incluye

### Agentes (`agents/`)

| Agente | Para qué sirve |
|---|---|
| `planificador-apptodolist` | Analiza la petición y genera un plan en `docs/` sin tocar código de producción. |
| `desarrollador-apptodolist` | Implementa la funcionalidad en backend y frontend siguiendo la arquitectura. |
| `verificador-apptodolist` | Compara la implementación con el plan y devuelve APROBADO o REVISAR. |
| `qa-apptodolist` | Ejecuta pruebas E2E con Playwright y captura pantallas para el manual. |
| `actualizador-manual-apptodolist` | Actualiza el manual de usuario en Word con las funciones reales. |
| `orquestador-apptodolist` | Coordina planificación, implementación y verificación; hace commit/push tras aprobación. |
| `github-issue-planner` | Convierte un plan en issues de GitHub con labels y milestone. |
| `prompt-engineer` | Ayuda a crear prompts de desarrollo claros y verificables. |

### Skills (`skills/`)

Skills por capa, alineados con la arquitectura del proyecto:

- `disenoanalisis` — documento de análisis y diseño.
- `modelo` — clases del modelo de dominio.
- `dto` — DTOs de entrada/salida de la API.
- `base-de-datos` — `AppDbContext`, EF Core con SQLite y migraciones.
- `logica-negocio` — interfaces e implementaciones de lógica de negocio.
- `validaciones` — validaciones de entrada y reglas de negocio.
- `servicio` — capa de servicios.
- `controlador` — controladores ASP.NET Core.
- `frontend-react` — frontend React + Vite + TypeScript.
- `nueva-feature` — implementa una feature de punta a punta por todas las capas.
- `tests-unitarios` — pruebas unitarias y huecos de cobertura.
- `project-audit` — auditoría del estado general del proyecto.
- `manual-usuario` — manual de usuario en Word (.docx).
- `commit-message-authoring` — mensajes de commit claros y específicos.

> Nota: los skills genéricos de terceros del repositorio (`docx`, `pdf`, `pptx`, `xlsx`,
> `security-review`, `skill-scanner`, `ui-ux-pro-max`) **no** se incluyen en este plugin por ser
> bundles externos; instálalos por separado si los necesitas.

## Estructura

```text
app-todolist-agents/
├── plugin.json        # Manifiesto (formato legacy)
├── README.md
├── agents/            # 8 agentes *.agent.md
└── skills/            # 14 skills <nombre>/SKILL.md
```

## Instalación

Con GitHub Copilot CLI, desde una ruta local:

```bash
copilot plugin install ./dist/app-todolist-agents
```

O directamente desde el repositorio (subdirectorio del plugin):

```bash
copilot plugin install hispafox/app-todolist-261005:dist/app-todolist-agents
```

Comprueba y gestiona la instalación con:

```bash
copilot plugin list
copilot plugin enable app-todolist-agents
copilot plugin disable app-todolist-agents
```

## Licencia

MIT.
