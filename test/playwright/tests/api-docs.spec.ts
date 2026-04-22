import { expect, test } from '@playwright/test';

const defaultBaseUrl = process.env.DOCFORGE_BASE_URL ?? 'https://docforge.lab.rvmtech.com.br';

test.describe('DocForge — API Docs', () => {
  test.skip(
    process.env.DOCFORGE_RUN_SMOKE !== '1',
    'Defina DOCFORGE_RUN_SMOKE=1 para rodar o smoke contra um ambiente real.',
  );

  test('GET /api/projects — retorna lista ou exige autenticação', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/projects`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/documents — retorna lista ou exige autenticação', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/documents`);
    expect([200, 401]).toContain(response.status());
  });

  test('GET /api/analysis — retorna lista ou exige autenticação', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.get(`${currentBaseUrl}/api/analysis`);
    expect([200, 401]).toContain(response.status());
  });

  test('POST /api/projects sem body — retorna 400 ou 401', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.post(`${currentBaseUrl}/api/projects`);
    expect([400, 401]).toContain(response.status());
  });

  test('POST /api/documents sem body — retorna 400 ou 401', async ({ request, baseURL }) => {
    const currentBaseUrl = baseURL ?? defaultBaseUrl;
    const response = await request.post(`${currentBaseUrl}/api/documents`);
    expect([400, 401]).toContain(response.status());
  });
});
