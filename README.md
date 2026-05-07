# ApiSpark

ApiSpark is a modular ASP.NET Core backend API platform for small personal and portfolio APIs. It is designed to consolidate low-volume APIs into a single Azure-hosted backend while keeping public websites static-first and inexpensive.

## Goals

- Host multiple small APIs in one ASP.NET Core application
- Use EF Core + SQLite as the default low-cost relational backend
- Use Cosmos DB selectively for document-oriented examples
- Support Azure Static Web Apps clients
- Provide browser-based CMS/admin capabilities
- Generate static JSON for public content websites
- Keep hosting simple, portable, and inexpensive

## Target Runtime

- .NET 10 LTS
- Azure App Service Linux B1
- SQLite under persistent App Service storage (`/home/data/apispark.db`)
- Azure Static Web Apps for public clients

## Route Areas

- `/api/public/*` — anonymous read-only APIs
- `/api/admin/*` — authenticated CMS/admin APIs (requires Admin role)
- `/api/publish/*` — authenticated publishing/export APIs
- `/api/integrations/*` — authenticated integration APIs
- `/api/health` — shallow health check (anonymous)
- `/api/admin/health/deep` — deep health check (Admin only)

## Quick Start

```bash
git clone https://github.com/MarkHazleton/ApiSpark.git
cd ApiSpark
dotnet run --project src/ApiSpark.Api
```

See [quickstart.md](.documentation/specs/001-apispark-foundation/quickstart.md) for full local setup instructions.

## Architecture

See [docs/decisions/](docs/decisions/) for Architecture Decision Records.

## Constitution

See [.documentation/memory/constitution.md](.documentation/memory/constitution.md) for non-negotiable project principles.
