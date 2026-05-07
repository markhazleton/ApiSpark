# Pull Request Review: feat: ApiSpark platform foundation — .NET 10 backend with EF Core + SQLite, public content API, auth boundaries, and CI/CD

## Review Metadata

- **PR Number**: #1
- **Source Branch**: `001-apispark-foundation`
- **Target Branch**: `main`
- **Review Date**: 2026-05-07 17:00:00 UTC
- **Last Updated**: 2026-05-07 17:00:00 UTC
- **Reviewed Commit**: `79a5f2e143b827035d5bbd71182f74463f85eb3e`
- **Reviewer**: devspark.pr-review
- **Constitution Version**: 1.1.0 (2026-05-06)

## Revision Log

| Rev | Commit | Date | Critical | High | Medium | Low | CON | Test Command | Result |
|-----|--------|------|----------|------|--------|-----|-----|--------------|--------|
| 1 | `79a5f2e` | 2026-05-07 | 0 | 2 | 2 | 2 | 0 | `dotnet test tests/ApiSpark.Api.Tests` | ✅ 27/27 pass |

## PR Summary

- **Author**: @markhazleton
- **Created**: 2026-05-07
- **Status**: OPEN
- **Files Changed**: 60
- **Commits**: 4
- **Lines**: +3,976 / −2

## Stats

| Metric | Value |
|--------|-------|
| Files changed | 60 |
| Lines added | +3,976 |
| Lines removed | −2 |
| Net lines | +3,974 |
| Commit snapshot | `79a5f2e` |

## Executive Summary

- ✅ **Constitution Compliance**: PASS — all 10 principles checked; Principles I–X satisfied
- 📋 **Spec Lifecycle**: Complete — spec status = `Complete`
- 📝 **Task Completion**: 50/50 tasks marked complete (⚠️ one task marked done without the corresponding file — see H-02)
- 🔒 **Security**: 1 issue (CORS fallback risk — H-01)
- 📊 **Code Quality**: 2 recommendations (M-01 dead test stub, M-02 hardcoded URL)
- 🧪 **Testing**: ✅ 27/27 pass — `dotnet test tests/ApiSpark.Api.Tests` (Release build)
- 📝 **Documentation**: PASS — README, 5 ADRs, quickstart.md all updated
- 🏛️ **Constitution Improvements**: 0 CON findings

**Overall Assessment**: A well-structured foundation PR that correctly establishes the modular ASP.NET Core platform. All 10 constitution principles are satisfied, the spec lifecycle is complete, and 27 tests pass clean. Two HIGH findings need resolution before merge: (1) the CORS fallback silently allows `localhost` origins in production when the `AllowedOrigins` config is unset, and (2) the structured logging test file (T012a) was marked complete in `tasks.md` but was never created — FR-009 has no direct field-by-field test coverage.

**Approval Recommendation**: ⚠️ REQUEST CHANGES
*Resolve H-01 (CORS fallback) and H-02 (missing logging test) before merging.*

---

## Action Items

### Immediate Actions (Blocking — must resolve before merge)

- [ ] **H-01** `src/ApiSpark.Api/Infrastructure/Cors/CorsSetup.cs:15–19` — CORS falls back to localhost origins when `AllowedOrigins` is empty, silently blocking production clients
  - **Broken code**:
    ```csharp
    if (origins.Length > 0)
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    else
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader().AllowAnyMethod();
    ```
  - **Fix**: When `AllowedOrigins` is empty, apply no allowed origins (deny all cross-origin) rather than falling back to localhost. The localhost fallback is safe for development but would break production clients if Azure App Service settings are not configured. Use environment-aware fallback:
    ```csharp
    if (origins.Length > 0)
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    else if (environment.IsDevelopment())
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader().AllowAnyMethod();
    // else: no origins allowed (implicit deny for cross-origin)
    ```
    Pass `IWebHostEnvironment` into `AddApiSparkCors()`.

