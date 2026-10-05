---
name: commit-message-authoring
description: Genera mensajes de commit claros, específicos y útiles para cambios de código, documentación o configuración. Úsalo antes de hacer un commit o cuando necesites resumir un conjunto de cambios con un título preciso y, si hace falta, un cuerpo breve.
---

# Generación de mensajes de commit

Usa este skill cuando vayas a preparar un commit, resumir cambios o escribir el mensaje de Git para un cambio concreto.

## Objetivo

El mensaje de commit debe decir exactamente qué cambió, sin vaguedades ni frases genéricas.

- No escribas cosas como: "actualizar fichero", "mejorar código", "cambiar cosas", "arreglar errores".
- Sí escribe detalles concretos: qué módulo, qué funcionalidad, qué archivo o qué flujo se tocó.
- La primera línea debe ser un resumen corto y preciso.
- El cuerpo es opcional, pero cuando aporta valor debe explicar el porqué y el alcance del cambio.
- Si el cambio es pequeño o trivial, una sola línea basta.

## Formato recomendado

Usa este patrón:

```text
<tipo>: <resumen corto y específico>

<detalle opcional del cambio>
```

Ejemplos válidos:

```text
docs: cambiar idioma de código de inglés a castellano
feat: añadir creación de tareas desde la pantalla principal
fix: corregir validación del título vacío al crear tareas
refactor: simplificar la obtención de tareas en el backend
chore: actualizar dependencias de React y Vite
```

## Reglas de calidad

### 1) Sé específico

Haz que el resumen responda a estas preguntas:

- ¿Qué se tocó exactamente?
- ¿Qué funcionalidad o fichero cambió?
- ¿Qué efecto tuvo el cambio?

Ejemplos:

- Correcto: `docs: cambiar idioma de código de inglés a castellano`
- Incorrecto: `docs: actualizar instrucciones`

- Correcto: `fix: corregir error al marcar tareas como completadas desde la API`
- Incorrecto: `fix: arreglar bug`

### 2) La primera línea es la síntesis

La primera línea debe leer como un resumen ejecutivo del cambio.

- Debe ser clara y legible.
- Debe contener el tipo y la acción principal.
- No debe ser un título demasiado amplio ni genérico.

### 3) El cuerpo es opcional, pero útil cuando hace falta

Si el cambio no cabe en una línea o tiene contexto relevante, añade un cuerpo breve.

Formato:

```text
feat: añadir edición de tareas en la modal de detalle

- actualizar el formulario para cargar el contenido actual
- validar el título antes de guardar cambios
- mantener la lista sincronizada tras la edición
```

Cuando el cambio es muy simple, no hace falta cuerpo. Un único resumen basta.

### 4) Sé escueto, no vago

La brevedad no significa falta de precisión.

- Bueno: `docs: cambiar idioma de código de inglés a castellano`
- Malo: `actualizar documentación`

- Bueno: `fix: evitar duplicados al guardar tareas con el mismo título`
- Malo: `arreglar validación`

## Tipos recomendados

Usa tipos de commit simples y consistentes:

- `feat`: nueva funcionalidad
- `fix`: corrección de un error
- `docs`: cambios de documentación o textos
- `refactor`: reorganización del código sin cambiar comportamiento
- `style`: cambios de formato o estilo visual
- `chore`: tareas de mantenimiento, dependencias, configuración

## Criterio final

Antes de finalizar un commit, comprueba:

- ¿La primera línea dice exactamente qué cambió?
- ¿He evitado frases vagas?
- ¿He incluido detalle solo si aporta contexto?
- ¿El commit es corto pero específico?

Si no hay un cambio importante que justificar cuerpo, no lo añadas.

## Ejemplos para este proyecto

```text
feat: añadir creación de tareas desde la API y la interfaz
fix: corregir estado completado al marcar tareas en la lista
docs: cambiar idioma de código de inglés a castellano
refactor: simplificar modelo de datos de TodoItem
chore: actualizar configuración de Vite para el frontend
```

## Regla de oro

Cada commit debe poder entenderse sin abrir el diff completo.

Si alguien leyera solo la primera línea, debería saber qué cambió exactamente.
