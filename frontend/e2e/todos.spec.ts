import { test, expect } from '@playwright/test'
import { mkdir } from 'node:fs/promises'
import path from 'node:path'

// Carpeta donde se guardan las capturas para el manual de usuario.
const CAPTURAS_DIR = path.join('..', 'docs', 'capturas')

const captura = (nombre: string) => path.join(CAPTURAS_DIR, nombre)

test.beforeAll(async () => {
  await mkdir(CAPTURAS_DIR, { recursive: true })
})

test('Flujo principal de tareas con capturas para el manual', async ({ page }) => {
  const tituloTarea = `Preparar informe ${Date.now()}`
  const tituloEditado = `${tituloTarea} (revisado)`

  // 1. Panel principal
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'Tu trabajo, en orden.' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Mis tareas' })).toBeVisible()
  // Esperar a que terminen las cargas para que las capturas no muestren estados "Cargando...".
  await expect(page.getByText('Cargando tareas...')).toHaveCount(0)
  await expect(page.getByText('Cargando usuarios...')).toHaveCount(0)
  await page.screenshot({ path: captura('01-panel-tareas.png'), fullPage: true })

  // 2. Rellenar el formulario de nueva tarea
  await page.getByRole('textbox', { name: 'Nueva tarea' }).fill(tituloTarea)
  await page.screenshot({ path: captura('02-crear-tarea.png'), fullPage: true })

  // 3. La tarea aparece en la lista
  await page.getByRole('button', { name: 'Añadir tarea' }).click()
  const tarea = page.locator('.todo-item', { hasText: tituloTarea })
  await expect(tarea).toBeVisible()
  await page.screenshot({ path: captura('03-tarea-creada.png'), fullPage: true })

  // 4. Marcar la tarea como completada
  await tarea.getByRole('checkbox').click()
  await expect(tarea).toHaveClass(/completed/)
  await page.screenshot({ path: captura('04-tarea-completada.png'), fullPage: true })

  // 5. Editar el título de la tarea
  await tarea.getByRole('button', { name: 'Editar' }).click()
  const formularioEdicion = page.locator('.todo-edit')
  await formularioEdicion.locator('input[type="text"]').fill(tituloEditado)
  await page.screenshot({ path: captura('05-editar-tarea.png'), fullPage: true })
  await formularioEdicion.getByRole('button', { name: 'Guardar' }).click()
  await expect(page.locator('.todo-item', { hasText: tituloEditado })).toBeVisible()

  // 6. Secciones de catálogo (categorías y usuarios)
  await expect(page.getByRole('heading', { name: 'Categorías' })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Usuarios' })).toBeVisible()
  await page.screenshot({ path: captura('06-catalogo-categorias-usuarios.png'), fullPage: true })

  // Limpieza: eliminar la tarea creada para no dejar datos de prueba
  const tareaEditada = page.locator('.todo-item', { hasText: tituloEditado })
  await tareaEditada.getByRole('button', { name: 'Eliminar' }).click()
  await expect(page.locator('.todo-item', { hasText: tituloEditado })).toHaveCount(0)
})
