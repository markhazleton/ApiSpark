# Tasks: ApiSpark Platform Foundation

**Input**: Design documents from `.documentation/specs/001-apispark-foundation/`
**Branch**: `001-apispark-foundation`
**Date**: 2026-05-07
**Prerequisites**: plan.md ✅ | spec.md ✅ | research.md ✅ | data-model.md ✅ | contracts/ ✅ | quickstart.md ✅

**Spec frontmatter**: `classification: full-spec` | `risk_level: medium` | `required_gates: checklist, analyze, critic`
**Tests**: Included — the spec requires test-first development (Constitution Principle II; spec User Story 5).

## Rationale Summary

### Core Problem

No unified, low-cost platform exists for hosting personal and portfolio APIs. The codebase is currently empty (no solution, no projects, no CI).

### Decision Summary

Build one modular ASP.NET Core .NET 10 LTS backend with route groups, EF Core + SQLite, one public content feature, admin authorization placeholder boundaries, Swagger (dev only), and GitHub Actions CI/CD — proving the architecture pattern without overbuilding.

### Key Drivers

- Replace scattered API hosting with a single Azure-hosted platform
- Foundation must be clean enough for future features to extend without rework
- Every PR must pass CI (test-first is non-negotiable per constitution)

### Reviewer Guidance

Verify: route groups match constitution auth table; public endpoints anonymous and read-only; admin routes return 401; no `.db` files or secrets committed; tests cover meaningful behavior.

---

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel with other [P] tasks in the same phase (different files, no blocking dependency)
- **[Story]**: Maps task to a user story (US1–US6) from spec.md

## Path Conventions

All paths relative to repository root:
- **API project**: `src/ApiSpark.Api/`
- **Test project**: `tests/ApiSpark.Api.Tests/`
- **Workflows**: `.github/workflows/`
- **Docs**: `docs/decisions/`

---

## Phase 1: Setup (Solution Scaffold)

**Purpose**: Create the solution and project files. Nothing compiles yet.

