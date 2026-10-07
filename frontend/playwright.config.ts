import { defineConfig, devices } from '@playwright/test'

// Configuración de las pruebas E2E del agente de QA.
// Levanta el backend (.NET) y el frontend (Vite) y ejecuta los flujos de la UI.
// Las capturas para el manual de usuario se guardan en docs/capturas/.
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  timeout: 60_000,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: 'http://localhost:5173',
    viewport: { width: 1280, height: 800 },
    screenshot: 'only-on-failure',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: [
    {
      command: 'dotnet run --project ../backend/TodoApi/TodoApi.csproj',
      url: 'http://localhost:5062/api/todos',
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
    },
    {
      command: 'npm run dev',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
    },
  ],
})