- [ ] **H-02** `tests/ApiSpark.Api.Tests/Infrastructure/Observability/` — T012a marked complete but `RequestLoggingMiddlewareTests.cs` was never created; FR-009 (9 structured logging fields) has no field-by-field assertion test
  - **Issue**: `tasks.md` marks T012a as `[X]` complete, but no test file exists at `tests/ApiSpark.Api.Tests/Infrastructure/Observability/RequestLoggingMiddlewareTests.cs`. The 9 required FR-009 fields (path, method, status, duration, correlationId, userId, featureName, operationName, success) are captured in `RequestLoggingMiddleware.cs` but are never verified by a test. Constitution Principle II (Test-First, NON-NEGOTIABLE) requires tests to verify meaningful behavior.
  - **Fix**: Create `tests/ApiSpark.Api.Tests/Infrastructure/Observability/RequestLoggingMiddlewareTests.cs` that uses a captured `ILogger` sink to assert all 9 fields are present in the logged output for a standard request. Minimal implementation using `Microsoft.Extensions.Logging.Testing` or a custom `FakeLogger`:
    ```csharp
    [Fact]
    public async Task RequestLogging_CapturesAllNineRequiredFields()
    {
        var client = _factory.CreateClient();
        await client.GetAsync("/api/health");
        // Assert log entries contain: Method, RequestPath, StatusCode,
        // DurationMs, CorrelationId, FeatureName, OperationName, Success
        // UserId should be "anonymous" for unauthenticated requests
    }
    ```

### Recommended Improvements

- [ ] **M-01** `tests/ApiSpark.Api.Tests/UnitTest1.cs:1–10` — Auto-generated xUnit scaffolding test with empty body committed; dead code
  - The file contains `public void Test1() { }` which always passes trivially, adds noise to the test report, and misleads reviewers about test quality. Remove or replace with a meaningful test.

- [ ] **M-02** `.github/workflows/deploy.yml:44` — Post-deploy smoke test URL is hardcoded
  - `curl --fail --max-time 30 https://api.markhazleton.com/api/health` assumes a specific domain. If the app is deployed to a staging or preview environment, this step will always test the wrong endpoint.
  - Fix: use a GitHub Actions variable or repository secret: `${{ vars.DEPLOYED_HEALTH_URL }}` or derive from the Azure App Service name.

### Low Priority Improvements

- [ ] **L-01** `dotnet-tools.json:1` — Tool manifest at repository root instead of standard `.config/dotnet-tools.json` location
  - The dotnet SDK convention is `.config/dotnet-tools.json`. While the root location is functional (found by `dotnet tool restore`), it diverges from the standard and may confuse tooling. Consider moving to `.config/dotnet-tools.json` to match `.NET` conventions.

- [ ] **L-02** `ApiSpark.slnx` — New XML solution format committed alongside the implied `ApiSpark.sln`
  - Both solution formats exist. The `.slnx` format is new (VS 2022 17.9+) and not all tooling supports it yet. Consider `.gitignore`-ing `ApiSpark.slnx` or removing `ApiSpark.sln` to avoid ambiguity. If both are kept, document which is the canonical solution file.

### Constitution Improvements

None found.

---

## What's Good

- **Critic/analyze gate discipline**: The PR went through full analyze + critic gate review, identified 2 showstoppers and 7 risks pre-implementation, and resolved all of them with documented rationale. This is exactly what the spec-driven workflow is supposed to produce.
- **Showstopper prevention**: The `public partial class Program { }` fix (SS-2) and JwtBearer scheme registration (SS-1) are documented directly in the task descriptions, making the reasoning accessible to future maintainers.
- **SQLite WAL mode**: Connection strings include `Journal Mode=WAL;Cache=Shared;` on both production and development paths — avoids the classic SQLite locked-database failure under concurrent startup reads/writes.
- **Test isolation pattern**: `ApiSparkWebApplicationFactory` correctly uses a named shared-cache in-memory SQLite with a long-lived `SqliteConnection`, preventing the per-connection blank-database problem that defeats integration tests. The `ConfigureAppConfiguration` override prevents the path-writability check from firing on developer machines.
- **Slug validation at endpoint layer**: `PublicContentEndpoints.cs` validates the slug against a compiled regex before reaching the service — prevents log injection via crafted path parameters (critic HP-2 resolved correctly).

