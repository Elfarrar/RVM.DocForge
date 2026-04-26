# Testes — RVM.DocForge

## Testes Unitarios
- **Framework:** xUnit + Moq
- **Localizacao:** `test/RVM.DocForge.Test/`
- **Total:** 82 testes
- **Foco:** RepositoryAnalyzerService, DocumentationOrchestrator, renderers Scriban, extractors Roslyn

```bash
dotnet test test/RVM.DocForge.Test/
```

## Testes E2E (Playwright)
- **Localizacao:** `test/playwright/`
- **Cobertura:** fluxo completo via Blazor (submeter repo, aguardar geracao, download do documento)

```bash
cd test/playwright
npm install
npx playwright install --with-deps
npx playwright test
```

Variaveis de ambiente necessarias:
```
DOCFORGE_BASE_URL=http://localhost:5000
DOCFORGE_API_KEY=<api-key-dev>
```

## CI
- **Arquivo:** `.github/workflows/ci.yml`
- Pipeline: build → testes unitarios → Playwright
- Testes de analise Roslyn usam projetos .NET fixture (copiados para temp em cada teste)
