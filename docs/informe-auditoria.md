# Informe de auditoría del proyecto

**Fecha:** 6 de octubre de 2026  
**Estado general:** parcialmente alineado

## Resumen ejecutivo

La solución sigue el stack previsto (.NET 10, ASP.NET Core, Entity Framework Core, SQLite y React + TypeScript) y contiene operaciones de tareas y categorías en ambas capas. La principal deuda está en las instrucciones de arranque y algunos riesgos de consistencia y validación.

## Hallazgos principales

| Prioridad | Hallazgo | Impacto y recomendación |
|---|---|---|
| **P2 · Media** | Las instrucciones de [README.md](../README.md) parecen de creación inicial: indican añadir dependencias y generar la aplicación Vite y la migración inicial, aunque esos elementos ya existen. Además, describen `backend/TodoApi.sln`, pero la solución presente es `TodoApp.slnx`. | Puede confundir el setup o inducir a modificar un proyecto ya configurado. Actualizar los pasos para restaurar dependencias, aplicar las migraciones existentes y ejecutar la solución actual. |
| **P2 · Media** | El [README principal](../README.md) y el [README del frontend](../frontend/README.md) indican Node.js 18+, pero el frontend declara Vite 8 y `@vitejs/plugin-react` 6. Las versiones instaladas requieren Node **20.19+ o 22.12+**. | En Node 18 la instalación o ejecución puede fallar. Actualizar el requisito documentado; el entorno auditado usa Node 22.20. |
| **P2 · Media** | La unicidad de nombres se compara con `StringComparison.OrdinalIgnoreCase` en `backend/TodoApi/Services/TodoService.cs`, mientras SQLite usa la colación `NOCASE` en `backend/TodoApi/Data/TodoDbContext.cs`. La comparación `NOCASE` de SQLite no cubre todos los caracteres Unicode como la comparación .NET. | El índice único puede no preservar exactamente la regla documentada para nombres con caracteres no ASCII, especialmente ante escrituras simultáneas. Alinear la normalización y la regla de unicidad con lo que puede garantizar la base de datos. |
| **P2 · Media** | `frontend/src/App.tsx` concentra la carga, el estado, los formularios y las operaciones de tareas y categorías en un único componente extenso. | A medida que crezca la interfaz, será más difícil cambiar o probar cada flujo de forma aislada. Separar las responsabilidades de tareas y categorías cuando se amplíe el alcance, evitando extraer abstracciones antes de necesitarlas. |
| **P3 · Baja** | Las pruebas de `backend/TodoApi.Tests/TodoServiceTests.cs` usan EF Core InMemory, no SQLite. | Cubren reglas del servicio, pero no verifican migraciones, colaciones, restricciones ni la excepción SQLite que maneja el servicio. Añadir pruebas específicas con SQLite en memoria para esas garantías. |
| **P3 · Baja** | `npm run lint` termina correctamente, pero Oxlint reporta advertencias de dependencias bajo `node_modules`; también muestra una advertencia en `frontend/src/App.tsx`. | El ruido de dependencias puede restar visibilidad a los problemas propios. Ajustar el alcance del lint para que valide el código del frontend y revisar la advertencia local. |

## Coherencia entre capas

- **Stack y arquitectura:** alineados con la intención del proyecto. La API implementa endpoints para tareas y categorías y el frontend consume esas rutas.
- **Contratos observados:** no se detectó un desajuste evidente entre los DTOs del backend y los tipos usados en el frontend; aun así, se mantienen por separado, por lo que conviene vigilar su sincronización.
- **Documentación de producto:** [docs/analisis-diseño.md](./analisis-diseño.md) describe las categorías y las reglas de API en línea con la implementación observada.
- **Deuda de documentación:** además de los requisitos de Node, conviene simplificar el README para que explique cómo ejecutar el proyecto existente, no cómo generarlo desde cero.

## Validaciones realizadas

- **Build del frontend:** correcto (`npm run build`).
- **Lint del frontend:** comando completado con código de salida 0; emitió advertencias, incluidas algunas procedentes de dependencias.
- **Pruebas del backend:** no se pudieron validar. `dotnet test` quedó bloqueado al intentar leer `C:\Users\hispa\.gitconfig`; no es un fallo de pruebas atribuible al código.

## Recomendaciones priorizadas

1. Corregir los README: actualizar los requisitos de Node, la ruta de la solución y los pasos de instalación y arranque.
2. Hacer coincidir la regla de unicidad de categorías entre .NET y SQLite, y cubrirla con pruebas SQLite.
3. Reducir el ruido de Oxlint y revisar su advertencia en `App.tsx`.
4. Si la interfaz sigue creciendo, dividir el componente por responsabilidades.

## Alcance y limitaciones

La auditoría refleja el estado del árbol de trabajo al 6 de octubre de 2026, que contiene cambios locales. No se modificó el código durante la auditoría. Para completar las pruebas del backend, la política del sandbox debe permitir el acceso al `.gitconfig` bloqueado.
