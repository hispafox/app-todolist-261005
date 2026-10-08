---
name: tests-unitarios
description: Genera pruebas unitarias faltantes, detecta huecos de cobertura y valida que el código cumpla lo definido en el análisis, el PRD y los requisitos funcionales. Está pensado para la primera capa de la pirámide de pruebas (unitarias), sin ampliar a integración ni E2E.
---

# Generador y validador de tests unitarios

Usa este skill cuando necesites:

- crear tests unitarios nuevos si aún no existen;
- detectar lógica sin cobertura o comportamientos no testeados;
- comparar el código con el análisis, el PRD, requisitos y casos de uso;
- validar que la implementación cumple la intención funcional antes de darlo por terminado;
- trabajar exclusivamente con pruebas unitarias, sin entrar en integración ni pruebas end-to-end.

## Objetivo

Este skill debe actuar como un analista de calidad y un generador de pruebas. Su misión es doble:

1. detectar si faltan tests unitarios;
2. completar la batería con tests que cubran el comportamiento esperado y evidencien los huecos que existen.

No debe inventar requisitos ni hacer suposiciones fuera del contexto del proyecto. Debe partir de lo que ya existe: código, análisis, PRD, historias de usuario, reglas de negocio y patrones del repositorio.

## Alcance

Este skill solo cubre la capa de pruebas unitarias.

- Sí: pruebas de unidades aisladas, casos felices, casos límite, validaciones, errores, condiciones y lógica de negocio.
- No: pruebas de integración, pruebas de base de datos reales, pruebas E2E, pruebas de UI con navegador ni pruebas de infraestructura.

## Flujo de trabajo recomendado

### 1) Identificar si existen tests unitarios

Antes de generar nada, revisa el proyecto para ver si ya hay tests y dónde están.

Busca patrones típicos como:

- `*Tests.cs` en proyectos .NET;
- `*.test.js`, `*.spec.js`, `*.test.ts`, `*.spec.ts` en JavaScript/TypeScript;
- carpetas `tests`, `__tests__`, `UnitTests`, `TestCases`, etc.

Si ya existen tests:

- comprueba qué partes del código cubren;
- identifica qué módulos no tienen ninguna prueba;
- detecta casos omitidos en la batería actual.

Si no existen:

- genera la estructura mínima necesaria y crea tests para la lógica relevante.

### 2) Analizar la intención del código y la documentación

Lee y usa la información disponible para definir el comportamiento esperado:

- PRD
- requisitos funcionales
- historias de usuario
- análisis técnico o funcional
- reglas de negocio
- nombres de clases, métodos y funciones
- comentarios y convenciones del proyecto

La prueba debe responder a la pregunta: “¿qué comportamiento debe garantizar este código?”

### 3) Detectar huecos de cobertura

Antes de escribir tests, identifica si faltan los siguientes casos:

- caso feliz / flujo principal;
- caso de error o validación;
- entradas vacías o nulas;
- valores límite o no esperados;
- condiciones alternativas;
- comportamiento cuando falla una dependencia;
- casos de frontera y edge cases;
- escenarios que la lógica del dominio debería proteger, aunque no estén documentados explícitamente.

Si el código tiene margen para interpretación, un test debe fijar el comportamiento esperado con un criterio claro.

### 4) Generar tests unitarios con el patrón AAA

Cuando escribas tests, usa siempre este patrón:

- Arrange: preparar datos y dependencias;
- Act: ejecutar la acción bajo prueba;
- Assert: verificar el resultado esperado.

Reglas:

- cada test debe enfocarse en una sola responsabilidad;
- evita tests demasiado grandes;
- usa nombres descriptivos y específicos;
- mantenlos deterministas y aislados;
- no dependas de estado global ni de entorno real de ejecución.

### 5) Elegir la tecnología adecuada al proyecto

Para este repositorio, el backend es .NET 10 con ASP.NET Core. En ese contexto, la opción natural es:

