## Context

See proposal.md - Why for motivation. Current state (mapped from the repo on `lab1/clean`/`main`):

```
HackerRank1 (single web project, net8.0)               LibraryService.Integration.Test (net8.0, works)
├─ Program.cs / Startup.cs  (LibraryService.WebAPI)     IntegrationTest (net6.0, orphan — cannot ref net8)
├─ Controllers/  AuthController, BooksController,
│                LibrariesController
├─ Services/     AuthenticationService (+ IAuthService),
│                LibrariesService (+ ILibrariesService),
│                BooksService (+ IBooksService)
├─ DTO/          BookForm, LibraryForm, User
├─ Entities/     JwtSettings
├─ Data/         LibraryContext (holds DbSets AND the
│                Book + Library entity classes inline)
├─ Helpers/      TokenGenerator
└─ Migrations/   EF Core (FK Book→Library CASCADE)
```

The web project mixes presentation, application, and persistence in one project and uses two namespace families (`LibraryService.WebAPI.*` and `HackerRank1.*`). Domain entities (`Book`, `Library`) are embedded in `Data/LibraryContext.cs`, so the "domain" is directly coupled to EF Core. Services query `LibraryContext` directly, so use cases cannot run without the ORM. The API contract is locked by read-only integration tests, so the layering work must be behavior-neutral.

## Goals / Non-Goals

**Goals:**
- Introduce four Clean Architecture projects with dependency flow `API → Application → Domain` and `Infrastructure → Domain/Application`; no inward-project references (DIP enforced by project structure).
- Move domain entities (`Book`, `Library`, `JwtSettings`) into `Domain/Entities` and persistence abstractions (`ILibraryRepository`, `IBookRepository`) into `Domain/Repositories`.
- Invert `Service → DbContext` into `Application service → Domain repository interface ← Infrastructure EF implementation`.
- Consolidate namespaces to `LibraryService.{Domain, Application, Infrastructure, API}`.
- Centralize composition in the API shell via `AddApplication()` + `AddInfrastructure(config)`.
- Preserve the exact endpoint contract and repository **existing** behavior (including the intact `NotImplementedException` stubs as-is).

**Non-Goals:**
- Not implementing the missing POST/DELETE actions or the `Add/Update/Delete` service stubs (separate feature; see proposal "Out of Scope").
- Not adding repositories/unit-of-work beyond what's needed to decouple — the existing service methods `Get/Add/AddRange/Update/Delete` map 1:1 to repository methods.
- Not introducing CQRS, Event Sourcing, advanced DDD (aggregates, value objects, domain events, domain services), or a DI container library.
- Not changing auth flow, JWT settings, CORS origins, or the DB schema (FK cascade preserved).

## Decisions

### D1. Solution layout — four net8.0 projects
```
lab1Paradigma/
├─ LibraryService.Domain/            (lib, no packages)
│  └─ Entities/   Book, Library, JwtSettings
│  └─ Repositories/ ILibraryRepository, IBookRepository   (persistence abstractions)
├─ LibraryService.Application/       (refs Domain only)
│  └─ Services/   ILibrariesService+I, IBooksService+I, IAuthenticationService+I
│  └─ DTO/        LibraryForm, BookForm, User
│  └─ DependencyInjection/ (AddApplication)
├─ LibraryService.Infrastructure/    (refs Application, Domain)
│  └─ Data/       LibraryContext (holds DbSets only now)
│  └─ Migrations/ (moved, preserved)
│  └─ Repositories/ LibraryRepository, BookRepository (EF impls of Domain interfaces)
│  └─ DependencyInjection/ (AddInfrastructure + DbContextPool/Npgsql/retry)
└─ LibraryService.API/               (refs Application, Infrastructure, Domain)
   ├─ Program.cs, Startup.cs
   ├─ Controllers/ Auth, Books, Libraries
   ├─ Helpers/    TokenGenerator
   └─ appsettings.*, Properties/
```
**Rationale:** four projects enforce the Clean Architecture dependency rule at build time. **Alternative considered:** keeping a single project with folders only — rejected because it cannot enforce `Domain` independence from EF/ASP.NET at compile time.

### D2. Dependency inversion via Domain persistence abstractions
- Extract `Book` and `Library` out of `Data/LibraryContext.cs` into `Domain/Entities` unchanged (no invented business rules — they are plain data today).
- Define `ILibraryRepository`/`IBookRepository` in `Domain/Repositories` with methods mirroring today's context queries exactly (`Get/Add/AddRange/Update/Delete`).
- Rewire `LibrariesService`/`BooksService` constructors to depend on those interfaces instead of `LibraryContext`; the exact `Get` filtering and the intact `NotImplementedException` stubs are preserved.
- **Rationale:** moves EF into one project and makes Application unit-testable. **Alternative considered:** keep services on EF and only relocate `DbContext` — rejected for leaving a data-access dependency in Application.

