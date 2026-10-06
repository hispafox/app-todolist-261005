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
- Consultar la lista de tareas desde una interfaz web

Es una base muy útil para practicar integración entre frontend y backend, manejo de persistencia, migraciones y despliegue local.

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

## Modelo de datos

Una tarea puede tener una estructura similar a esta:

```csharp
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

## Endpoints de ejemplo

La API podría exponer endpoints como:

```http
GET /api/todos
GET /api/todos/{id}
POST /api/todos
PUT /api/todos/{id}
DELETE /api/todos/{id}
```

Ejemplo de creación de una tarea:

```http
POST /api/todos
Content-Type: application/json

{
  "title": "Terminar el proyecto final",
  "isCompleted": false
}
```

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

El PRD de la aplicación está disponible en [docs/PRD.md](docs/PRD.md).

### Manual de usuario en Word

El skill [manual-usuario](.github/skills/manual-usuario/SKILL.md) automatiza la creación o actualización del manual en español, basándose en las funcionalidades realmente implementadas y reutilizando el skill [docx](.github/skills/docx/SKILL.md).

Para ejecutarlo, pide a Copilot: «Usa manual-usuario para crear el manual de la aplicación en Word». La salida predeterminada es `docs/manual-usuario.docx`; también puedes indicar otra ruta. El flujo incluye comprobación del contenido y validación del documento generado.

## Licencia

Este proyecto se puede usar como base educativa. Si lo adaptas o lo compartes, te recomendamos mantener la referencia al original y documentar los cambios realizados.