- [ ] T001 Create `.gitignore` (standard .NET: bin/, obj/, *.user, `data/*.db`, `*.db`, `*.db-shm`, `*.db-wal`, `.env`, `publish/`)
- [ ] T002 Create `ApiSpark.sln` at repository root using `dotnet new sln -n ApiSpark`
- [ ] T003 [P] Create `src/ApiSpark.Api/ApiSpark.Api.csproj` using `dotnet new web -n ApiSpark.Api -o src/ApiSpark.Api --framework net10.0` and add to solution
- [ ] T004 [P] Create `tests/ApiSpark.Api.Tests/ApiSpark.Api.Tests.csproj` using `dotnet new xunit -n ApiSpark.Api.Tests -o tests/ApiSpark.Api.Tests --framework net10.0` and add to solution; add project reference to ApiSpark.Api
- [ ] T005 Add NuGet packages to `src/ApiSpark.Api/ApiSpark.Api.csproj`: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`, `Swashbuckle.AspNetCore`
- [ ] T006 Add NuGet packages to `tests/ApiSpark.Api.Tests/ApiSpark.Api.Tests.csproj`: `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.Sqlite`
- [ ] T007 Verify `dotnet build ApiSpark.sln` succeeds with zero errors

**Checkpoint**: `dotnet build` green. Empty projects compile.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that ALL user stories depend on. No user story work starts until this phase is complete.

**⚠️ CRITICAL**: Admin route group, auth policies, DbContext, and logging middleware must be in place before any feature implementation.

### Configuration

- [ ] T008 Create `src/ApiSpark.Api/appsettings.json` with: `ConnectionStrings:DefaultConnection` → `Data Source=/home/data/apispark.db`, `Database:ApplyMigrationsOnStartup` → `true`, `Database:SeedOnStartup` → `false`, `AllowedOrigins: []`
- [ ] T009 Create `src/ApiSpark.Api/appsettings.Development.json` with: `ConnectionStrings:DefaultConnection` → `Data Source=./data/apispark.local.db`, `Database:ApplyMigrationsOnStartup` → `true`, `Database:SeedOnStartup` → `true`, `AllowedOrigins: ["http://localhost:5173","http://localhost:3000"]`

### Authorization Policies

- [ ] T010 Create `src/ApiSpark.Api/Infrastructure/Auth/AuthorizationSetup.cs` — registers three policies: `AdminOnly` (authenticated + Admin role), `Publisher` (authenticated + Admin or Publisher role), `ServiceOrAdmin` (Admin role OR `scope` claim `apispark.publish`); call `builder.Services.AddAuthentication().AddAuthorization()` in `Program.cs`

### CORS

- [ ] T011 Create `src/ApiSpark.Api/Infrastructure/Cors/CorsSetup.cs` — reads `AllowedOrigins` string array from `IConfiguration`; registers named policy `"ApiSparkPolicy"` with those origins, no wildcard; registers `app.UseCors("ApiSparkPolicy")` in `Program.cs`

### Structured Logging Middleware

- [ ] T012 Create `src/ApiSpark.Api/Infrastructure/Observability/RequestLoggingMiddleware.cs` — captures per-request: path, method, status code, duration (ms), correlation ID (from `X-Correlation-ID` header or `HttpContext.TraceIdentifier`), user ID (from `ClaimTypes.NameIdentifier` when authenticated), feature name (first route segment after `/api/`), operation name (full route pattern), success/failure (`statusCode < 400`); logs as structured ILogger Information entry; register in `Program.cs`

### Data Layer

- [ ] T013 Create `src/ApiSpark.Api/Infrastructure/Data/ApiSparkDbContext.cs` — `DbContext` with `DbSet<Article>` and `DbSet<Tag>`; configure unique index on `Article.Slug` and `Tag.Name` in `OnModelCreating`; read connection string from `IConfiguration["ConnectionStrings:DefaultConnection"]`; register as scoped service in `Program.cs`
- [ ] T014 Create EF Core entities in `src/ApiSpark.Api/Infrastructure/Data/`:
  - `Article.cs` — fields: `Id`, `Slug`, `Title`, `Summary`, `Body`, `PublishDate` (nullable `DateTimeOffset`), `Status` (`ArticleStatus` enum), `CreatedAt`, `UpdatedAt`, `Tags` (ICollection<Tag>)
  - `Tag.cs` — fields: `Id`, `Name`, `Articles` (ICollection<Article>)
  - `ArticleStatus.cs` — enum: `Draft = 0`, `Published = 1`
- [ ] T015 Run `dotnet ef migrations add InitialCreate --project src/ApiSpark.Api --output-dir Migrations` to generate the first EF Core migration; verify migration file is created
- [ ] T016 Create `src/ApiSpark.Api/Infrastructure/Data/DatabaseSetup.cs` — static helper called from `Program.cs` after `app.Build()`: creates scope, resolves `ApiSparkDbContext`, calls `db.Database.Migrate()` when `Database:ApplyMigrationsOnStartup` is `true`, then calls `SeedData.LoadAsync(db, ct)` when Articles table is empty and `Database:SeedOnStartup` is `true`

### Route Group Skeleton

- [ ] T017 Create `src/ApiSpark.Api/Program.cs` (full version) — wire all services (auth, CORS, DbContext, Swagger dev-only, logging middleware); define route groups: `publicApi = app.MapGroup("/api/public")`, `adminApi = app.MapGroup("/api/admin").RequireAuthorization("AdminOnly")`, `publishApi = app.MapGroup("/api/publish").RequireAuthorization("Publisher")`, `integrationsApi = app.MapGroup("/api/integrations").RequireAuthorization("ServiceOrAdmin")`; call `DatabaseSetup.InitializeAsync(app)` before `app.Run()`

**Checkpoint**: `dotnet build` green. Route groups registered. `dotnet run` starts and logs startup messages.

---

## Phase 3: User Story 1 — Platform Health Verification (Priority: P1) 🎯 MVP

**Goal**: Any caller can confirm ApiSpark is running via anonymous `GET /api/health`.

**Independent Test**: `GET /api/health` returns `200 OK` with `{"status":"Healthy","service":"ApiSpark","version":"..."}` without authentication.

### Tests for User Story 1

> Write tests FIRST — they must FAIL before implementation (T019).

- [ ] T018 [P] [US1] Create `tests/ApiSpark.Api.Tests/Features/Health/HealthEndpointTests.cs` — using `WebApplicationFactory<Program>`: test `GET /api/health` returns `200`; response body deserializes to `HealthResponse` with `status = "Healthy"`, `service = "ApiSpark"`, non-empty `version`; test anonymous access (no auth header required); test repeated calls return consistent `200`

### Implementation for User Story 1

- [ ] T019 [US1] Create `src/ApiSpark.Api/Features/Health/HealthEndpoints.cs` — static class with `MapHealthApi(this WebApplication app)` extension: registers `GET /api/health` → returns `HealthResponse` with `Status = "Healthy"`, `Service = "ApiSpark"`, `Version` from assembly version; `.WithName("GetHealth").WithOpenApi().AllowAnonymous()`; add `HealthResponse` record to `src/ApiSpark.Api/Features/Health/HealthModels.cs`; call `app.MapHealthApi()` from `Program.cs`

**Checkpoint**: `dotnet test --filter "Category=US1"` green. `GET /api/health` returns `200` with correct body.

---

## Phase 4: User Story 2 — Public Content Browsing (Priority: P2)

**Goal**: Anonymous callers retrieve published articles and tags via EF Core + SQLite.

**Independent Test**: Seed DB with 2 published articles + 1 draft + 2 tags. `GET /api/public/content/articles` returns 2 items (no draft). `GET /api/public/content/articles/hello-world` returns full detail. `GET /api/public/content/articles/draft-article` returns `404`. `GET /api/public/content/tags` returns 2 tags.

### Tests for User Story 2

- [ ] T020 [P] [US2] Create `tests/ApiSpark.Api.Tests/Infrastructure/Data/ContentRepositoryTests.cs` — use temporary SQLite database (`Data Source=:memory:` or temp file): test `GetPublishedArticlesAsync` returns only Published articles; test `GetPublishedArticleBySlugAsync` returns null for Draft slug; test `GetAllTagsAsync` returns all tags
- [ ] T021 [P] [US2] Create `tests/ApiSpark.Api.Tests/Features/PublicContent/PublicContentEndpointTests.cs` — using `WebApplicationFactory<Program>` with seeded in-memory/temp SQLite: test `GET /api/public/content/articles` returns `200` with list containing only published items; test each article summary has slug, title, summary, publishDate, tags (no body); test `GET /api/public/content/articles/hello-world` returns `200` with full body; test `GET /api/public/content/articles/draft-article` returns `404`; test `GET /api/public/content/articles/nonexistent` returns `404`; test `GET /api/public/content/tags` returns `200` with all tags; test empty DB returns `200` with empty list

### Implementation for User Story 2

- [ ] T022 [P] [US2] Create `src/ApiSpark.Api/Features/PublicContent/ContentModels.cs` — response records: `ArticleSummary(string Slug, string Title, string Summary, DateTimeOffset? PublishDate, IReadOnlyList<string> Tags)`, `ArticleDetail(string Slug, string Title, string Summary, string Body, DateTimeOffset? PublishDate, IReadOnlyList<string> Tags)`, `TagResponse(string Name)`
- [ ] T023 [P] [US2] Create `src/ApiSpark.Api/Infrastructure/Data/Repositories/IContentRepository.cs` — interface: `GetPublishedArticlesAsync`, `GetPublishedArticleBySlugAsync`, `GetAllTagsAsync` (matching data-model.md signatures)
- [ ] T024 [US2] Create `src/ApiSpark.Api/Infrastructure/Data/Repositories/ContentRepository.cs` — EF Core implementation: filter `Status == ArticleStatus.Published` at query level; slug lookup returns null for Draft; eager-load Tags; map to response records (no AutoMapper — manual projection in LINQ)
- [ ] T025 [US2] Register `IContentRepository` → `ContentRepository` as scoped service in `Program.cs`
- [ ] T026 [US2] Create `src/ApiSpark.Api/Features/PublicContent/ContentService.cs` — thin service delegating to `IContentRepository`; inject via constructor; no business logic at this phase beyond delegation
- [ ] T027 [US2] Create `src/ApiSpark.Api/Features/PublicContent/PublicContentEndpoints.cs` — extension `MapPublicContentApi(this RouteGroupBuilder group)`: `GET /content/articles` → `GetPublishedArticles`; `GET /content/articles/{slug}` → `GetArticleBySlug` (returns `404` typed result when null); `GET /content/tags` → `GetAllTags`; all `.WithOpenApi().AllowAnonymous()`; call `publicApi.MapPublicContentApi()` from `Program.cs`
- [ ] T028 [US2] Create `src/ApiSpark.Api/Infrastructure/Data/Seed/SeedData.cs` — `LoadAsync(ApiSparkDbContext db, CancellationToken ct)`: seeds 2 published articles (`hello-world`, `getting-started-with-apispark`) + 1 draft article (`draft-article`) + tags (`general`, `intro`, `apispark`, `tutorial`); guard: only runs when `db.Articles.AnyAsync()` is false

**Checkpoint**: `dotnet test --filter "Category=US2"` green. Seeded articles retrievable; draft excluded; 404 for missing/draft slugs.

---

## Phase 5: User Story 3 — Admin Route Protection (Priority: P3)

**Goal**: Admin route group enforces the `AdminOnly` policy — unauthenticated callers receive `401`.

**Independent Test**: `GET /api/admin/health/deep` without auth → `401`. With valid Admin credentials → not `401`/`403`.

### Tests for User Story 3

- [ ] T029 [P] [US3] Create `tests/ApiSpark.Api.Tests/Infrastructure/Auth/AuthorizationBoundaryTests.cs` — using `WebApplicationFactory<Program>`: test `GET /api/admin/health/deep` without auth header returns `401`; test `GET /api/admin/health/deep` with valid Admin JWT/claim returns `200`; test caller with authenticated identity but without Admin role returns `403`; test `GET /api/public/content/articles` without auth returns `200` (public access unaffected)

### Implementation for User Story 3

- [ ] T030 [US3] Create `src/ApiSpark.Api/Features/Health/AdminHealthEndpoints.cs` — `MapAdminHealthApi(this RouteGroupBuilder group)`: registers `GET /health/deep` → returns deep health response (checks DB connectivity via `db.Database.CanConnectAsync()`); returns `200` with `{"status":"Healthy","checks":{"database":"ok"}}` or `503` on failure; endpoint is inside `adminApi` group which already has `RequireAuthorization("AdminOnly")`; call `adminApi.MapAdminHealthApi()` from `Program.cs`
- [ ] T031 [US3] Configure test authentication in `tests/ApiSpark.Api.Tests/` — create a `TestAuthHandler` (inherits `AuthenticationHandler<AuthenticationSchemeOptions>`) that allows tests to inject claims; configure `WebApplicationFactory` to register test auth scheme; enable Admin-role and non-Admin-role test helpers

**Checkpoint**: `dotnet test --filter "Category=US3"` green. Unauthenticated → `401`; Admin → `200`; non-Admin → `403`.

---

## Phase 6: User Story 4 — API Documentation Discovery (Priority: P4)

**Goal**: Swagger UI available in Development, absent in Production.

**Independent Test**: Run in Development → `GET /swagger` returns `200` HTML page; run in Production → `GET /swagger` returns `404`.

### Tests for User Story 4

- [ ] T032 [P] [US4] Create `tests/ApiSpark.Api.Tests/Features/SwaggerAvailabilityTests.cs` — test with `ASPNETCORE_ENVIRONMENT=Development`: swagger endpoint returns `200`; test that all three contracts (health, articles list, article detail, tags) appear in OpenAPI JSON at `/swagger/v1/swagger.json`; test with `ASPNETCORE_ENVIRONMENT=Production`: swagger endpoint returns `404`

### Implementation for User Story 4

- [ ] T033 [US4] In `Program.cs`, confirm Swagger/OpenAPI setup is already gated: `if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }` — verify the conditional was added in T017; add `builder.Services.AddEndpointsApiExplorer()` and `builder.Services.AddSwaggerGen()` (already in T017 scope); ensure health and public content endpoints use `.WithOpenApi()` (already specified in T019 and T027)

**Checkpoint**: `dotnet test --filter "Category=US4"` green. Swagger visible in dev; absent in prod.

---

## Phase 7: User Story 5 — Automated Build and Test Validation (Priority: P5)

**Goal**: GitHub Actions runs `dotnet build` + `dotnet test` on every PR to `main`; deploys on merge to `main`.

**Independent Test**: Open a PR and observe the Actions workflow completes with pass/fail status reported on the PR.

### Implementation for User Story 5 (no additional tests — CI itself is the test)

- [ ] T034 [P] [US5] Create `.github/workflows/build-test.yml` — triggers on `pull_request` to `main` and `push` to `main`; steps: `actions/checkout@v4`, `actions/setup-dotnet@v4` (version `10.0.x`), `dotnet restore`, `dotnet build --configuration Release --no-restore`, `dotnet test --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"`, upload test results artifact
- [ ] T035 [P] [US5] Create `.github/workflows/deploy.yml` — triggers on `push` to `main`; steps: checkout, setup .NET 10, restore, build, test, `dotnet publish src/ApiSpark.Api/ApiSpark.Api.csproj --configuration Release --output ./publish`, `azure/webapps-deploy@v3` with `app-name: apispark` and `publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}`; deploy step must NOT include the `./data/` directory

