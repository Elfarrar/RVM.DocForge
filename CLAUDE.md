# RVM.DocForge

## Visao Geral
Gerador automatico de documentacao tecnica para projetos .NET. Analisa repositorios via Roslyn, extrai XML docs, interfaces, classes e metodos publicos, e produz documentacao em 7 formatos diferentes usando templates Scriban.

Projeto portfolio demonstrando uso de Roslyn para processamento de codigo, git clone de repos externos, orquestracao de pipeline de extracao, e rendering multi-formato.

## Stack
- .NET 10, ASP.NET Core, Blazor Server
- Roslyn (Microsoft.CodeAnalysis) para analise de codigo
- Scriban (templates) para rendering de documentacao
- LibGit2Sharp (git clone de repositorios externos)
- Entity Framework Core + PostgreSQL (historico de geracoes)
- Serilog + Seq, RVM.Common.Security
- Autenticacao via API Key (`ApiKeyAuthHandler`)
- Rate limiting: 120 req/min global, 200 req/min por API Key
- xUnit 82 testes, Playwright E2E

## Estrutura do Projeto
```
src/
  RVM.DocForge.API/           # Host: Blazor Server + REST controllers
    Auth/                     # ApiKeyAuthOptions + ApiKeyAuthHandler
    Components/               # Blazor pages (dashboard, projetos, preview)
    Controllers/              # Endpoints de geracao e listagem
    Health/                   # DatabaseHealthCheck
    Middleware/               # CorrelationIdMiddleware
    Services/
      GitCloneService         # Clona repos externos para analise
      RepositoryAnalyzerService # Analisa codigo via Roslyn
      DocumentationOrchestrator # Coordena pipeline completo
  RVM.DocForge.Domain/        # Entidades (Project, Document, GenerationJob)
  RVM.DocForge.Infrastructure/
    Data/                     # DocForgeDbContext (EF Core)
    Repositories/             # IProjectRepository, IDocumentRepository
test/
  RVM.DocForge.Test/          # xUnit (82 testes)
  playwright/                 # Testes E2E
```

## Convencoes
- `DocumentationOrchestrator` orquestra: clone → analise → render → persistencia
- Autenticacao exclusiva via API Key (sem sessao/cookies para endpoints REST)
- Blazor admin usa autenticacao de sessao separada
- `EnsureCreated` para dev; migration explica EF Core em producao
- Data Protection persiste chaves em diretorio configuravel (`DataProtection:Directory`)
- PathBase configuravel para deploy atras de reverse proxy

## Como Rodar
### Dev
```bash
# Subir PostgreSQL
docker compose -f docker-compose.dev.yml up -d

# API + Blazor
cd src/RVM.DocForge.API
dotnet run
```

### Testes
```bash
dotnet test test/RVM.DocForge.Test/
```

## Decisoes Arquiteturais
- **7 formatos de saida**: Markdown, HTML, PDF, JSON, OpenAPI, docx, AsciiDoc — demonstra flexibilidade do pipeline Scriban
- **GitCloneService**: permite analisar repos publicos sem copia local — cenario comum em geradores de docs
- **DocumentationOrchestrator como servico scoped**: cada request de geracao e isolado, sem estado compartilhado
- **API Key como unico esquema**: simplifica integracao M2M e demonstra auth sem OIDC
