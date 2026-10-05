# Frontend de la Todo App

Este frontend está desarrollado con React + TypeScript + Vite y consume la API REST de ASP.NET Core.

## Requisitos

- Node.js 18+
- npm

## Instalación

```bash
npm install
```

## Ejecutar en local

```bash
npm run dev
```

La aplicación queda disponible en `http://localhost:5173` y proxya las peticiones `/api` hacia `http://localhost:5062`.

## Construcción de producción

```bash
npm run build
```

## Scripts disponibles

- `npm run dev`: arranca el entorno de desarrollo.
- `npm run build`: genera el bundle de producción.
- `npm run preview`: sirve la build localmente.
- `npm run lint`: ejecuta la validación con Oxlint.
