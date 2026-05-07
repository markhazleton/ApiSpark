# Research: ApiSpark Platform Foundation

**Phase**: 0 — Outline & Research
**Branch**: `001-apispark-foundation`
**Date**: 2026-05-07
**Status**: Complete — all NEEDS CLARIFICATION resolved

---

## Research Topics

The technical context for this feature is well-defined by the Jumpstart Guide and the spec. The following topics were investigated to resolve implementation details before design.

---

### 1. EF Core + SQLite Startup Migration Pattern (.NET 10)

**Decision**: Use `db.Database.Migrate()` inside an explicit startup scope, gated by the `Database:ApplyMigrationsOnStartup` configuration flag.

**Rationale**: Controlled migration (not automatic) gives the production environment explicit control. The `Database.EnsureCreated()` API must not be used alongside migrations — it skips migration history.

**Implementation pattern** (from Jumpstart §9.2):

```csharp
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApiSparkDbContext>();
if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    db.Database.Migrate();
}
if (!await db.Articles.AnyAsync())
{
    await SeedData.LoadAsync(db);
}
```

**Alternatives considered**:
- `Database.EnsureCreated()`: Rejected — does not apply migrations, breaks migration history tracking.
- `IHostedService` migration runner: Deferred — adds complexity not warranted at foundation stage. Revisit if async startup becomes necessary.
- Auto-migration on every startup (unconditional): Rejected — removes production control, risky for schema changes once data matters.

---

### 2. Authentication Approach for Foundation Phase

**Decision**: Register authorization policies (AdminOnly, Publisher, ServiceOrAdmin) at startup. Do not configure an external identity provider in this phase. Admin routes enforce `RequireAuthorization("AdminOnly")` which returns `401 Unauthorized` for unauthenticated callers by default when no authentication middleware is configured.

**Rationale**: The spec (FR-024) requires policies to be registered even if no admin endpoints are implemented in this phase. User Story 3 requires that unauthenticated callers receive `401`. The simplest correct implementation is policy registration with `AddAuthentication` / `AddAuthorization` configured but no identity provider attached yet. This satisfies the authorization boundary requirement without hardcoding an OIDC provider too early.

**Consequences**: When an external identity provider (e.g., Azure App Service EasyAuth / Entra ID) is added in a future phase, it slots into the existing policy structure without route changes.

**Alternatives considered**:
- Configuring Azure Entra ID OIDC in this phase: Deferred — requires tenant configuration not yet established; premature for a local-first foundation.
- API key authentication for admin routes in Phase 0: Rejected — more complex than a stub policy registration; admin CRUD endpoints don't exist yet.
- No auth configuration: Rejected — violates constitution Principle VIII (clear authorization boundaries must be enforced from day one).

---

### 3. Structured Logging Implementation

**Decision**: Use ASP.NET Core's built-in `ILogger<T>` with a custom `RequestLoggingMiddleware` to capture all required fields per spec FR-009. No third-party logging framework (e.g., Serilog) in Phase 0; it can be added in a later phase if structured output format to a sink is needed.

**Required fields per spec FR-009 / Jumpstart §16.1**:
- Request path
- HTTP method
- Status code
- Duration (ms)
- Correlation ID (from `HttpContext.TraceIdentifier` or a custom `X-Correlation-ID` header)
- User ID / auth subject (when authenticated: `HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)`)
- Feature name (derived from route)
- Operation name (derived from route pattern)
- Success/failure indicator (`statusCode < 400`)

**Alternatives considered**:
- Serilog with structured JSON output: Deferred — excellent choice for production but adds a dependency and configuration complexity not needed for foundation phase; add when deploying to Azure with Application Insights sink.
- OpenTelemetry: Deferred — heavier setup; appropriate when distributed tracing across services is needed (not applicable to single-backend model).
- No custom middleware (rely on built-in ASP.NET Core request logging): Rejected — built-in request logging does not capture all required fields (correlation ID, user subject, feature name, success indicator).

---

### 4. CORS Configuration

**Decision**: Configure CORS with named policy `"ApiSparkPolicy"` allowing only explicitly listed origins. Origins are loaded from `AllowedOrigins` configuration array (not hardcoded). In local development, `http://localhost:xxxx` entries are added via `appsettings.Development.json`.

**Rationale**: Wildcard CORS (`AllowAnyOrigin`) is forbidden for authenticated routes (constitution Principle IV, spec FR-008). Making origins configuration-driven allows Azure App Service settings to control production origins without code changes.

**Configuration pattern**:

```json
// appsettings.json (no origins — safe default)
{
  "AllowedOrigins": []
}

// appsettings.Development.json
{
  "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
}
```

**Azure App Service settings** (production):
```
AllowedOrigins__0=https://markhazleton.com
AllowedOrigins__1=https://promptspark.markhazleton.com
```

**Alternatives considered**:
- Hardcoded origin list: Rejected — origins change as client sites are added; configuration is cleaner.
- `AllowAnyOrigin`: Rejected — violates spec FR-008 and constitution Principle IV for authenticated routes.

---

### 5. SQLite Database Path Strategy

**Decision**: Use two separate default paths:
- **Local development**: `./data/apispark.local.db` (relative to project output, created automatically)
- **Production (Azure App Service Linux)**: `/home/data/apispark.db` (persistent storage, survives redeployments)

`./data/` directory is added to `.gitignore` to prevent accidental commit of local DB files.

**Configuration**:

```json
// appsettings.json (production default)
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/home/data/apispark.db"
  }
}

// appsettings.Development.json (local override)
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./data/apispark.local.db"
  }
}
```

**Alternatives considered**:
- Single path with environment-specific override only in Azure: Rejected — local developers shouldn't need to create `/home/data/`; the Development config override is cleaner.
- Using `Path.Combine(AppContext.BaseDirectory, "apispark.db")`: Rejected — less obvious; `./data/` directory makes the local DB easy to find and easy to `.gitignore`.

---

## Summary of Resolutions

| Topic | Decision | Resolved |
|-------|----------|----------|
| Startup migration | `db.Database.Migrate()` gated by `Database:ApplyMigrationsOnStartup` config flag | ✅ |
| Auth for Phase 0 | Register policies; no external IdP yet; admin routes return 401 by default | ✅ |
| Structured logging | Custom `RequestLoggingMiddleware` using built-in `ILogger<T>` | ✅ |
| CORS | Named policy; origins from config array; no wildcard | ✅ |
| SQLite paths | `./data/apispark.local.db` (dev) / `/home/data/apispark.db` (prod) via config | ✅ |

All NEEDS CLARIFICATION items are resolved. Proceed to Phase 1: Design & Contracts.
