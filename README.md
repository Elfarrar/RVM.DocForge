*[English](README.en.md) | **Portugues***

# RVM.DocForge

Servico API que analisa repositorios C# via Roslyn e gera documentacao tecnica automatizada em 7 formatos.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-30%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## Sobre

RVM.DocForge e um servico API que analisa automaticamente repositorios C# e gera documentacao abrangente. Utiliza Roslyn para analise estatica de codigo, extrai endpoints de API, entidades de dominio e servicos, e entao gera multiplos formatos de documentacao: README, API Reference, Entity Schema, Architecture Overview, Dependency Graph, Service Catalog e Full Documentation. O sistema persiste snapshots de analise e documentos gerados em PostgreSQL para rastreamento historico.

## Tecnologias

| Camada           | Tecnologia                                   |
|------------------|----------------------------------------------|
| Runtime          | .NET 10 / ASP.NET Core 10                    |
| Analise de codigo| Roslyn (Microsoft.CodeAnalysis) 5.0           |
| Geracao Markdown | Markdig 0.40                                 |
| ORM              | Entity Framework Core 10                     |
| Banco de dados   | PostgreSQL (Npgsql 10.0)                     |
| Logging          | Serilog (AspNetCore 10.0, Compact 3.0)       |
| Autenticacao     | API Key customizada                          |
| Testes           | xUnit 2.9 + Moq 4.20 + EF Core InMemory     |
| Cobertura        | Coverlet 6.0                                 |

## Arquitetura

```
┌──────────────────────────────────────────────────┐
│                   API Layer                      │
│          RVM.DocForge.API (net10.0)              │
│  Controllers ─ Services ─ Generators ─ Roslyn   │
│  Auth ─ Middleware ─ Health ─ DTOs               │
├──────────────────────────────────────────────────┤
│                 Domain Layer                     │
│         RVM.DocForge.Domain (net10.0)            │
│     Entities ─ Enums ─ Interfaces ─ Models       │
├──────────────────────────────────────────────────┤
│             Infrastructure Layer                 │
│      RVM.DocForge.Infrastructure (net10.0)       │
│   DbContext ─ Configurations ─ Repositories      │
└──────────────────────────────────────────────────┘
```

**Patterns:** Clean Architecture (3 camadas), Strategy Pattern (7 Document Generators), Visitor Pattern (CSharpSyntaxWalker), Repository Pattern, Dependency Injection

## Estrutura do Projeto

```
RVM.DocForge/
├── src/
│   ├── RVM.DocForge.API/
│   │   ├── Auth/
│   │   │   ├── ApiKeyAuthHandler.cs
│   │   │   └── ApiKeyAuthOptions.cs
│   │   ├── Controllers/
│   │   │   ├── AnalysisController.cs
│   │   │   ├── DocumentsController.cs
│   │   │   └── ProjectsController.cs
│   │   ├── Dtos/
│   │   │   ├── AnalysisDtos.cs
│   │   │   ├── DocumentDtos.cs
│   │   │   └── ProjectDtos.cs
│   │   ├── Health/
│   │   │   └── DatabaseHealthCheck.cs
│   │   ├── Middleware/
│   │   │   └── CorrelationIdMiddleware.cs
│   │   ├── Services/
│   │   │   ├── Generators/
│   │   │   │   ├── IDocumentGenerator.cs
│   │   │   │   ├── ReadmeGenerator.cs
│   │   │   │   ├── ApiReferenceGenerator.cs
│   │   │   │   ├── EntitySchemaGenerator.cs
│   │   │   │   ├── ArchitectureOverviewGenerator.cs
│   │   │   │   ├── DependencyGraphGenerator.cs
│   │   │   │   ├── ServiceCatalogGenerator.cs
│   │   │   │   └── FullDocumentationGenerator.cs
│   │   │   ├── Roslyn/
│   │   │   │   ├── EndpointExtractor.cs
│   │   │   │   ├── EntityExtractor.cs
│   │   │   │   └── ServiceExtractor.cs
│   │   │   ├── DocumentationOrchestrator.cs
│   │   │   └── RepositoryAnalyzerService.cs
│   │   ├── Components/
│   │   │   ├── Layout/
│   │   │   └── Pages/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── RVM.DocForge.Domain/
│   │   ├── Entities/
│   │   │   ├── DocumentationProject.cs
│   │   │   ├── ProjectSnapshot.cs
│   │   │   ├── GeneratedDocument.cs
│   │   │   ├── DiscoveredEndpoint.cs
│   │   │   ├── DiscoveredEntity.cs
│   │   │   └── DiscoveredService.cs
│   │   ├── Enums/
│   │   │   ├── DocumentType.cs
│   │   │   ├── DocumentationStatus.cs
│   │   │   └── OutputFormat.cs
│   │   ├── Interfaces/
│   │   │   ├── IDocumentationProjectRepository.cs
│   │   │   ├── IGeneratedDocumentRepository.cs
│   │   │   └── IProjectSnapshotRepository.cs
│   │   └── Models/
│   │       └── AnalysisModels.cs
│   └── RVM.DocForge.Infrastructure/
│       ├── Data/
│       │   ├── DocForgeDbContext.cs
│       │   └── Configurations/
│       │       ├── DiscoveredItemsConfiguration.cs
│       │       ├── DocumentationProjectConfiguration.cs
│       │       └── ProjectSnapshotConfiguration.cs
│       ├── Repositories/
│       │   ├── DocumentationProjectRepository.cs
│       │   ├── GeneratedDocumentRepository.cs
│       │   └── ProjectSnapshotRepository.cs
│       └── DependencyInjection.cs
├── test/
│   └── RVM.DocForge.Test/
│       ├── Generators/
│       │   └── GeneratorTests.cs
│       ├── Repositories/
│       │   └── RepositoryTests.cs
│       └── Roslyn/
│           ├── EndpointExtractorTests.cs
│           ├── EntityExtractorTests.cs
│           └── ServiceExtractorTests.cs
├── docker-compose.dev.yml
├── docker-compose.prod.yml
├── global.json
└── RVM.DocForge.slnx
```

