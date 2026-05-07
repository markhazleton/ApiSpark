# ApiSpark Constitution

## Core Principles

### I. API-First (NON-NEGOTIABLE)

Design and document APIs before implementation begins.
All endpoints must have defined contracts (OpenAPI/Swagger) before any implementation work starts.
API design decisions must be explicit, versioned, and backward-compatible.

### II. Test-First (NON-NEGOTIABLE)

Tests must be written before or alongside implementation — never after.
No feature is considered complete without passing tests that verify its behavior.
All PRs must include tests; untested code changes are rejected in review.

### III. Simplicity (NON-NEGOTIABLE)

Prefer simple, readable solutions over clever ones.
Complexity must be justified. Reject abstractions that serve only one use case.
If a solution requires a long explanation, reconsider the solution.

### IV. Security by Default (NON-NEGOTIABLE)

Treat security as a first-class concern in every change.
Input validation, authentication, authorization, and secrets management must be considered at design time.
Security issues are showstopper severity in PR review.

### V. Spec-Driven Development

All features must be specified before they are planned or implemented.
The workflow is: specify → plan → tasks → implement → PR.
Skipping specification requires explicit documented justification.

### VI. Ownership Boundary

`.devspark/` is the installed framework payload — the only directory DevSpark installs, upgrades, or removes.
`.documentation/` directories are repository-owned work product and must never be modified by framework operations.

## Technology Stack

- **Language**: C# (.NET — ASP.NET Core, Web API)
- **API Documentation**: OpenAPI / Swagger
- **Testing**: xUnit / NUnit / MSTest with test-first discipline
- **Scripts**: PowerShell (primary), Bash (cross-platform fallback)
- **Source Control**: Git / GitHub

## Development Workflow

- All features follow the DevSpark spec-driven workflow: `/devspark.specify` → `/devspark.plan` → `/devspark.tasks` → `/devspark.implement` → `/devspark.create-pr`
- All PRs and reviews must verify compliance with this constitution
- API contracts must be defined and reviewed before implementation begins
- Security review is required for any changes to authentication, authorization, input handling, or data access

## Governance

This constitution is authoritative over all development practices in this repository.
Amendments require documentation of the change, author approval, and a migration plan for any affected workflows.

**Project**: ApiSpark | **Version**: 1.0.0 | **Ratified**: 2026-05-06
