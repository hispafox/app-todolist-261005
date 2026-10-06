# Análisis completo del proyecto: Todo App

## 1. Introducción

La aplicación Todo App es un proyecto de tipo full-stack orientado a la gestión de tareas personales y de trabajo en un entorno local. Su finalidad principal es demostrar cómo construir una solución completa con tecnologías modernas, combinando un backend robusto, una base de datos persistente y una interfaz web amigable.

El proyecto está pensado como una base educativa y práctica para validar conceptos de:

- arquitectura cliente-servidor;
- API REST;
- persistencia con SQLite;
- uso de Entity Framework Core;
- desarrollo con React y Vite;
- integración entre frontend y backend;
- trabajo colaborativo con GitHub Copilot.

## 2. Contexto del negocio

El producto responde a una necesidad real y cotidiana: organizar actividades, tareas o recordatorios de forma simple, clara y efectiva. En muchos contextos, las personas necesitan mantener una lista de pendientes sin depender de soluciones complejas, costosas o difíciles de personalizar.

La Todo App busca resolver este problema con una herramienta ligera y funcional, enfocada en la productividad básica, con una experiencia intuitiva y una implementación accesible para aprendizaje.

## 3. Objetivo del proyecto

Desarrollar una aplicación web de gestión de tareas que permita:

- crear tareas nuevas;
- visualizar el conjunto de tareas pendientes y completadas;
- actualizar el estado de una tarea;
- modificar su contenido cuando cambie la necesidad;
- eliminar tareas que ya no resulten útiles;
- conservar la información entre sesiones mediante persistencia local.

## 4. Alcance del sistema

### 4.1 Alcance incluido

- CRUD de tareas.
- Persistencia en SQLite.
- API REST para consumo del frontend.
- Interfaz web con React.
- Validación de entrada mínima.
- Operación local en entorno de desarrollo.

### 4.2 Alcance excluido del MVP

- autenticación de usuarios;
- roles y permisos;
- tareas compartidas;
- categorías complejas;
- prioridad o etiquetas avanzadas;
- filtros sofisticados;
- sincronización con servicios externos;
- notificaciones automáticas;
- despliegue en producción.

## 5. Usuarios objetivo

### 5.1 Perfil principal

El usuario principal es cualquier persona que necesite gestionar tareas personales, académicas o profesionales de manera ágil.

Ejemplos:

- estudiantes;
- profesionales autónomos;
- equipos pequeños;
- usuarios que buscan una herramienta sencilla y directa.

### 5.2 Necesidades principales

- registrar actividades de forma rápida;
- consultar el estado del trabajo pendiente;
- actualizar tareas cuando cambian los requerimientos;
- eliminar elementos irrelevantes;
- tener una interfaz clara y sin fricción.

## 6. Visión funcional

La aplicación debe permitir al usuario llevar una lista de tareas de forma simple, visual y persistente. La operación básica no debe requerir entrenamiento previo ni procesos complejos. La aplicación debe funcionar como una herramienta de organización personal con alta legibilidad y mínima curva de aprendizaje.

## 7. Arquitectura propuesta

### 7.1 Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite como base de datos local
- migraciones para evolución del esquema

### 7.2 Frontend

- React
- Vite
- comunicación con el backend mediante HTTP
- interfaz para crear, listar, editar y eliminar tareas

### 7.3 Diagrama conceptual

```text
[Frontend React]
        |
        v
[API ASP.NET Core]
        |
        v
[SQLite Database]
```

## 8. Modelo de dominio

La entidad principal del sistema es la tarea.

### Entidad sugerida