### D3. Infrastructure implements the abstractions
- `LibraryContext` stays named `LibraryContext` (the integration test substitutes it by type via `RemoveAll`/`AddSingleton`, so the name matters).
- `LibraryRepository`/`BookRepository` implement the Domain interfaces with EF; the query logic that previously lived in the services moves into the repositories verbatim.
- **Rationale:** this is the DIP — Infrastructure "points up" to the abstractions the core declares. **Alternative considered:** interfaces in Application — acceptable but Domain interfaces keep the persistence contract closest to the entities.

### D4. Composition root lives in the API project
- `Startup.ConfigureServices` calls `services.AddApplication()` then `services.AddInfrastructure(config)`, mapping only in the shell.
- Application and Infrastructure services registered **scoped** (fixed: today services are `AddTransient` while `DbContext` is scoped).
- **Rationale:** single registration site; aligns lifetimes with `DbContextPool`'s scoped slice. **Alternative considered:** Autofac — unnecessary dependency.

### D5. Single, consistent namespace scheme
- Replace `LibraryService.WebAPI.*` and `HackerRank1.*` with project-root namespaces (`LibraryService.Domain`, `.Application`, `.Infrastructure`, `.API`). Solution-wide search/replace, then compile-clean any strays.

### D6. Packages move per layer
- `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.EntityFrameworkCore` → Infrastructure.
- `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore`, `Newtonsoft.Json`, `MSTest.TestFramework` → API.
- `Microsoft.Extensions.DependencyInjection.Abstractions` + `Newtonsoft.Json` → Application (DTO `[JsonProperty]` attributes live there).
- Domain: no packages.

### D7. Remove the orphaned net6 `IntegrationTest`
- `IntegrationTest` (=net6) cannot reference the net8 web project; it duplicates `LibraryService.Integration.Test`. Delete the folder (and confirm no `.sln` entry); keep `LibraryService.Integration.Test` and update its usings/project-reference target.

## Risks / Trade-offs

- [Behavior drift while moving files] → Every step keeps the code compiling; endpoint contract unchanged; run `LibraryService.Integration.Test` after each milestone as a smoke check.
- [Reference cycle if layering violated] → D1 gives one shared dependency direction; enforce via `ProjectReference` (only the sanctioned set per project); audit all references at the end.
- [Namespace rename ripple] → Do the rename top-down (Domain first), one pass per project, compile after each project.
- [DbContextPool + migration move breaks test boot] → The test project short-circuits the real DB with SQLite in-memory via `RemoveAll(typeof(LibraryContext))`/`AddSingleton`; after re-homing ensure `EnsureCreated()`/`Migrate()` call sites still wrap the same `LibraryContext` type and that the test project references resolve.
- [TokenGenerator/JwtSettings placement] → `TokenGenerator` (JWT signing via `Microsoft.IdentityModel.Tokens`) stays in API/Helpers; `JwtSettings` binding model stays in Domain (config/entity, dependency-free). If the auth flow later needs Application access, hoist the abstraction then.
- [Pre-existing test failure] → The integration tests currently fail with `MissingMethodException` (test project uses EF Core 6 packages vs the EF 8 runtime the web project brings). This predates the migration and is out of scope to fix; behavior must stay identical before/after.

## Migration Plan

0. Ensure working branch is `lab1/clean` (cut from `main`); all migration commits stay on it.
1. Create the four projects (`dotnet new`) + update `.sln`; wire ProjectReferences per D1.
2. Domain: extract entities out of `LibraryContext.cs` into `Domain/Entities`; add repository interfaces in `Domain/Repositories`; compile (zero external deps).
3. Application: move services + DTOs; rewire constructors to repository interfaces; add `AddApplication()`; compile.
4. Infrastructure: move `LibraryContext` + Migrations; implement `LibraryRepository`/`BookRepository`; add `AddInfrastructure()`; compile.
5. API: re-home `Program`/`Startup`/`Controllers`/`TokenGenerator`/config; swap manual registrations for `AddApplication()`/`AddInfrastructure()`; rename namespaces solution-wide; compile each project.
6. Tests & cleanup: update `LibraryService.Integration.Test` usings/project-reference; delete net6 `IntegrationTest`; full `dotnet clean && restore && build`.
7. Verification: run tests, audit references/dependencies, confirm endpoints unchanged, confirm branch isolation.
- **Rollback:** single atomic `git revert` of the change commit; `main`/`lab1/clean` (pre-migration) is the base, and the branch can be discarded and re-cut.

## Open Questions

No open questions that would change the approach. (`JwtSettings` in Domain vs API is a minor taste call; chosen Domain to keep the binding model dependency-free and consistent with entity placement.)
