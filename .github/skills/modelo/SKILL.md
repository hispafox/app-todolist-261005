---
name: modelo
description: 'Crea o actualiza las clases del modelo de dominio de la aplicación a partir del análisis del proyecto y mantiene sincronizados únicamente los elementos relacionados que requiera el cambio.'
---

# Skill: Modelo de la aplicación

## Cuándo usar este skill

- El usuario pide crear o actualizar el modelo de datos o una entidad de dominio.
- El usuario cambia la definición de una entidad y necesita reflejar el cambio en el código.
- Se necesita generar las clases del modelo a partir del análisis vigente de la aplicación.

## Fuentes de verdad

Antes de editar, lee:

- [`README.md`](../../../README.md), para el propósito y la estructura del proyecto.
- [`docs/analisis-diseño.md`](../../../docs/analisis-diseño.md), especialmente la sección **4. Modelo de datos** y las reglas de dominio relacionadas.
- [`.github/copilot-instructions.md`](../../copilot-instructions.md), para las convenciones del repositorio.

La sección 4 del análisis es la fuente de verdad para las entidades, sus campos, tipos y relaciones. No copies ni mantengas una lista de entidades o propiedades en este skill: vuelve a leer la documentación en cada invocación. Si el análisis contradice el código, conserva la intención documentada y adapta solo lo solicitado; no implementes automáticamente todas las diferencias preexistentes. Si la petición cambia el diseño y la documentación todavía no lo refleja, actualiza primero el análisis con el flujo de documentación adecuado y después genera el código.

## Ubicación y estilo del modelo

Comprueba la estructura real antes de crear archivos. En este repositorio, las entidades C# se ubican en `backend\TodoApi\Models\` y usan el namespace `TodoApi.Models`. Mantén esa ubicación y el namespace establecidos; si el proyecto ha cambiado, sigue el patrón actual del código.

Las clases de entidad deben ser POCO sencillas: propiedades tipadas, valores por defecto apropiados y navegaciones necesarias para las relaciones documentadas. No añadas métodos de negocio, anotaciones de datos, DTOs ni capas nuevas a las entidades. Configura restricciones y relaciones de EF Core en el `DbContext` cuando sea necesario, siguiendo el patrón existente.

## Procedimiento

1. Identifica qué entidad o cambio solicita el usuario y consulta su definición actual en el análisis.
2. Inspecciona las clases de `Models`, `TodoDbContext` y sus consumidores para conservar convenciones y evitar duplicados. Genera entidades en orden de dependencias y un archivo por clase cuando corresponda.
3. Crea o modifica las clases del modelo para que coincidan con el análisis y la petición. No cambies entidades o propiedades no relacionadas.
4. Actualiza otros elementos **solo si el cambio del modelo los afecta**:
   - `backend\TodoApi\Data\TodoDbContext.cs`: conjuntos, claves, relaciones o configuración de EF Core que falten.
   - `backend\TodoApi\Migrations\`: añade una migración únicamente si cambia el esquema persistido; usa EF Core y conserva los datos existentes.
   - `backend\TodoApi\Services\` y `backend\TodoApi\Program.cs`: solo si la lógica o el contrato de los endpoints necesita adaptarse al modelo.
   - Contratos y UI en `frontend\src\`: solo si cambia la forma de los datos consumidos por React.
   - `backend\TodoApi.Tests\`: ajusta o añade pruebas cuando el comportamiento relacionado cambie.
5. Si no hay elementos relacionados que deban cambiar, limita el trabajo a crear o actualizar las clases del modelo.
6. Valida los proyectos afectados: ejecuta `dotnet build backend\TodoApi\TodoApi.csproj`; ejecuta las pruebas del backend si cambió comportamiento o hay pruebas afectadas. Si cambió un contrato TypeScript, ejecuta también la compilación del frontend.
7. Resume qué clases y elementos relacionados cambiaste, y qué validaciones ejecutaste. Señala explícitamente cualquier elemento que no se haya actualizado y por qué.

## Reglas de alcance

- No derives requisitos nuevos del README ni de funcionalidades futuras; úsalos como contexto y respeta el análisis vigente.
- No implementes en bloque carencias preexistentes detectadas al comparar el análisis con el código.
- No modifiques endpoints, servicios, migraciones, frontend o pruebas por rutina: hazlo únicamente cuando el cambio solicitado tenga impacto directo.
- No cambies el análisis desde este skill para justificar una implementación. Si se debe modificar la especificación, hazlo primero en su documento y luego regenera el modelo.
- Mantén los nombres de propiedades y tipos indicados por el análisis; aplica las convenciones C# existentes del repositorio.