---

## Findings Detail

### Critical Issues

None found.

### High Priority Issues

| ID | Status | Principle | File:Line | Issue | Fix |
|----|--------|-----------|-----------|-------|-----|
| H-01 | 🔴 Open | Security by Default (IV) | `src/ApiSpark.Api/Infrastructure/Cors/CorsSetup.cs:15` | When `AllowedOrigins` is empty (default in `appsettings.json`), CORS silently falls back to localhost origins. A production deployment without Azure App Service `AllowedOrigins__N` settings set will block production clients (markhazleton.com etc.) via CORS, or leak the localhost fallback if those origins somehow match a future test client. | Wrap fallback in `if (environment.IsDevelopment())` guard; deny cross-origin when empty in non-dev environments |
| H-02 | 🔴 Open | Test-First (II) | `tests/ApiSpark.Api.Tests/Infrastructure/Observability/` (missing file) | T012a marked `[X]` complete in tasks.md but `RequestLoggingMiddlewareTests.cs` does not exist. FR-009 requires 9 specific structured fields be captured; no test verifies the fields are actually present in log output. The middleware runs on every test request but field presence is unverified. | Create `RequestLoggingMiddlewareTests.cs` with a captured ILogger sink asserting all 9 FR-009 fields |

### Medium Priority Suggestions

| ID | Status | Principle | File:Line | Issue | Recommendation |
|----|--------|-----------|-----------|-------|----------------|
| M-01 | 🔴 Open | Simplicity (III) | `tests/ApiSpark.Api.Tests/UnitTest1.cs:6` | Empty auto-generated test `Test1()` committed; trivially passes and adds noise | Delete file or replace with a genuinely useful smoke test |
| M-02 | 🔴 Open | Simplicity (III) | `.github/workflows/deploy.yml:44` | Post-deploy smoke test hardcodes `https://api.markhazleton.com/api/health`; breaks for staging environments | Extract to `${{ vars.DEPLOYED_HEALTH_URL }}` GitHub Actions variable |

### Low Priority Improvements

| ID | Status | Principle | File:Line | Issue | Recommendation |
|----|--------|-----------|-----------|-------|----------------|
| L-01 | 🔴 Open | Simplicity (III) | `dotnet-tools.json:1` | Tool manifest at root instead of `.config/dotnet-tools.json` | Move to `.config/dotnet-tools.json` to follow dotnet convention |
| L-02 | 🔴 Open | Simplicity (III) | `ApiSpark.slnx` | Both `.sln` and `.slnx` solution formats may coexist | Keep one canonical format; `.gitignore` the other or document which is primary |

### Constitution Improvements

None found.

---

## Constitution Alignment Details

