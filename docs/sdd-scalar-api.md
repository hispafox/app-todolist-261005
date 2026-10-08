# SDD (Software Design Document) — Implementar Scalar en la API

## 1. Objetivo del documento

Este documento describe la especificación de diseño para integrar la documentación interactiva de la API con Scalar en la solución backend de TodoApp.

El objetivo es proporcionar una experiencia de exploración y prueba de la API REST generada automáticamente desde el esquema OpenAPI, manteniendo la documentación solo disponible en entorno de desarrollo y evitando exponerla en producción.

## 2. Qué es un SDD

Un SDD (Software Design Document o Especificación de Diseño de Software) es un artefacto técnico que define cómo se va a resolver un requisito funcional o no funcional a nivel de arquitectura, componentes, flujo de datos y decisiones de implementación.

En este caso, el SDD describe:

- el problema a resolver,
- el alcance de la solución,
- los cambios concretos en el backend,
- las dependencias necesarias,
- la estructura de la implementación,
- la validación y el comportamiento esperado.

No es un documento de requisitos de negocio, sino de diseño técnico. Su finalidad es guiar la implementación y dejar un punto de referencia para mantenimiento, revisión y evolución de la solución.

## 3. Contexto del proyecto

El repositorio actual contiene una API REST en ASP.NET Core con Minimal API, integrada con SQLite mediante Entity Framework Core. La solución ya expone un esquema OpenAPI y usa el backend para gestionar tareas, categorías y usuarios.

La aplicación actual tiene una estructura orientada a servicios y endpoints mínimos, con arquitectura simple y directa:

- Frontend React + TypeScript
- Backend ASP.NET Core
- SQLite + EF Core
- Minimal API
- Servicios de dominio dentro de `TodoApi.Services`

La funcionalidad a diseñar consiste en añadir una interfaz visual para explorar la API mediante Scalar, compatible con el esquema OpenAPI que ya genera la aplicación.

## 4. Objetivo funcional

Implementar una vista interactiva para probar la API en local, con la siguiente finalidad:

- documentar la API generada desde el código,
- facilitar pruebas manuales sin necesidad de utilizar Postman ni curl,
- activar la exploración con `Try it` y ejemplos de llamadas,
- mantener la documentación fuera de producción.

## 5. Alcance

### 5.1 Incluido

- Añadir la dependencia `Scalar.AspNetCore`.
- Configurar la generación del esquema OpenAPI con `AddOpenApi()`.
- Mapear la documentación de Scalar con `MapScalarApiReference()`.
- Restringir la exposición en entorno de desarrollo.
- Validar la ruta `/scalar` desde el entorno local.
- Documentar el comportamiento de la solución.

### 5.2 No incluido

- Autenticación o autorización.
- Nuevas entidades de negocio.
- Cambios de contrato de endpoints ya existentes.
- Publicación de la documentación en producción.
- Aumentar complejidad arquitectónica innecesaria.

## 6. Requisitos

### 6.1 Requisitos funcionales

1. La API debe seguir generando su esquema OpenAPI.
2. La documentación de la API debe estar disponible en entorno de desarrollo.
3. La interfaz de Scalar debe estar accesible mediante una ruta estándar, normalmente `/scalar`.
4. La documentación debe reflejar automáticamente los endpoints del backend.
5. La solución debe seguir permitiendo el consumo desde el frontend.

### 6.2 Requisitos no funcionales

- Simplicidad: solución directa y fácil de mantener.
- Seguridad: no exponer documentación fuera de desarrollo.
- Compatibilidad: mantener el stack actual de ASP.NET Core y .NET 10.
- Mantenibilidad: no crear capas adicionales ni lógica duplicada.

## 7. Diseño propuesto

### 7.1 Arquitectura

La integración de Scalar se realiza dentro del mismo backend API con una modificación mínima en `Program.cs`.

La solución sigue el patrón de Minimal API existente:

- `builder.Services.AddOpenApi()` registra el esquema OpenAPI.
- `app.MapOpenApi()` expone el esquema de la API.
- `app.MapScalarApiReference()` crea la interfaz gráfica documental.
- El acceso se protege con `if (app.Environment.IsDevelopment())`.

### 7.2 Diagrama de flujo

```text
Cliente (navegador)
       |
       v
 /scalar
       |
       v
 ASP.NET Core + Scalar
       |
       v
 OpenAPI schema generado por la API
       |
       v
 Endpoints del backend
```

