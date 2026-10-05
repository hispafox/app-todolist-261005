# PRD - Todo App

## 1. Resumen ejecutivo

La aplicación Todo App es un proyecto full-stack de gestión de tareas pensado como base educativa y demostrativa para practicar el desarrollo con tecnologías modernas: .NET 10, ASP.NET Core Web API, SQLite, Entity Framework Core y React con Vite. El objetivo principal es permitir a los usuarios crear, consultar, actualizar y eliminar tareas de forma sencilla desde una interfaz web, con persistencia real en base de datos.

El producto se centra en ofrecer una experiencia clara y funcional para la administración de tareas diarias, con una arquitectura simple y mantenible que puede evolucionar hacia casos de uso más complejos en el futuro.

Este documento define los requisitos del producto, el alcance del MVP, las personas usuarias, los objetivos de negocio y los criterios de aceptación para garantizar un desarrollo consistente y medible.

## 2. Visión del producto

Crear una aplicación web de lista de tareas útil, intuitiva y fácil de mantener, que permita:

- registrar tareas rápidamente;
- visualizar el estado de cada tarea;
- marcar tareas como completadas;
- editar contenido cuando sea necesario;
- eliminar tareas que ya no sean relevantes;
- mantener la información persistida entre sesiones.

La aplicación debe servir como base sólida para aprender arquitectura de software, integración entre frontend y backend, manejo de bases de datos, migraciones y despliegue local.

## 3. Problema a resolver

Muchas personas necesitan una herramienta simple para organizar su trabajo diario, sin depender de soluciones complejas, costosas o difíciles de personalizar. Las listas de tareas básicas suelen requerir:

- rapidez de uso;
- acceso inmediato a la información;
- actualización del estado de las tareas;
- persistencia confiable;
- una interfaz visual simple.

La aplicación busca cubrir esa necesidad con una solución ligera y moderna, accesible para aprendizaje y ampliación posterior.

## 4. Objetivos del producto

### 4.1 Objetivos de negocio

- ofrecer una aplicación funcional de tareas que demuestre buenas prácticas de desarrollo;
- facilitar la práctica de integración entre frontend y backend;
- mostrar un flujo real de persistencia con SQLite y Entity Framework Core;
- proporcionar una base reutilizable para proyectos más complejos.

### 4.2 Objetivos de usuario

- registrar tareas sin fricción;
- ver el estado general de la lista;
- actualizar tareas cuando cambian sus requisitos;
- mantener un control simple de actividades pendientes y completadas.

### 4.3 Objetivos técnicos

- estructurar un backend REST con ASP.NET Core;
- usar SQLite como base de datos local; 
- implementar acceso a datos con Entity Framework Core;
- construir un frontend con React y Vite;
- mantener una separación clara entre capas y responsabilidades.

## 5. Usuarios objetivo

### 5.1 Usuario principal

Persona que desea organizar tareas cotidianas de manera simple y visual.

Perfil tipo:

- estudiante;
- profesional independiente;
- equipo pequeño;
- persona que necesita un sistema personal de tareas.

### 5.2 Necesidades principales

- crear nuevas tareas con rapidez;
- ver tareas pendientes y completadas;
- editar contenido cuando se detecte una corrección;
- eliminar tareas que ya no son útiles;
- no requerir una curva de aprendizaje compleja.

## 6. Alcance

### 6.1 MVP (versión mínima viable)

El MVP incluye:

- creación de tareas;
- listado de tareas;
- visualización del estado de cada tarea;
- marcado como completada;
- edición de título o contenido;
- eliminación de tareas;
- persistencia en SQLite;
- API REST para consumir la información desde el frontend;
- interfaz web básica con React.

### 6.2 Fuera de alcance del MVP

- autenticación y autorización;
- gestión de usuarios;
- roles y permisos;
- tareas con prioridad, etiquetas o categorías avanzadas;
- filtros complejos;
- sincronización en la nube;
- notificaciones;
- integración con terceros.

## 7. Requisitos funcionales

### RF-01: Crear tarea
El usuario puede crear una nueva tarea desde la interfaz web.

Criterios de aceptación:
- el usuario introduce un título y/o contenido;
- la tarea se guarda en la base de datos;
- la nueva tarea aparece inmediatamente en la lista;
- el sistema valida que la información obligatoria no esté vacía.

### RF-02: Consultar lista de tareas
El usuario puede ver todas las tareas registradas.

Criterios de aceptación:
- la lista se carga desde la API;
- cada tarea muestra su estado actual;
- la información se presenta de forma legible.

### RF-03: Marcar tarea como completada
El usuario puede cambiar el estado de una tarea a completada o pendiente.

Criterios de aceptación:
- la actualización se refleja en la base de datos;
- la interfaz refleja el cambio de estado sin recarga completa;
- la tarea conserva su historial básico de edición.

### RF-04: Editar tarea
El usuario puede modificar el contenido de una tarea existente.

Criterios de aceptación:
- la edición se realiza desde la UI;
- los cambios se guardan en la base de datos;
- la tarea actualizada reemplaza la versión anterior en la vista.

### RF-05: Eliminar tarea
El usuario puede eliminar una tarea.

Criterios de aceptación:
- la tarea desaparece de la lista;
- la eliminación se refleja en la base de datos;
- la operación confirma la intención del usuario si aplica.

### RF-06: Persistencia de datos
La aplicación debe guardar la información de las tareas en SQLite.

Criterios de aceptación:
- la base de datos se crea o actualiza mediante migraciones;
- las tareas resisten reinicios de la aplicación;
- los datos se mantienen en almacenamiento local.

### RF-07: Acceso desde frontend
El frontend debe consumir la API REST del backend.

