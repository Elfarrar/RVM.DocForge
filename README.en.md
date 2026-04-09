***English** | [Portugues](README.md)*

# RVM.DocForge

API service that analyzes C# repositories via Roslyn and generates automated technical documentation in 7 formats.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-30%20passed-brightgreen)
![license](https://img.shields.io/badge/license-MIT-blue)
![dotnet](https://img.shields.io/badge/.NET-10.0-purple)

---

## About

RVM.DocForge is an API service that automatically analyzes C# repositories and generates comprehensive documentation. It uses Roslyn for static code analysis, extracts API endpoints, domain entities and services, then generates multiple documentation formats: README, API Reference, Entity Schema, Architecture Overview, Dependency Graph, Service Catalog and Full Documentation. The system persists analysis snapshots and generated documents in PostgreSQL for historical tracking.

## Technologies

| Layer            | Technology                                   |
|------------------|----------------------------------------------|
| Runtime          | .NET 10 / ASP.NET Core 10                    |
| Code Analysis    | Roslyn (Microsoft.CodeAnalysis) 5.0           |
| Markdown Gen     | Markdig 0.40                                 |
| ORM              | Entity Framework Core 10                     |
| Database         | PostgreSQL (Npgsql 10.0)                     |
| Logging          | Serilog (AspNetCore 10.0, Compact 3.0)       |
| Authentication   | Custom API Key                               |
| Testing          | xUnit 2.9 + Moq 4.20 + EF Core InMemory     |
| Coverage         | Coverlet 6.0                                 |

## Architecture

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

**Patterns:** Clean Architecture (3 layers), Strategy Pattern (7 Document Generators), Visitor Pattern (CSharpSyntaxWalker), Repository Pattern, Dependency Injection

## Project Structure

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

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (10.0.201+)
- [PostgreSQL](https://www.postgresql.org/) 15+
- Docker (optional, for docker-compose)

### 1. Clone the repository

```bash
git clone https://github.com/rvenegas5/RVM.DocForge.git
cd RVM.DocForge
```

### 2. Configure the database

Edit `src/RVM.DocForge.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=docforge;Username=postgres;Password=YourPassword"
  },
  "ApiKeys": {
    "Keys": ["your-api-key-here"]
  }
}
```

Or via Docker Compose:

```bash
docker compose -f docker-compose.dev.yml up -d
```

### 3. Build and run

```bash
dotnet build
dotnet run --project src/RVM.DocForge.API
```

The API will be available at `https://localhost:5001`.

## API Endpoints

| Method   | Route                                   | Description                      |
|----------|-----------------------------------------|----------------------------------|
| `POST`   | `/api/analysis/{projectId}`             | Analyze repository               |
| `GET`    | `/api/analysis/snapshots/{id}`          | Snapshot details                 |
| `GET`    | `/api/analysis/project/{projectId}/snapshots` | List project snapshots     |
| `GET`    | `/api/projects`                         | List projects                    |
| `GET`    | `/api/projects/{id}`                    | Project details                  |
| `POST`   | `/api/projects`                         | Create project                   |
| `PUT`    | `/api/projects/{id}`                    | Update project                   |
| `DELETE` | `/api/projects/{id}`                    | Delete project                   |
| `POST`   | `/api/documents/generate`              | Generate documentation           |
| `GET`    | `/api/documents/project/{projectId}`   | List project documents           |
| `GET`    | `/api/documents/{id}`                  | Document details                 |
| `GET`    | `/api/documents/{id}/raw`              | Raw Markdown                     |
| `DELETE` | `/api/documents/{id}`                  | Delete document                  |
| `DELETE` | `/api/documents/project/{projectId}`   | Delete project documents         |
| `GET`    | `/health`                              | Health check                     |

> All endpoints (except `/health`) require API Key authentication.

## Tests

```bash
dotnet test
```

**30 tests** organized in 5 suites:

| Suite                    | File                         | Tests  | Coverage                                     |
|--------------------------|------------------------------|--------|----------------------------------------------|
| GeneratorTests           | `GeneratorTests.cs`          | 8      | 7 documentation generators + empty scenario  |
| RepositoryTests          | `RepositoryTests.cs`         | 4      | CRUD for projects, snapshots and documents    |
| EndpointExtractorTests   | `EndpointExtractorTests.cs`  | 6      | HTTP route extraction via Roslyn              |
| EntityExtractorTests     | `EntityExtractorTests.cs`    | 7      | Class, record and enum extraction             |
| ServiceExtractorTests    | `ServiceExtractorTests.cs`   | 5      | Interface and DI lifetime extraction          |

## Features

- [x] Automated repository analysis using Roslyn
- [x] Documentation generation in 7 formats (README, API Reference, Entity Schema, Architecture Overview, Dependency Graph, Service Catalog, Full Documentation)
- [x] Snapshot tracking with history
- [x] API endpoint extraction (HttpGet, HttpPost, HttpPut, HttpDelete)
- [x] Entity discovery (classes, records, enums)
- [x] Service catalog with lifetime detection (Scoped, Singleton, Transient)
- [x] Markdown generation with Markdig
- [x] API Key authentication with rate limiting
- [x] Database health check
- [x] Correlation ID per request
- [x] Auto-migration on startup
- [x] Structured logging with Serilog (compact JSON)
- [x] Reverse proxy support (Forwarded Headers)
- [x] Docker Compose for dev and prod

---

Developed by **RVM Tech**