### 7.3 Cambio de diseño en `Program.cs`

Se añade el bloque de configuración dentro del entorno de desarrollo:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

Esto asegura que la documentación no se publique cuando la aplicación se despliega en producción.

### 7.4 Cambio de diseño en el proyecto

Se añadirá la referencia al paquete NuGet:

```xml
<PackageReference Include="Scalar.AspNetCore" Version="x.y.z" />
```

La versión concreta debe ser la compatible con el target framework del proyecto y con la versión de ASP.NET Core utilizada.

## 8. Especificación técnica

### 8.1 Dependencia

El paquete `Scalar.AspNetCore` permite integrar la interfaz de Scalar con la API ASP.NET Core y consumir el esquema OpenAPI generado por el sistema.

### 8.2 Configuración de OpenAPI

La API ya registra el generador del esquema de OpenAPI mediante:

```csharp
builder.Services.AddOpenApi();
```

Esto genera el contrato de la API basado en los endpoints existentes y los tipos del modelo de la aplicación.

### 8.3 Registro de Scalar

En desarrollo se habilita el endpoint de documentación con:

```csharp
app.MapScalarApiReference();
```

Esto pone a disposición una interfaz web en la ruta `/scalar` donde el desarrollador puede:

- ver todos los endpoints,
- inspeccionar la estructura del schema,
- probar peticiones con `Try it`,
- validar casos de uso en tiempo real.

## 9. Comportamiento esperado

### 9.1 En entorno de desarrollo

Cuando la aplicación se ejecuta en local con `ASPNETCORE_ENVIRONMENT=Development`:

- `OpenAPI` está activo,
- la ruta `/scalar` está disponible,
- la documentación refleja el estado actual del backend,
- el desarrollador puede probar los endpoints desde navegador.

### 9.2 En entorno de producción

Cuando la aplicación se ejecuta fuera del entorno de desarrollo:

- no se expone el esquema OpenAPI,
- no se habilita la ruta `/scalar`,
- la API sigue funcionando sin documentar en producción.

Esto es una buena práctica de seguridad y reduce la superficie de exposición de la aplicación.

## 10. Criterios de aceptación

Se considerará implementado correctamente si se cumplen las siguientes condiciones:

1. La solución compila con la nueva dependencia.
2. La aplicación arranca en el entorno correcto.
3. El endpoint `/scalar` está disponible en desarrollo.
4. La documentación representa la API actual.
5. La documentación no aparece en producción.
6. No se introducen cambios innecesarios en la lógica de negocio.

## 11. Ejemplo de implementación

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

Con esto, la API queda documentada y navegable en local sin comprometer la seguridad del entorno de despliegue.

## 12. Riesgos y consideraciones

- Si `MapOpenApi()` y `MapScalarApiReference()` se ubicaran fuera del `if (app.Environment.IsDevelopment())`, la documentación quedaría activa también en producción.
- Debe comprobarse que el paquete NuGet es compatible con la versión de ASP.NET Core del proyecto.
- Si cambia la API, la documentación se actualiza automáticamente siempre que el esquema OpenAPI siga generándose correctamente.

## 13. Validación propuesta

Para validar la implementación:

1. restaurar paquetes NuGet,
2. compilar la API,
3. arrancar la aplicación en modo desarrollo,
4. abrir la ruta `/scalar`,
5. comprobar que se cargan los endpoints,
6. probar un endpoint real con `Try it`,
7. verificar que la ruta no está disponible en producción.

## 14. Conclusión

La integración de Scalar en la API es una mejora de documentación y usabilidad sin introducir complejidad arquitectónica. Aprovecha el esquema OpenAPI ya generado por ASP.NET Core y lo transforma en una experiencia interactiva y clara para desarrolladores.

La decisión de limitar la documentación al entorno de desarrollo es una práctica recomendada, simple y segura, y encaja perfectamente con la filosofía del proyecto: mantener una solución clara, útil y mantenible.

## 15. Resumen ejecutivo

- Problema: la API no tiene una interfaz visual de exploración y prueba.
- Solución: integrar Scalar con OpenAPI.
- Cambios principales: añadir el paquete y mapear la API para desarrollo.
- Beneficio: mejor experiencia de desarrollo y testing sin exposición innecesaria.
- Decisión clave: activar Scalar solo en desarrollo.