- xUnit para tests unitarios;
- FluentAssertions para aserciones más legibles;
- Moq para mocks cuando haga falta aislar dependencias;
- `dotnet test` como comando de verificación.

Si el proyecto fuera JavaScript o TypeScript, usaría:

- Vitest o Jest;
- `describe`/`it` con expectativas claras;
- mocking mínimo necesario.

### 6) Validar con la suite

Tras generar o ajustar tests:

- ejecuta la batería de pruebas relevante;
- corrige errores de compilación o de aserciones;
- refina los tests hasta que pasen de forma consistente.

Si el proyecto tiene un comando de test específico, úsalo. En este repositorio, lo más probable es `dotnet test`.

## Criterios de calidad para la skill

La skill solo debe dar resultado si cumple estas condiciones:

- crea tests unitarios si faltan;
- no inventa un alcance más amplio que el de unit tests;
- compara el código con los requisitos existentes;
- detecta huecos de cobertura y los documenta o resuelve;
- mantiene el test aislado y reproducible;
- usa el framework apropiado al stack del proyecto;
- ejecuta la validación antes de cerrar la tarea.

## Reglas de trabajo

### Regla 1: no hagas pruebas “de relleno”

No generes tests que solo repitan el mismo caso con un nombre distinto. Cada test debe cubrir una intención real y verificable.

### Regla 2: prioriza el comportamiento observable

Los tests deben verificar lo que un usuario o una capa de negocio espera, no solo la implementación interna.

### Regla 3: identifica prueba y no prueba

Cuando descubras una ruta de código sin cobertura, no lo dejes como “se supone que está cubierto”. Debes explicitar:

- qué falta;
- por qué es necesario;
- qué test debería añadirse.

### Regla 4: la validación tiene que ser real

No te quedes en un análisis conceptual. Ejecuta la batería de tests y confirma el resultado con evidencia.

### Regla 5: no amplíes el alcance

Si el objetivo es unit tests, no conviertas la tarea en integración o E2E. Mantén la frontera clara.

## Salida esperada

Cuando el skill funcione correctamente, debe entregar al menos:

- lista de pruebas existentes y huecos detectados;
- tests nuevos creados o ajustados;
- cobertura funcional frente a requisitos y PRD;
- resultado de validación ejecutada;
- si faltan casos adicionales, una lista de sugerencias concretas para ampliarlos.

## Plantilla de ejecución

Usa este flujo al trabajar con el proyecto:

```text
1. Revisa si hay tests unitarios actuales.
2. Identifica módulos o funciones sin prueba.
3. Lee el PRD, requisitos y análisis funcional disponible.
4. Detecta casos no cubiertos: happy path, errores, límites, validaciones.
5. Genera tests unitarios con el patrón AAA.
6. Ejecuta la suite relevante.
7. Corrige fallos y repite la validación.
8. Resume los huecos resueltos y los que aún quedan abiertos.
```

## Ejemplos de prompts útiles

```text
Crea los tests unitarios que faltan para esta funcionalidad y comprueba que cumplan el PRD y los requisitos del análisis.
```

```text
Busca huecos de cobertura en la lógica de negocio y añade pruebas unitarias para los casos no contemplados.
```

```text
Revisa si el código ya tiene tests unitarios; si no, genera la batería mínima necesaria y valida el resultado con dotnet test.
```

```text
Valida esta clase frente al PRD y al análisis funcional y añade pruebas unitarias para los casos clave, errores y edge cases.
```

## Resultado esperado para este repositorio

Dado que este proyecto es una Todo App con backend .NET y frontend React, la skill debe priorizar:

- tests unitarios del backend y de la lógica de negocio;
- validaciones de creación, consulta, actualización y eliminación de tareas;
- casos de errores y validaciones de datos;
- comparación con requisitos funcionales del PRD;
- ejecución real con `dotnet test` cuando corresponda.

No debe forzar pruebas de integración ni E2E en esta fase.