| Principle | Status | Evidence | Notes |
|-----------|--------|----------|-------|
| I. API-First | ✅ Pass | `contracts/health.yaml`, `contracts/public-content.yaml` precede implementation | OpenAPI contracts exist; Swagger in dev only; endpoints use `.WithName()` for discoverability |
| II. Test-First | ⚠️ Partial | 27 tests pass; H-02 — T012a missing | All features tested; FR-009 field assertions missing (see H-02) |
| III. Simplicity | ✅ Pass | Two-project layout; no premature abstraction | Feature folder pattern; repository interface is warranted; no over-engineering |
| IV. Security by Default | ⚠️ Partial | H-01 — CORS localhost fallback; otherwise all security checks in place | Input validation, auth boundaries, no secrets — only CORS fallback concern |
| V. Spec-Driven | ✅ Pass | spec.md Complete; tasks.md 50/50; all gates resolved | Full workflow: specify → plan → tasks → implement |
| VI. Ownership Boundary | ✅ Pass | `.devspark/` untouched; all artifacts in `.documentation/` | Framework files not modified |
| VII. Single Backend Platform | ✅ Pass | `src/ApiSpark.Api/` — one project | No microservices; route groups correctly separate features |
| VIII. Clear Auth Boundaries | ✅ Pass | Route groups: `/api/public` anonymous, `/api/admin` AdminOnly, `/api/publish` Publisher, `/api/integrations` ServiceOrAdmin | Exact match with constitution authorization table |
| IX. Relational-First Data | ✅ Pass | EF Core + SQLite only; no Cosmos | WAL mode configured; prod path `/home/data/apispark.db` |
| X. Zero Secrets in Source | ✅ Pass | `.gitignore` excludes `*.db`; no credentials in config files | Git log scanned; no `.db`, `.env`, `.pfx` in history |

---

## Security Checklist

- [x] No hardcoded secrets or credentials — connection strings contain file paths only; no tokens, keys, or passwords
- [ ] Input validation present where needed — **H-01 partial**: slug validated ✅; CORS fallback gap ⚠️
- [x] Authentication/authorization checks appropriate — JwtBearer scheme registered; policies enforced on all admin/publish/integration routes; `public partial class Program` enables test auth injection
- [x] No SQL injection vulnerabilities — EF Core parameterized queries throughout; `ContentRepository` uses LINQ projection
- [x] No XSS vulnerabilities — API-only (no HTML output); slug sanitized before logging
- [x] Dependencies reviewed — EF Core 10, JwtBearer 10, Swashbuckle 10.1.7, Microsoft.AspNetCore.OpenApi 10; no known vulnerabilities; dotnet-ef 10.0.7 pinned in tools manifest

---

## Testing Coverage

**Status**: ADEQUATE (with gap in FR-009 field assertions — see H-02)

27 tests passing across 6 test files:
- `HealthEndpointTests.cs` — 4 tests (status, body, anonymous access, 500ms timing)
- `PublicContentEndpointTests.cs` — 7 tests (list, list no-body, slug detail, draft 404, nonexistent 404, invalid slug 400, tags)
- `ContentRepositoryTests.cs` — 6 tests (published only, exclusions, detail, draft null, nonexistent null, tags)
- `AuthorizationBoundaryTests.cs` — 4 tests (401, admin 200, non-admin 403, public unaffected)
- `SwaggerAvailabilityTests.cs` — 2 tests (dev 200, prod 404)
- `LocalSetupTests.cs` — 3 tests (health, seed articles, swagger)

**Gap**: No test file verifies the 9 structured log fields from `RequestLoggingMiddleware` (T012a).

---

## Test Inventory

| File | Tests | Notes |
|------|-------|-------|
| `tests/ApiSpark.Api.Tests/Features/Health/HealthEndpointTests.cs` | 4 | All new |
| `tests/ApiSpark.Api.Tests/Features/PublicContent/PublicContentEndpointTests.cs` | 7 | All new |
| `tests/ApiSpark.Api.Tests/Features/SwaggerAvailabilityTests.cs` | 2 | All new |
| `tests/ApiSpark.Api.Tests/Features/LocalSetupTests.cs` | 3 | All new |
| `tests/ApiSpark.Api.Tests/Infrastructure/Auth/AuthorizationBoundaryTests.cs` | 4 | All new |
| `tests/ApiSpark.Api.Tests/Infrastructure/Data/ContentRepositoryTests.cs` | 6 | All new |
| `tests/ApiSpark.Api.Tests/UnitTest1.cs` | 1 | ⚠️ Empty stub — M-01 |
| **Total** | **27** | 27 meaningful + 1 stub |