**Checkpoint**: PR to `main` triggers `build-test.yml` and reports status. Merge triggers `deploy.yml`.

---

## Phase 8: User Story 6 — Local Developer Setup (Priority: P6)

**Goal**: Clone → `dotnet run` → seeded DB + health endpoint in under 10 minutes.

**Independent Test**: Fresh clone, `dotnet run`, `GET /api/public/content/articles` returns seeded articles within 10 minutes.

### Tests for User Story 6

- [ ] T036 [P] [US6] Create `tests/ApiSpark.Api.Tests/Features/LocalSetupTests.cs` — using `WebApplicationFactory<Program>` with Development environment: test that on first start (empty SQLite) seed data is applied; test `GET /api/public/content/articles` returns at least 2 published articles; test `GET /api/health` responds; test `GET /swagger` returns `200`; test repeated startup does not re-seed (idempotency guard)

### Implementation for User Story 6

- [ ] T037 [P] [US6] Create `docs/decisions/0001-single-backend-api.md` — ADR content from Jumpstart §21
- [ ] T038 [P] [US6] Create `docs/decisions/0002-sqlite-default.md` — ADR content from Jumpstart §21
- [ ] T039 [P] [US6] Create `docs/decisions/0003-static-web-app-clients.md` — ADR content from Jumpstart §21
- [ ] T040 [P] [US6] Create `docs/decisions/0004-dotnet-10-lts.md` — ADR content from Jumpstart §21
- [ ] T041 [P] [US6] Create `docs/decisions/0005-authentication-boundaries.md` — ADR content from Jumpstart §21
- [ ] T042 [US6] Update `README.md` with content from Jumpstart §20: ApiSpark purpose, goals, target runtime, route areas, quick-start command; add link to `quickstart.md`

