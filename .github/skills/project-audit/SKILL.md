---
name: project-audit
description: Revisa el estado general del proyecto para detectar inconsistencias, desajustes técnicos, deuda de configuración, problemas de arquitectura y desviaciones respecto a la especificación del repositorio y al stack acordado.
---

# Auditoría general del proyecto

Usa este skill cuando necesites:

- revisar si el proyecto cumple la especificación acordada;
- detectar inconsistencias entre documentación, configuración y código;
- identificar deuda técnica o desalineaciones de stack;
- comprobar si el repositorio está construido según el enfoque definido por el equipo;
- evaluar el estado general del proyecto antes de continuar con cambios o nuevas funcionalidades;
- confirmar si el proyecto está usando correctamente el frontend y el backend esperados.

## Objetivo

Este skill debe actuar como auditor del repositorio completo. Su misión principal es comparar el estado real del proyecto contra la intención declarada del mismo y señalar discrepancias, riesgos, deuda técnica y puntos de mejora.

No debe centrarse solo en un detalle concreto como TypeScript o en una capa aislada. Debe revisar la solución en conjunto: documentación, estructura de carpetas, dependencias, configuración del frontend, configuración del backend, patrones de arquitectura, scripts, convenciones, y coherencia entre los distintos niveles del proyecto.

## Alcance

Este skill cubre una auditoría general del repositorio, no solo de una tecnología concreta.

Sí incluye:

- revisión del README y documentación del proyecto;
- comprobación del stack tecnológico declarado versus el realmente configurado;
- inspección del frontend y backend para detectar incoherencias;
- análisis de dependencias, scripts y configuración;
- revisión de la estructura del repositorio y de los artefactos generados;
- detección de deuda técnica, configuraciones incompletas o desalineadas;
- evaluación de si la implementación refleja las decisiones de diseño del proyecto.

No incluye:

- corrección automática del proyecto;
- refactors de código ni cambios funcionales sin consenso;
- tareas de despliegue o ejecución en nube;
- pruebas de integración o E2E como centro de la auditoría.

## Qué debe comprobar

El auditor debe revisar, al menos, estos aspectos:

### 1) Documentación y especificación

- Si el README o documentos del proyecto describen correctamente el stack y el objetivo.
- Si la documentación está actualizada respecto a lo que el repositorio realmente contiene.
- Si las instrucciones de setup y ejecución coinciden con la configuración real.

### 2) Stack tecnológico y compatibilidad

- Si el frontend está configurado como React + TypeScript o si está realmente en JavaScript.
- Si la versión del framework, compilador y tooling declarados en package.json se corresponden con la intención del proyecto.
- Si el backend y el frontend son compatibles con el objetivo del repositorio y con la documentación.

### 3) Configuración del frontend

- Archivos clave como `package.json`, `vite.config.*`, `tsconfig*`, `index.html`, `src/`.
- Si la base del frontend está tipada correctamente en TypeScript cuando la intención del proyecto lo exige.
- Scripts de construcción, lint y arranque.
- Dependencias necesarias y redundancias.

### 4) Configuración del backend

- Estructura del proyecto ASP.NET Core.
- Dependencias y versión del SDK.
- Configuración de la API y su relación con el frontend.
- Si la solución se ajusta a la intención general del repositorio.

### 5) Coherencia entre capas

- Si backend y frontend siguen una misma estrategia de diseño.
- Si los contratos de comunicación están alineados.
- Si la app ofrece un flujo consistente de tareas como creación, lectura, actualización, eliminación y visualización.

### 6) Deuda técnica y riesgos

- Configuraciones incompletas.
- Archivos generados por plantilla que no corresponden al proyecto.
- Dependencias obsoletas, redundantes o innecesarias.
- Archivos de ejemplo, documentación genérica o plantillas que no reflejan la app real.
- Inconsistencias entre lo que la documentación dice y lo que el repo contiene.

## Método de auditoría

1. Revisar la documentación principal y las instrucciones del repositorio.
2. Inspeccionar los principales archivos de configuración y estructura del proyecto.
3. Validar si el stack real coincide con el stack deseado.
4. Buscar incoherencias entre archivos clave (README, package.json, configs, código principal).
5. Evaluar si la arquitectura general y la organización del repositorio son consistentes.
6. Identificar riesgos, deuda técnica y elementos a corregir antes de continuar.
7. Entregar un resumen claro con hallazgos y recomendación.

## Salida esperada

La auditoría debe entregar, como mínimo:

- resumen ejecutivo del estado general del proyecto;
- lista de inconsistencias detectadas;
- hallazgos de configuración y documentación;
- riesgos o deuda técnica relevante;
- recomendaciones concretas priorizadas;
- declaración de si el proyecto está alineado con la intención del stack y de la especificación.

## Plantilla de respuesta esperada

La auditoría debe responder con un formato útil para toma de decisiones, por ejemplo:

```text
Estado general: [alineado / parcialmente alineado / desalineado]

Hallazgos principales:
- ...
- ...

Riesgos:
- ...

Recomendaciones:
1. ...
2. ...
3. ...
```

## Reglas de trabajo

### Regla 1: mirar el repositorio completo

No te quedes en una sola capa. La auditoría debe cubrir el proyecto como un conjunto.

### Regla 2: comparar con la intención del proyecto

La auditoría no debe limitarse a decir “esto está mal”, sino a responder si el proyecto está alineado con su propósito y su stack deseado.

### Regla 3: identificar desajustes concretos

Cada hallazgo debe ser explicable, observable y verificable en el repositorio.

### Regla 4: priorizar hechos sobre suposiciones

La auditoría debe apoyarse en archivos reales de configuración, código y documentación, no en hipótesis.

### Regla 5: mantener el enfoque general

Aunque el proyecto use una tecnología concreta, no conviertas la auditoría en una revisión solo de una librería o de un detalle puntual.

## Resultado esperado para este repositorio

En este proyecto se debe comprobar especialmente:

- que la documentación y configuración del frontend estén alineadas con React + TypeScript;
- que el repositorio no contenga un stack mezclado o inconsistencias entre JavaScript y TypeScript;
- que la estructura general del backend y frontend siga siendo coherente y útil para el desarrollo;
- que no exista deuda técnica o archivos de plantilla que confundan la intención del proyecto;
- que la solución general esté preparada para continuar desarrollándose de forma limpia.