Missing: `tests/ApiSpark.Api.Tests/Infrastructure/Observability/RequestLoggingMiddlewareTests.cs` — H-02

---

## Documentation Status

**Status**: ADEQUATE

- `README.md` — updated with project goals, route areas, quick start, and architecture links ✅
- `docs/decisions/000[1-5]-*.md` — 5 ADRs covering single backend, SQLite default, static clients, .NET 10, auth boundaries ✅
- `.documentation/specs/001-apispark-foundation/quickstart.md` — full local setup guide validated during implementation ✅
- `CLAUDE.md` — updated by agent context script with tech stack ✅

---

## Changed Files Summary

| File | Tier | Changes | Type | Findings |
|------|------|---------|------|---------|
| `src/ApiSpark.Api/Program.cs` | P0 | +69 | Added | None |
| `src/ApiSpark.Api/Infrastructure/Auth/AuthorizationSetup.cs` | P0 | +49 | Added | None |
| `src/ApiSpark.Api/Infrastructure/Data/DatabaseSetup.cs` | P0 | +73 | Added | None |
| `src/ApiSpark.Api/Infrastructure/Data/ApiSparkDbContext.cs` | P0 | +43 | Added | None |
| `src/ApiSpark.Api/Infrastructure/Observability/RequestLoggingMiddleware.cs` | P0 | +38 | Added | H-02 (no test) |
| `src/ApiSpark.Api/Infrastructure/Cors/CorsSetup.cs` | P0 | +25 | Added | H-01 |
| `src/ApiSpark.Api/Features/PublicContent/PublicContentEndpoints.cs` | P0 | +49 | Added | None |
| `src/ApiSpark.Api/Features/Health/HealthEndpoints.cs` | P0 | +24 | Added | None |
| `src/ApiSpark.Api/Features/Health/AdminHealthEndpoints.cs` | P0 | +32 | Added | None |
| `.github/workflows/deploy.yml` | P0 | +47 | Added | M-02 |
| `.github/workflows/build-test.yml` | P0 | +31 | Added | None |
| `src/ApiSpark.Api/Infrastructure/Data/Repositories/ContentRepository.cs` | P1 | +49 | Added | None |
| `src/ApiSpark.Api/Infrastructure/Data/Seed/SeedData.cs` | P1 | +52 | Added | None |
| `src/ApiSpark.Api/Features/PublicContent/ContentService.cs` | P1 | +13 | Added | None |
| `src/ApiSpark.Api/appsettings.json` | P2 | +19 | Modified | None |
| `src/ApiSpark.Api/appsettings.Development.json` | P2 | +19 | Added | None |
| `tests/ApiSpark.Api.Tests/Infrastructure/ApiSparkWebApplicationFactory.cs` | P2 | +101 | Added | None |
| `tests/ApiSpark.Api.Tests/Features/Health/HealthEndpointTests.cs` | P2 | +53 | Added | None |
| `tests/ApiSpark.Api.Tests/Infrastructure/Auth/AuthorizationBoundaryTests.cs` | P2 | +40 | Added | None |
| `tests/ApiSpark.Api.Tests/Features/PublicContent/PublicContentEndpointTests.cs` | P2 | +81 | Added | None |
| `tests/ApiSpark.Api.Tests/Infrastructure/Data/ContentRepositoryTests.cs` | P2 | +85 | Added | None |
| `tests/ApiSpark.Api.Tests/UnitTest1.cs` | P2 | +10 | Added | M-01 |
| `src/ApiSpark.Api/Migrations/` | P2 | ~120 | Added | None |
| `dotnet-tools.json` | P3 | +12 | Added | L-01 |
| `ApiSpark.slnx` | P3 | +13 | Added | L-02 |
| `docs/decisions/000[1-5]-*.md` | P3 | +115 | Added | None |
| `README.md` | P3 | +65 | Modified | None |
| `.gitignore` | P3 | +12 | Modified | None |
| `.documentation/specs/001-apispark-foundation/` | P3 | ~3,100 | Added | None |

