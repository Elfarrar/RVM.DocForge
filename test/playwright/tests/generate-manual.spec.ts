/**
 * RVM.DocForge — Gerador de Manual Visual
 *
 * Playwright script que navega por todas as telas do sistema de documentacao automatica,
 * captura screenshots em desktop e mobile, e gera as imagens para o manual.
 *
 * Uso:
 *   cd test/playwright
 *   npx playwright test tests/generate-manual.spec.ts --reporter=list
 */
import { test, type Page } from '@playwright/test';
import path from 'path';

const BASE_URL = process.env.DOCFORGE_BASE_URL ?? 'https://docforge.lab.rvmtech.com.br';
const SCREENSHOTS_DIR = path.resolve(__dirname, '../../../docs/screenshots');

/** Captura desktop (1280x800) + mobile (390x844) */
async function capture(page: Page, name: string, opts?: { fullPage?: boolean }) {
  const fullPage = opts?.fullPage ?? true;
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${name}--desktop.png`), fullPage });
  await page.setViewportSize({ width: 390, height: 844 });
  await page.screenshot({ path: path.join(SCREENSHOTS_DIR, `${name}--mobile.png`), fullPage });
  await page.setViewportSize({ width: 1280, height: 800 });
}

test.describe('RVM.DocForge — Manual Visual', () => {
  test('01 Home / Dashboard', async ({ page }) => {
    await page.goto(`${BASE_URL}/`);
    await page.waitForLoadState('networkidle');
    await capture(page, '01-home');
  });

  test('02 Projetos', async ({ page }) => {
    await page.goto(`${BASE_URL}/projects`);
    await page.waitForLoadState('networkidle');
    await capture(page, '02-projects');
  });

  test('03 Documentos', async ({ page }) => {
    await page.goto(`${BASE_URL}/documents`);
    await page.waitForLoadState('networkidle');
    await capture(page, '03-documents');
  });

  test('04 Gerar Documentacao (API)', async ({ page }) => {
    await page.goto(`${BASE_URL}/api/docs`);
    await page.waitForLoadState('networkidle');
    await capture(page, '04-api-docs');
  });

  test('05 Analise de Codigo', async ({ page }) => {
    await page.goto(`${BASE_URL}/api/analysis`);
    await page.waitForLoadState('networkidle');
    await capture(page, '05-analysis');
  });
});