```csharp
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Atributos

- Id: identificador único de la tarea.
- Title: título o descripción breve de la actividad.
- IsCompleted: indica si la tarea está completa o pendiente.
- CreatedAt: fecha de creación.

## 9. Requisitos funcionales (RF)

A continuación se detallan los requisitos funcionales del sistema.

### RF-01: Crear una tarea
El sistema debe permitir al usuario registrar una nueva tarea desde la interfaz web.

Criterios de aceptación:

- el usuario ingresa un título o contenido válido;
- la tarea se crea correctamente en el backend;
- la información se almacena en SQLite;
- la nueva tarea aparece en la lista inmediatamente;
- el sistema valida que el contenido obligatorio no esté vacío.

Prioridad: Alta

### RF-02: Consultar la lista de tareas
El sistema debe mostrar todas las tareas registradas para que el usuario pueda visualizarlas.

Criterios de aceptación:

- la aplicación consulta la API;
- la lista se presenta de forma legible;
- cada tarea muestra su estado actual;
- el usuario puede ver tareas pendientes y completadas.

Prioridad: Alta

### RF-03: Marcar una tarea como completada
El sistema debe permitir cambiar el estado de una tarea entre pendiente y completada.

Criterios de aceptación:

- el usuario puede marcar una tarea como completada;
- el cambio se persiste en la base de datos;
- la interfaz refleja el nuevo estado sin requerir recarga completa;
- la estructura de la tarea se mantiene intacta.

Prioridad: Alta

### RF-04: Editar una tarea
El sistema debe permitir modificar el contenido o título de una tarea existente.

Criterios de aceptación:

- el usuario puede abrir la tarea para editarla;
- los cambios se guardan en la base de datos;
- la vista se actualiza con la información nueva;
- la edición no rompe la integridad del registro.

Prioridad: Alta

### RF-05: Eliminar una tarea
El sistema debe permitir la eliminación de una tarea.

Criterios de aceptación:

- la tarea desaparece de la lista después de la eliminación;
- la operación se refleja en la base de datos;
- la acción puede requerir confirmación si la UX lo necesita;
- la lista se actualiza tras la eliminación.

Prioridad: Alta

### RF-06: Persistencia de datos
El sistema debe guardar la información en SQLite para que los datos sobrevivan a reinicios de la aplicación.

Criterios de aceptación:

- la base de datos se crea o actualiza con migraciones;
- la información persiste entre sesiones;
- el entorno local utiliza almacenamiento persistente.

Prioridad: Alta

### RF-07: Consumo desde frontend
El frontend debe consumir la API del backend para realizar las operaciones del CRUD.

Criterios de aceptación:

- el frontend realiza llamadas HTTP correctas;
- la aplicación maneja errores de servidor y red;
- la experiencia del usuario no se rompe cuando falla una operación.

Prioridad: Alta

### RF-08: Validación de entradas
El sistema debe validar que la información ingresada por el usuario sea apropiada antes de guardar o actualizar una tarea.

Criterios de aceptación:

- no se permiten cadenas vacías o inválidas;
- el sistema devuelve feedback visual o lógico cuando la entrada no es válida;
- la integridad del dato se conserva.

Prioridad: Media

## 10. Historias de usuario (HU)

### HU-01: Crear una nueva tarea
Como usuario, quiero crear una tarea rápidamente para registrar actividades pendientes.

Aceptación:

- el usuario puede ingresar una tarea en la interfaz;
- la tarea se crea y almacena correctamente;
- la nueva tarea aparece en la lista principal.

### HU-02: Revisar mi lista de tareas
Como usuario, quiero ver todas mis tareas para tener un panorama claro de lo que debo hacer.

Aceptación:

- la lista se carga desde el backend;
- cada tarea muestra su estado actual;
- el usuario puede identificar tareas pendientes y completadas.

### HU-03: Completar una tarea
Como usuario, quiero marcar una tarea como completada para llevar un control visual de mi progreso.

Aceptación:

- la tarea cambia de estado;
- la actualización se guarda en la base de datos;
- la UI refleja el cambio sin perder el resto de la información.

### HU-04: Corregir una tarea
Como usuario, quiero editar una tarea si cambia su descripción o su contenido para mantenerla actualizada.

Aceptación:

- el usuario puede modificar la tarea seleccionada;
- los cambios se guardan correctamente;
- la tarea editada se muestra con el nuevo contenido.

### HU-05: Eliminar una tarea equivocada
Como usuario, quiero borrar tareas que ya no necesito para mantener mi lista ordenada.

Aceptación:

- el usuario elimina una tarea;
- la tarea desaparece de la vista;
- la eliminación queda persistida en la base de datos.

### HU-06: Recuperar el estado de la lista
Como usuario, quiero que las tareas persistan entre sesiones para no perder información importante.

Aceptación:

- la aplicación conserva los datos después de cerrar o reiniciar la app;
- la base de datos mantiene el estado de cada tarea.

### HU-07: Gestionar errores de operación
Como usuario, quiero recibir una respuesta clara cuando una acción falla para saber que ocurrió y poder reintentar.

Aceptación:

- el sistema maneja errores de red o de servidor;
- la UI comunica la condición al usuario;
- la operación fallida no deja la aplicación en un estado inconsistente.

### HU-08: Mantener la interfaz simple
Como usuario, quiero una interfaz limpia y fácil de usar para gestionar mis tareas sin complicaciones.

Aceptación:

- la pantalla principal es clara y directa;
- las acciones principales están visibles y comprensibles;
- la experiencia es usable en un entorno local de desarrollo.

## 11. Reglas de negocio

- toda tarea debe tener un identificador único;
- el contenido de la tarea debe ser válido antes de guardarse;
- una tarea puede estar en estado pendiente o completada;
- la aplicación debe mantener la integridad de la información;
- la eliminación debe ser persistente y visible en la interfaz;
- la solución debe operar sin infraestructura externa compleja;
- la base de datos debe ser compatible con el modelo de trabajo local de desarrollo.

## 12. Requisitos no funcionales

### 12.1 Rendimiento
- la carga de la lista debe ser rápida en un entorno local;
- las operaciones CRUD deben responder sin retrasos visibles para el usuario.

### 12.2 Seguridad básica
- la entrada del usuario debe validarse antes de guardar información;
- la aplicación debe evitar trabajo con datos vacíos o inconsistentes;
- la solución está diseñada para un entorno local y no para uso multiusuario avanzado.

### 12.3 Mantenibilidad
- la separación entre frontend y backend debe mantenerse clara;
- la estructura del proyecto debe ser simple y extensible;
- el código debe facilitar futuras mejoras sin reescrituras completas.

### 12.4 Escalabilidad
- el sistema debe permitir añadir nuevas funcionalidades sin romper el MVP actual;
- la arquitectura debe ser compatible con evoluciones futuras secuenciales.

## 13. Casos de uso principales

### Caso de uso 1: Crear tarea
1. El usuario accede a la aplicación.
2. Escribe el título o contenido de la tarea.
3. Envía la información.
4. El backend valida la entrada.
5. El sistema guarda la tarea en SQLite.
6. La tarea aparece en la lista.

### Caso de uso 2: Completar tarea
1. El usuario selecciona una tarea.
2. Marca la opción de completado.
3. El sistema actualiza el estado.
4. La base de datos refleja el cambio.
5. La interfaz muestra el estado actualizado.

### Caso de uso 3: Editar tarea
1. El usuario selecciona una tarea existente.
2. Modifica su contenido.
3. Guarda los cambios.
4. El sistema persiste la actualización.
5. La tarea mostrada se actualiza con la nueva versión.

### Caso de uso 4: Eliminar tarea
1. El usuario indica que desea eliminar una tarea.
2. El sistema confirma o ejecuta la operación.
3. La tarea desaparece de la lista.
4. La base de datos elimina el registro.

## 14. Riesgos y supuestos

### Riesgos

- errores de integración entre frontend y backend;
- cambios de esquema sin migración correcta;
- fallos de configuración local de entorno;
- dependencia de paquetes y herramientas externas en la máquina del desarrollador.

### Supuestos

- el proyecto será ejecutado en un entorno de desarrollo local;
- los usuarios principales son personas con un nivel básico de experiencia en aplicaciones web;
- la solución se usará como base educativa y demostrativa.

## 15. Criterios de éxito

La solución será exitosa si:

- el usuario puede crear tareas desde la interfaz;
- la lista se actualiza correctamente;
- las tareas se mantienen persistidas en SQLite;
- editar y eliminar tareas funciona sin errores de flujo;
- la API responde de forma consistente;
- la aplicación puede ejecutarse localmente con una configuración mínima.

## 16. Conclusión

La Todo App es una propuesta funcional y educativa para gestionar tareas diarias con tecnologías modernas. A través de una arquitectura simple y clara, el proyecto demuestra cómo combinar React, ASP.NET Core, SQLite y Entity Framework para crear una solución realista de manejo de información.

El análisis funcional muestra que el sistema cumple con un conjunto claro de requisitos básicos: creación, consulta, actualización, eliminación y persistencia. Esto genera una base sólida para evolución futura, tanto en el plano funcional como técnico.

Este documento establece la base para el desarrollo, validación y mantenimiento del proyecto, y sirve como referencia para futuras ampliaciones.