## Como Executar

### Pre-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (10.0.201+)
- [PostgreSQL](https://www.postgresql.org/) 15+
- Docker (opcional, para docker-compose)

### 1. Clonar o repositorio

```bash
git clone https://github.com/rvenegas5/RVM.DocForge.git
cd RVM.DocForge
```

### 2. Configurar banco de dados

Edite `src/RVM.DocForge.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=docforge;Username=postgres;Password=SuaSenha"
  },
  "ApiKeys": {
    "Keys": ["sua-api-key-aqui"]
  }
}
```

Ou via Docker Compose:

```bash
docker compose -f docker-compose.dev.yml up -d
```

### 3. Build e execucao

```bash
dotnet build
dotnet run --project src/RVM.DocForge.API
```

A API estara disponivel em `https://localhost:5001`.

## Endpoints da API

| Metodo   | Rota                                    | Descricao                        |
|----------|-----------------------------------------|----------------------------------|
| `POST`   | `/api/analysis/{projectId}`             | Analisar repositorio             |
| `GET`    | `/api/analysis/snapshots/{id}`          | Detalhes do snapshot             |
| `GET`    | `/api/analysis/project/{projectId}/snapshots` | Listar snapshots do projeto |
| `GET`    | `/api/projects`                         | Listar projetos                  |
| `GET`    | `/api/projects/{id}`                    | Detalhes do projeto              |
| `POST`   | `/api/projects`                         | Criar projeto                    |
| `PUT`    | `/api/projects/{id}`                    | Atualizar projeto                |
| `DELETE` | `/api/projects/{id}`                    | Excluir projeto                  |
| `POST`   | `/api/documents/generate`              | Gerar documentacao               |
| `GET`    | `/api/documents/project/{projectId}`   | Listar documentos do projeto     |
| `GET`    | `/api/documents/{id}`                  | Detalhes do documento            |
| `GET`    | `/api/documents/{id}/raw`              | Markdown bruto                   |
| `DELETE` | `/api/documents/{id}`                  | Excluir documento                |
| `DELETE` | `/api/documents/project/{projectId}`   | Excluir documentos do projeto    |
| `GET`    | `/health`                              | Health check                     |

> Todos os endpoints (exceto `/health`) requerem autenticacao via API Key.

## Testes

```bash
dotnet test
```

**30 testes** organizados em 3 suites:

| Suite                    | Arquivo                      | Testes | Cobertura                                    |
|--------------------------|------------------------------|--------|----------------------------------------------|
| GeneratorTests           | `GeneratorTests.cs`          | 8      | 7 geradores de documentacao + cenario vazio   |
| RepositoryTests          | `RepositoryTests.cs`         | 4      | CRUD de projetos, snapshots e documentos      |
| EndpointExtractorTests   | `EndpointExtractorTests.cs`  | 6      | Extracao de rotas HTTP via Roslyn             |
| EntityExtractorTests     | `EntityExtractorTests.cs`    | 7      | Extracao de classes, records, enums           |
| ServiceExtractorTests    | `ServiceExtractorTests.cs`   | 5      | Extracao de interfaces e lifetimes DI         |

## Funcionalidades

- [x] Analise automatizada de repositorios usando Roslyn
- [x] Geracao de documentacao em 7 formatos (README, API Reference, Entity Schema, Architecture Overview, Dependency Graph, Service Catalog, Full Documentation)
- [x] Rastreamento de snapshots com historico
- [x] Extracao de endpoints de API (HttpGet, HttpPost, HttpPut, HttpDelete)
- [x] Descoberta de entidades (classes, records, enums)
- [x] Catalogo de servicos com deteccao de lifetime (Scoped, Singleton, Transient)
- [x] Geracao de Markdown com Markdig
- [x] Autenticacao por API Key com rate limiting
- [x] Health check de banco de dados
- [x] Correlation ID por requisicao
- [x] Migracao automatica do banco ao iniciar
- [x] Logging estruturado com Serilog (JSON compacto)
- [x] Suporte a reverse proxy (Forwarded Headers)
- [x] Docker Compose para dev e prod

---

Desenvolvido por **RVM Tech**
