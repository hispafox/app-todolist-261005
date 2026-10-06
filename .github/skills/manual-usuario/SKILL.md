---
name: manual-usuario
description: 'Crea o actualiza el manual de usuario de la aplicación en formato Word (.docx), en español y basado en las pantallas y funcionalidades realmente implementadas. Úsalo cuando se pida generar el manual de usuario, una guía de uso o documentación para usuarios finales en Word.'
argument-hint: 'Ruta de salida opcional (por defecto: docs/manual-usuario.docx)'
---

# Skill: Manual de usuario en Word

## Objetivo y alcance

Generar un documento Word listo para usuarios finales, con instrucciones prácticas para utilizar la aplicación. El resultado es un archivo `.docx` real, no un Markdown renombrado ni únicamente una propuesta de contenido.

Este skill documenta la aplicación; no modifica su código, base de datos ni configuración. No genera un manual técnico de instalación o de la API salvo petición expresa.

## Dependencias y fuentes

- Invocar el skill [docx](../docx/SKILL.md) antes de crear o editar el documento y seguir sus reglas de generación y validación.
- Leer [README.md](../../../README.md) para conocer el propósito y las instrucciones de acceso.
- Consultar [docs/analisis-diseño.md](../../../docs/analisis-diseño.md) si existe, distinguiendo los requisitos de las funcionalidades disponibles.
- Inspeccionar [frontend/src/](../../../frontend/src/) para identificar pantallas, botones, formularios, estados y mensajes reales. Actualmente la interfaz principal está en [App.tsx](../../../frontend/src/App.tsx).
- Consultar los controladores, DTOs y servicios de [backend/TodoApi/](../../../backend/TodoApi/) únicamente para verificar restricciones o efectos que influyan en el uso.
- Verificar la configuración efectiva del frontend y del backend antes de mencionar URLs o puertos. No copiar valores de ejemplos sin contrastarlos.

El código implementado y, cuando sea posible, la aplicación ejecutándose prevalecen sobre la documentación desactualizada. No presentar funciones previstas, endpoints sin interfaz o posibles mejoras como acciones disponibles para el usuario.

## Procedimiento

### 1. Determinar la salida

Usar `docs/manual-usuario.docx` si no se ha indicado otra ruta.

Si el documento ya existe, leerlo antes de actualizarlo. Conservar contenido y personalizaciones válidas; si hay cambios manuales que entren en conflicto con la actualización, consultar al usuario antes de sustituirlos.

Trabajar con las convenciones del repositorio y rutas compatibles con el sistema operativo. No incluir rutas absolutas de la máquina del autor en el manual.

### 2. Comprobar las funciones reales

Preparar una lista de las operaciones visibles y sus etiquetas exactas. Para esta aplicación, comprobar al menos:

- Consultar tareas, fecha de creación, categoría y contador de completadas.
- Añadir una tarea, con categoría opcional o la opción «Sin categoría».
- Marcar y desmarcar una tarea como completada.
- Editar el título y la categoría; guardar o cancelar la edición.
- Eliminar una tarea y verificar si existe confirmación, recuperación o deshacer.
- Crear, renombrar y eliminar categorías; verificar qué ocurre si una categoría está en uso.
- Reconocer los estados de carga, lista vacía y errores de validación o comunicación.

Esta lista es una guía de comprobación, no una garantía de funcionalidad. Adaptarla a la versión actual y añadir otras acciones solo si existen en la interfaz.

Verificar límites de texto, campos obligatorios, duplicados y consecuencias de eliminar datos en el código correspondiente. No inventar confirmaciones, filtros, autenticación, copias de seguridad, recuperación de datos ni guardado automático.

### 3. Redactar para usuarios finales

Estructurar el documento en este orden:

1. **Portada:** nombre real de la aplicación, «Manual de usuario» y fecha de elaboración. Incluir versión o revisión del repositorio solo si se puede comprobar; no inventarla ni atribuir cambios locales a una versión publicada.
2. **Índice:** tabla de contenido de Word basada en estilos de encabezado.
3. **Introducción:** finalidad del producto, público destinatario y alcance del manual.
4. **Acceso y primeros pasos:** abrir la aplicación en el navegador usando la dirección verificada; diferenciar el entorno local de una URL publicada. Si no se conoce la dirección publicada, indicar que debe facilitarla el responsable.
5. **Conocer la pantalla:** zonas principales, campos, botones, indicador de progreso y estados visibles.
6. **Gestionar tareas:** un procedimiento por operación disponible.
7. **Gestionar categorías:** operaciones disponibles, asignación opcional y restricciones comprobadas.
8. **Mensajes y resolución de problemas:** síntomas reales, significado y acciones seguras para el usuario.
9. **Preguntas frecuentes y límites:** persistencia y comportamiento verificados; funciones no disponibles relevantes para evitar confusiones.

Cada procedimiento debe incluir objetivo, requisitos previos si aplican, pasos numerados y resultado esperado. Reiniciar la numeración en cada procedimiento. Reproducir literalmente las etiquetas de los controles, con lenguaje claro en español y ejemplos ficticios sin datos personales.

Advertir antes de las acciones destructivas. No recomendar borrar la base de datos, ejecutar migraciones, reinstalar dependencias o modificar archivos como solución para usuarios finales.

### 4. Capturas de pantalla

Incluir capturas solo cuando se puedan obtener de la aplicación real o hayan sido proporcionadas por el usuario.

- Usar datos ficticios y evitar secretos, información personal y rutas locales sensibles.
- Añadir pie descriptivo y texto alternativo a cada captura.
- Mantener la proporción y ajustar las imágenes al ancho útil de la página.
- No crear pantallas simuladas ni dejar marcadores de capturas pendientes.
- No realizar pruebas destructivas sobre datos existentes para obtener imágenes. Usar un entorno de prueba autorizado.

Si la aplicación no puede ejecutarse o no hay capturas disponibles, generar el manual sin ellas a partir del código e informar de esta limitación al entregar el resultado. No afirmar que se ha probado visualmente la interfaz.

### 5. Generar el Word

Aplicar el flujo del skill `docx`. Para un documento nuevo, usar `docx-js`; para uno existente, seguir su procedimiento de lectura y edición.

Formato predeterminado:

- A4 vertical, dimensiones explícitas y márgenes de 2,54 cm.
- Arial, cuerpo de 11 o 12 puntos y títulos jerárquicos legibles.
- Encabezados reales de Word para navegación e índice.
- Listas numeradas nativas, no números o viñetas dibujados manualmente.
- Pie de página con numeración y encabezado discreto con el nombre de la aplicación.
- Tablas solo cuando ayuden, con anchos coherentes y cabecera reconocible.

Reutilizar herramientas disponibles. Si faltan dependencias, instalarlas de forma aislada para la generación documental, sin añadirlas al frontend ni cambiar los manifiestos de la aplicación. Guardar scripts auxiliares y archivos temporales fuera del código de la app, en el espacio temporal de la sesión.

Si una herramienta falla, mostrar la causa y corregirla cuando sea posible. No entregar un archivo corrupto, vacío o de otro formato como si fuera un Word válido.

### 6. Validar y entregar

Antes de dar por terminado el trabajo:

- Ejecutar el validador indicado por `docx` sobre el archivo final y corregir los errores.
- Extraer o leer el contenido del `.docx` generado y comprobar todas las secciones y procedimientos frente a las funciones verificadas.
- Comprobar portada, idioma, títulos, listas, índice, pies, tablas e imágenes. Si hay herramientas de renderizado disponibles, revisar también el aspecto visual.
- No confundir validación estructural con revisión visual. Si no se puede renderizar, comunicarlo.
- Si el índice requiere actualizar los campos en Word, indicar al usuario que seleccione el índice y elija «Actualizar tabla»; no dar por verificadas las páginas calculadas sin renderizado.
- Confirmar que el archivo existe en la ruta acordada y limpiar únicamente los archivos temporales creados para esta ejecución.

Entregar un enlace al `.docx`, un resumen breve de los temas cubiertos y las limitaciones de comprobación, si las hay. No hacer commit ni publicar el documento salvo petición expresa.

## Ejemplos de invocación

- «Usa manual-usuario para crear el manual de la aplicación en Word».
- «Actualiza el manual de usuario con las funciones actuales de categorías».
- «Genera la guía de uso en docs/guia-usuario.docx, sin capturas».