**Checkpoint**: `dotnet test --filter "Category=US6"` green. Fresh `dotnet run` from clone produces seeded data.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Final validation, edge case hardening, and documentation completeness checks.

- [ ] T043 [P] Verify `.gitignore` excludes: `data/*.db`, `*.db`, `*.db-shm`, `*.db-wal`, `publish/`, `*.user`, `bin/`, `obj/`; confirm no `.db` file exists in git history using `git log --all --full-history -- "*.db"`
- [ ] T044 [P] Review all `appsettings*.json` files: confirm no connection strings with passwords, no API keys, no tokens; confirm `DefaultConnection` uses file path only (no credentials)
- [ ] T045 Validate `quickstart.md` steps match the actual running application — walk through each step: clone → build → run → curl health → curl articles → curl swagger; note any discrepancies and update `quickstart.md`
- [ ] T046 Add `data/` directory with `.gitkeep` and ensure `data/*.db` is in `.gitignore`; create `data/seed/` placeholder for future JSON seed files
- [ ] T047 [P] Run `dotnet test` (full suite) and confirm all tests pass with zero failures
- [ ] T048 [P] Run `dotnet build --configuration Release` and confirm zero warnings (treat warnings as errors if possible via `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `.csproj`)

---

## Dependencies & Execution Order

### Phase Dependencies

```text
Phase 1 (Setup)
  └─→ Phase 2 (Foundational) ← BLOCKS all story phases
        ├─→ Phase 3 (US1 Health) ← MVP complete here
        ├─→ Phase 4 (US2 Public Content)
        ├─→ Phase 5 (US3 Admin Boundary)
        ├─→ Phase 6 (US4 Swagger)
        ├─→ Phase 7 (US5 CI/CD)
        └─→ Phase 8 (US6 Local Setup)
              └─→ Phase 9 (Polish)
```

### User Story Dependencies

| Story | Depends On | Notes |
|-------|-----------|-------|
| US1 Health (P1) | Phase 2 complete | Independent — no data layer needed |
| US2 Content (P2) | Phase 2 + T013–T016 (DbContext, entities, migration) | Content repository needs DB |
| US3 Admin (P3) | Phase 2 + T010 (auth policies) + any admin route | Auth boundary test needs a registered admin endpoint |
| US4 Swagger (P4) | T019, T027 (endpoints using .WithOpenApi()) | Swagger lists existing endpoints |
| US5 CI/CD (P5) | Phase 1 (buildable solution) | Workflows don't depend on feature completion |
| US6 Local Setup (P6) | All above complete | Validates the full stack end-to-end |

### Within Each User Story

- Tests created FIRST (must FAIL before implementation)
- Models before services (T022 before T023–T024)
- Repository/service before endpoints (T024–T025 before T027)
- Seed data before integration tests (T028 before T021 full run)

### Parallel Opportunities

```text
Phase 1:  T003 ‖ T004 (project creation can overlap)
Phase 2:  T008 ‖ T009 (config files) → then T010 ‖ T011 ‖ T012 ‖ T013
          T013 → T014 → T015 (sequential: DbContext → entities → migration)
Phase 3:  T018 (test) created, then T019 (impl)
Phase 4:  T020 ‖ T021 ‖ T022 ‖ T023 (tests + models in parallel, then T024 → T025 → T026 → T027 → T028)
Phase 5:  T029 (test) then T030 ‖ T031 in parallel
Phase 6:  T032 (test) then T033 (one task)
Phase 7:  T034 ‖ T035 (two workflow files, fully independent)
Phase 8:  T036 (test) then T037 ‖ T038 ‖ T039 ‖ T040 ‖ T041 (ADRs all independent) → T042
Phase 9:  T043 ‖ T044 ‖ T047 ‖ T048 (all independent)
```

---

## Parallel Execution Examples

### Phase 2 Parallel Batch

```text
Batch A (start immediately):
  T008 — Write appsettings.json
  T009 — Write appsettings.Development.json

Batch B (after T002 solution exists):
  T010 — AuthorizationSetup.cs
  T011 — CorsSetup.cs
  T012 — RequestLoggingMiddleware.cs
  T013 — ApiSparkDbContext.cs

Sequential after Batch B:
  T014 — Entities (Article, Tag, ArticleStatus)
  T015 — EF migration
  T016 — DatabaseSetup.cs
  T017 — Program.cs (wires everything)
```

### Phase 4 Parallel Batch (US2)

```text
Batch A (parallel):
  T020 — ContentRepositoryTests.cs
  T021 — PublicContentEndpointTests.cs
  T022 — ContentModels.cs
  T023 — IContentRepository.cs

Sequential after Batch A:
  T024 — ContentRepository.cs
  T025 — Register DI
  T026 — ContentService.cs
  T027 — PublicContentEndpoints.cs
  T028 — SeedData.cs
```

---

## Gate Acknowledgements

No unresolved gate findings at time of task generation. Gates (`analyze`, `critic`) will be run after task generation and findings recorded here if any blocking concerns arise.

---

## Implementation Strategy

### MVP First (User Story 1 — Health Check)

1. Complete Phase 1: Setup (T001–T007)
2. Complete Phase 2: Foundational (T008–T017)
3. Complete Phase 3: US1 Health (T018–T019)
4. **STOP AND VALIDATE**: `dotnet test` passes; `curl /api/health` returns `200`
5. This is the minimum deployable slice — proves the platform stands up

### Incremental Delivery Order

1. Phases 1–3 → MVP (health endpoint proves platform boots)
2. Phase 4 (US2) → Public content proves the data layer end-to-end
3. Phase 5 (US3) → Auth boundary proves the security model
4. Phase 7 (US5) → CI/CD proves the delivery pipeline (can overlap with Phase 5/6)
5. Phase 6 (US4) + Phase 8 (US6) → Polish developer experience
6. Phase 9 → Final hardening

### Total Task Count

| Phase | Tasks | Notes |
|-------|-------|-------|
| Phase 1: Setup | 7 (T001–T007) | Sequential scaffold |
| Phase 2: Foundational | 10 (T008–T017) | Critical path |
| Phase 3: US1 | 2 (T018–T019) | MVP increment |
| Phase 4: US2 | 9 (T020–T028) | Core content feature |
| Phase 5: US3 | 3 (T029–T031) | Auth boundary |
| Phase 6: US4 | 2 (T032–T033) | Swagger gate |
| Phase 7: US5 | 2 (T034–T035) | CI/CD workflows |
| Phase 8: US6 | 7 (T036–T042) | Local setup + ADRs |
| Phase 9: Polish | 6 (T043–T048) | Hardening |
| **Total** | **48 tasks** | |

Parallel opportunities: 22 tasks marked [P].

---

## Notes

- [P] tasks = work on different files with no unmet dependencies in the same phase — safe to parallelize
- [Story] label maps each task to a user story for traceability and independent testing
- Each user story phase ends with a checkpoint — validate independently before moving on
- Seed data (T028) is required before US2 integration tests run against a real (temp) SQLite DB
- T031 (test auth handler) is a prerequisite for US3 tests — must be complete before T029 test can pass
- `.gitignore` must exclude `*.db` before any `dotnet run` that creates `data/apispark.local.db`