---

## Behavioral Changes

None detected. This is a greenfield PR — no existing behavior was modified.

---

## Approval Decision

**Recommendation**: ⚠️ REQUEST CHANGES

**Reasoning**:
- H-01 (CORS localhost fallback) is a latent production risk: if `AllowedOrigins` Azure App Service settings are not configured at deploy time, production clients will be blocked by CORS. The fix is a one-line guard clause.
- H-02 (missing T012a test file) violates Constitution Principle II (Test-First, NON-NEGOTIABLE): a task was marked complete without creating the required test. FR-009 structured logging field requirements have no direct test assertion.
- Both HIGH findings have clear, low-effort fixes (< 30 minutes each).
- All 10 constitution principles otherwise pass; the spec is Complete; 27 tests pass clean.

**Estimated Rework Time**: ~1–2 hours to implement both fixes and add the missing test file.

```yaml
findings:
  - finding_id: pr1-H-01
    severity: high
    description: "CORS setup falls back to localhost:5173/3000 when AllowedOrigins config is empty. In production without Azure App Service CORS settings configured, production client origins will be blocked by CORS while localhost is silently allowed."
    recommended_action: "Wrap localhost fallback in environment.IsDevelopment() guard. Add IWebHostEnvironment parameter to AddApiSparkCors(). When origins are empty in non-Development, apply no cross-origin policy (implicit deny)."
    execution_mode: auto
    status: open
    outcome: ""

  - finding_id: pr1-H-02
    severity: high
    description: "tasks.md marks T012a (RequestLoggingMiddlewareTests.cs) as [X] complete but the file does not exist. FR-009 requires 9 structured log fields; none are verified by a test. Constitution Principle II (Test-First, NON-NEGOTIABLE) is violated."
    recommended_action: "Create tests/ApiSpark.Api.Tests/Infrastructure/Observability/RequestLoggingMiddlewareTests.cs with a captured ILogger sink asserting Method, RequestPath, StatusCode, DurationMs, CorrelationId, FeatureName, OperationName, Success fields are present in logged output."
    execution_mode: selective
    status: open
    outcome: ""

  - finding_id: pr1-M-01
    severity: medium
    description: "UnitTest1.cs committed as auto-generated xUnit scaffolding with empty Test1() body. Dead code that trivially passes and misleads reviewers about test quality."
    recommended_action: "Delete tests/ApiSpark.Api.Tests/UnitTest1.cs."
    execution_mode: auto
    status: open
    outcome: ""

  - finding_id: pr1-M-02
    severity: medium
    description: "deploy.yml hardcodes https://api.markhazleton.com/api/health in the post-deploy smoke test. Breaks for staging or preview environment deployments."
    recommended_action: "Replace hardcoded URL with ${{ vars.DEPLOYED_HEALTH_URL }} GitHub Actions variable."
    execution_mode: auto
    status: open
    outcome: ""

  - finding_id: pr1-L-01
    severity: low
    description: "Tool manifest at repository root (dotnet-tools.json) instead of standard .config/dotnet-tools.json location."
    recommended_action: "Move to .config/dotnet-tools.json to follow dotnet SDK convention."
    execution_mode: auto
    status: open
    outcome: ""

  - finding_id: pr1-L-02
    severity: low
    description: "ApiSpark.slnx (new XML format) committed; may coexist with ApiSpark.sln causing ambiguity about canonical solution file."
    recommended_action: "Keep one canonical format. .gitignore the other or document which is primary in README."
    execution_mode: manual
    status: open
    outcome: ""
```

---

*Review generated by devspark.pr-review v1.2*
*Constitution-driven code review for ApiSpark*
*To re-review after fixes: `/devspark.pr-review #1 re-review`*
*When addressing these findings, run `/devspark.address-pr-review pr1`. The review file must be committed separately from code fixes.*
