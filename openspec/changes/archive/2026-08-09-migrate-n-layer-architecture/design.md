## Context

See proposal.md - Why for motivation. Current state (from mapping the repo):

```
HackerRank1 (single project, net8.0)                  LibraryService.Integration.Test (net8.0, works)
├─ Program.cs / Startup.cs                            IntegrationTest (net6.0, orphaned — cannot ref net8)
├─ Controllers/  Auth, Books, Libraries
├─ Services/     AuthenticationService, LibraryService,
│                BookService  (+ interfaces)
├─ DTO/          BookForm, LibraryForm, User
├─ Entities/     JwtSettings
├─ Data/         LibraryContext (holds Book + Library too)
├─ Helpers/      TokenGenerator
└─ Migrations/   EF Core (FK Book→Library CASCADE)
```

The web project mixes presentation, application, and persistence in one project and uses two namespace families (`LibraryService.WebAPI.*` and `HackerRank1.*`). The API contract is locked by read-only integration tests, so the layering work must be behavior-neutral.

## Goals / Non-Goals

**Goals:**
- Introduce conventional four-layer projects so dependencies point only downward: Presentation → (Domain, Infrastructure, Application) and Application/Infrastructure never reference upward.
- Persistence abstractions in `Domain`; EF data access in `Infrastructure` (services no longer depending on `LibraryContext` directly).
- Consolidate namespaces to `LibraryService.{Presentation,Application,Domain,Infrastructure}`.
- Centralize composition (DI) in the web shell via composition-root extension methods from Application/Infrastructure.
- Preserve the exact endpoint contract and repository **existing** behavior (including the intact `NotImplementedException` stubs as-is).

**Non-Goals:**
- Not implementing the missing POST/DELETE actions or the `Add/Update/Delete` service stubs (separate feature; see proposal "Out of Scope").
- Not adding repositories/unit-of-work beyond what's needed to decouple — the existing service methods `Get/Add/AddRange/Update/Delete` map 1:1 to repository methods.
- Not changing auth flow, JWT settings, CORS origins, or the DB schema (FK cascade preserved).
- Not introducing a DI container library; use the built-in `IServiceCollection`.

## Decisions

### D1. Solution layout — four net8.0 projects
```
lab1Paradigma/
├─ LibraryService.Domain/            (lib, no packages)
│  └─ Entities/   Book, Library, JwtSettings
│  └─ Repositories/ ILibraryRepository, IBookRepository   (persistence abstractions)
├─ LibraryService.Application/       (refs Domain nly)
│  └─ Services/   ILibrariesService+I impl, IBooksService+I impl, IAuthenticationService+I
│  └─ DTO/        LibraryForm, BookForm, User
│  └─ DependencyInjection/ (AddApplication)
├─ LibraryService.Infrastructure/    (refs Application, Domain)
│  └─ Data/       LibraryContext (renamed; holds DbSet) + EF entity configurations
│  └─ Migrations/ (moved, preserved)
│  └─ Repositories/ LibraryRepository, BookRepository (EF impls)
│  └─ DependencyInjection/ (AddInfrastructure + DbContextPool/Npgsql/retry)
└─ LibraryService.Presentation/      (refs Domain, Application, Infrastructure)
   ├─ Program.cs, Startup.cs
   ├─ Controllers/ Auth, Books, Libraries
   ├─ Helpers/    TokenGenerator
   └─ appsettings.*, Properties/
```
**Rationale:** classic N-layer gives clear dependency rules and testability. **Alternative considered:** keeping a single project with folders only — rejected because it doesn't enforce the layering/namespace isolation `Domain` is decoupled from EF and ASP.NET.