Criterios de aceptación:
- la capa frontend realiza peticiones HTTP válidas;
- los errores de red o del servidor se gestionan de forma visible para el usuario;
- la experiencia de uso no rompe la interacción si falla una operación.

## 8. Historias de usuario

### HU-01: Crear una tarea nueva
Como usuario quiero poder crear una tarea rápidamente para registrar actividades pendientes.

### HU-02: Revisar mi lista de tareas
Como usuario quiero ver todas mis tareas para tener un panorama claro de lo que debo hacer.

### HU-03: Completar una tarea
Como usuario quiero marcar una tarea como completada para tener una visión del progreso.

### HU-04: Corregir una tarea
Como usuario quiero editar una tarea si cambió su descripción o su estado.

### HU-05: Eliminar una tarea equivocada
Como usuario quiero borrar tareas que ya no necesito para mantener mi lista ordenada.

## 9. Reglas de negocio

- todo dato obligatorio debe validarse antes de guardarse;
- una tarea debe tener al menos un identificador único y un contenido mínimo válido;
- una tarea puede estar en estado pendiente o completada;
- la eliminación debe ser persistente y visible en la UI;
- la base de datos debe mantener integridad de datos por medio de modelos y migraciones;
- la aplicación debe operar sin necesidad de infraestructura externa compleja.

## 10. Experiencia de usuario

La interfaz debe ser:

- limpia y fácil de navegar;
- de alta legibilidad;
- accesible a nivel básico;
- responsiva para diferentes tamaños de pantalla.

La interacción primaria debe centrarse en cuatro acciones rápidas: crear, completar, editar y eliminar. La experiencia debe permitir que un usuario sin experiencia previa comprenda el flujo en pocos segundos.

## 11. Modelo de datos

Se propone una estructura base para la entidad de tarea:

```csharp
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Campos sugeridos

- Id: identificador único;
- Title: nombre o descripción breve de la tarea;
- IsCompleted: estado de la tarea;
- CreatedAt: fecha de registro.

## 12. API REST propuesta

La API podría exponer los siguientes endpoints:

```http
GET /api/todos
GET /api/todos/{id}
POST /api/todos
PUT /api/todos/{id}
DELETE /api/todos/{id}
```

### Ejemplos

#### Obtener tareas
```http
GET /api/todos
```

#### Crear tarea
```http
POST /api/todos
Content-Type: application/json

{
  "title": "Terminar el proyecto final",
  "isCompleted": false
}
```

#### Actualizar tarea
```http
PUT /api/todos/1
Content-Type: application/json

{
  "title": "Terminar el proyecto final",
  "isCompleted": true
}
```

#### Eliminar tarea
```http
DELETE /api/todos/1
```

## 13. Arquitectura propuesta

### Backend
- .NET 10
- ASP.NET Core Web API
- SQLite
- Entity Framework Core
- migraciones para versionado del esquema

### Frontend
- React
- Vite
- consumo de la API mediante HTTP
- UI simple para la gestión de tareas

### Topología general

```text
[Frontend React]
       |
       v
[API ASP.NET Core]
       |
       v
[SQLite Database]
```

## 14. Criterios de aceptación del producto

La solución se considera exitosa si:

- el usuario puede crear tareas desde la interfaz web;
- el usuario puede visualizar la lista actualizada;
- las tareas se mantienen persistidas en la base de datos;
- las operaciones de edición y borrado funcionan correctamente;
- la API responde de forma consistente para las operaciones principales;
- el proyecto puede ejecutarse localmente con una configuración básica.

## 15. Requisitos no funcionales

### Rendimiento
- la carga de la lista debe ser inmediata en entornos locales;
- las operaciones CRUD deben responder sin retrasos visibles.

### Seguridad
- se debe validar la entrada del usuario;
- la aplicación debe evitar procesamiento de datos inválidos;
- se recomienda preparar la base para seguridad posterior, como autenticación y validaciones avanzadas.

### Mantenibilidad
- separación de responsabilidades entre backend y frontend;
- uso de patrones claros y documentación mínima;
- estructura de carpetas simple y comprensible.

### Escalabilidad
- el proyecto debe permitir evolucionar con nuevas funcionalidades sin reescribir toda la base.

## 16. Riesgos y supuestos

### Riesgos
- dependencia de bibliotecas y herramientas que requieran instalación específica;
- errores de integración entre frontend y backend;
- cambios de estructura del modelo sin migración correcta.

### Supuestos
- el proyecto será ejecutado en un entorno local de desarrollo;
- los usuarios principales son personas con un uso básico de aplicaciones web;
- la aplicación se utilizará como base educativa y experimental.

## 17. Roadmap sugerido

### Fase 1: Base funcional
- configuración del backend
- modelo de datos
- persistencia con SQLite
- API REST básica

### Fase 2: Frontend
- UI de lista de tareas
- crear, editar y borrar tareas
- integración con la API

### Fase 3: Validación
- pruebas manuales de flujo completo
- corrección de errores de UX
- mejora de validaciones

### Fase 4: Evolución
- filtros por estado
- ordenamiento por fecha o prioridad
- autenticación de usuarios
- tests automatizados

## 18. Conclusión

La Todo App es una solución educativa y funcional de gestión de tareas que combina tecnologías modernas del stack .NET y React. Su objetivo principal es demostrar cómo construir una aplicación full-stack completa, con persistencia real, arquitectura simple y una experiencia de usuario clara.

El producto cumple con los principios básicos de un MVP: resolver un problema real con una interfaz sencilla, un backend robusto y una base de datos local confiable. Esta base puede extenderse con nuevas funcionalidades y escenarios más avanzados en futuras iteraciones.

## 19. Cierre del PRD

Este PRD define la visión, alcance, requisitos y criterios de éxito del proyecto. Sirve como referencia para el diseño, implementación y validación del MVP de la aplicación Todo App.