### D2. A separation via persistence abstractions in Domain
- `IDb...` Instead of services `new`/querying `LibraryContext` directly, `Library` services depend on `ILibraryRepository`/`IBookRepository` (methods mirror today's context queries exactly). EF implementations live in Infrastructure.
- **Rationale:** moves EF into one project, makes Application unit-testable. **A.F. considered:** keep services EF and put only DbContext in Infrastructure — rejected for leaving a data-access dependency in Application.

### D3. Composition root lives in Presentation
- Presentational `LibraryService.Presentation.Startup.ConfigureServices` calls `services.AddApplication()` then `services.AddInfrastructure(config)`, keeping web-`Startup` as sole mapper of ODanyer to implementations.
- Register Application/Infrastructure services **scoped** (fixed: currently services are `AddTransient` while `DbContext` is scoped).
- **Rationale:** single registration site; aligns lifetimes (scoped) with `DbContextPool`'s scoped slice. **Alternative considered:** Autofac — unnecessary dependency.

### D4. Single, consistent namespace scheme
- Replace `LibraryService.WebAPI.*` and `HackerRank1.*` with project-root namespaces, and JS namespace per project (`LibraryService.Domain`, `.Application`, `.Infrastructure`, `.Presentation`). Use solution-wide search/replace, then compile-clean any strays.

### D5. EF Core packages move to Infrastructure; web packages stay in Presentation
- `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design` → Infrastructure.
- `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore`, `Newtonsoft.Json` (DTO + Swagger/serialization) → Presentation.
- **Deviation (resolved during apply):** D1 places `BookForm`/`LibraryForm` (which carry `[JsonProperty]` attributes) in Application, so Application must also reference `Newtonsoft.Json` — the DTO-placement decision (D1) takes precedence over "Newtonsoft only in Presentation". Presentation still references Newtonsoft for serialization. Domain remains package-free.

### D6. Remove the orphaned net6 `IntegrationTest`
- `IntegrationTest` (=net6) cannot reference the net8 web project; it's a duplicate of `LibraryService.Integration.Test`. Delete the folder and its `.sln` entry; keep `LibraryService.Integration.Test` and update its usings/project-reference target.

## Risks / Trade-offs

- [Behavior drift while moving files] → Every step keeps the code compiling; endpoint contract unchanged; run `LibraryService.Integration.Test` after each milestone and (`TestAddBook`, `TestGetBooks`, `TestDelete`) as smoke check.
- [Reference cycle if layering violated] → D1 gives one shared dependency direction; keep a small readme of "referencing rules"; grep enforcement via `ProjectReference` (only the sanctioned set per project). All the `ProjectReference`s in `.sln` audited at the end.
- [Namespace rename ripple] → Do the rename top-down (Domain first) and keep one `sed`-style pass per project; compile after each project to catch strays.
- [DbContextPool + migration move breaks test boot] → Testproject short-circuits real DB with SQLite in-memory; after re-homing ensure `EnsureCreated()`/`Migrate()` call sites still wrap the same context.
- [TokenGenerator/JwtSettings moved to a different layer] → They carry no rev/asp deps except `JwtBearer`/identity (Presentation); keep them there; if `JwtSettings` referenced by media Application later, hoist binding but keep model in Domain.

## Migration Plan

0. Create branch `architecture/n-layer` from `lab1/layers`; do all migration work on it (keep `architecture/clean` / `architecture/vertical-slice` independent later).
1. `New via CLI` the four projects + update `.sln` (net8.0, refs: Presentation→Domain/App/Infra; Infra→Application/Domain; Application→Domain).
2. Move files: Domain (entities+ JwtSettings + repo interfaces) → then Application (services, DTO) → then Infrastructure (context [migrations, repos, EF packages) → then compose next.
3. Presentation: keep Controllers/Swagger/CORS/JWT but reduce their imports; swap direct `LibraryContext` service dependencies for repository-based; rename namespaces.
4. Update `LibraryService.Integration.Test` usings; delete net6 `IntegrationTest`.
5. `dotnet clean && dotnet restore && dotnet build` + `dotnet test` (smoke).
6. Update DI registrations (scope/lifetimes), verify boot (migrations run against configured Supabase).
- **Rollback:** single atomic `git revert` of the change commit; branch `lab1/layers` is the pre-migration base, and `architecture/n-layer` can be discarded and re-cut.

## Acceptance Criteria

The migration is complete and behavior-preserving when:

- `dotnet build` of the whole solution succeeds with no errors and no new warnings from re-homing.
- Project reference graph is exactly the sanctioned set (D1); no `Controller` or Application code reaches `DbContext` directly (D2 enforced), and DataAccess holds no business rules.
- `dotnet test` on `LibraryService.Integration.Test` passes `TestAddBook`, `TestGetBooks`, `TestDeleteLibrary` unchanged.
- All existing endpoints keep routes, verbs, request/response shapes, and status codes; integration-test contract untouched.
- App boots against Supabase with migrations applied; DB schema unchanged (FK `Book→Library` CASCADE, PKs).
- Only layer-appropriate packages exist (D5); no architectural patterns beyond N-Layer added.
- All changes live on branch `architecture/n-layer` only.

## Open Questions

No open questions that would change the approach. (JwtSettings placement in Domain vs Application is a minor taste call; chosen Domain to keep config/entities dependency-free.)