## Why

Today the entire solution lives in one `HackerRank1` web project (net8.0) that mixes every concern: controllers and auth (`Controllers`, `TokenGenerator`, JWT) sit beside EF Core persistence (`LibraryContext`, `Migrations`, Npgsql), business logic (`Services`), DTOs, and entities (`Book`, `Library` even live inside `Data/LibraryContext.cs`). Two namespace families (`LibraryService.WebAPI.*` and `HackerRank1.*`) add confusion. The code works, but the domain and use cases are coupled to EF Core and ASP.NET: services construct/query `LibraryContext` directly, so they cannot be tested in isolation, persisted elsewhere, or reused without the web framework. Reorganizing it into a Clean Architecture (Domain / Application / Infrastructure / API) protects the business core by inverting those dependencies, without changing any externally visible API behavior.

## What Changes

- Split the solution into **four net8.0 projects** following Clean Architecture, so dependencies point inward only:
  - `LibraryService.Domain` — entities (`Book`, `Library`, `JwtSettings`) and persistence abstractions (`ILibraryRepository`, `IBookRepository`) only. No EF, ASP.NET, HTTP, or Newtonsoft references.
  - `LibraryService.Application` — use cases / application services (`ILibrariesService`+impl, `IBooksService`+impl, `IAuthenticationService`+impl) and DTOs (`BookForm`, `LibraryForm`, `User`). Depends on `Domain` only.
  - `LibraryService.Infrastructure` — concrete details: EF Core `LibraryContext`, `Migrations`, and repository implementations (`LibraryRepository`, `BookRepository`) that implement the Domain abstractions. Depends on `Domain` (and `Application` for composition wiring), never on the web layer.
  - `LibraryService.API` (renamed from `HackerRank1`) — web/API shell and composition root: `Controllers`, `Startup`, `Program`, `TokenGenerator` (JWT helper), `appsettings`, Swagger, CORS, JWT wiring.
- **Invert the persistence dependency**: replace direct `Service → DbContext` dependencies with `Application service → IRepository (Domain) ← Infrastructure repository → DbContext`.
- Reconcile the two namespace families into project-scoped namespaces (`LibraryService.Domain`, `.Application`, `.Infrastructure`, `.API`).
- Move all DI registration into the API composition root via `AddApplication()` and `AddInfrastructure(config)` extension methods; Application and Infrastructure services registered **scoped**.
- Keep the API contract untouched: same endpoints, routes, verbs, DTO shapes, response codes, auth flow, CORS, Swagger, and DB schema (FK `Book→Library` CASCADE preserved).
- Update `LibraryService.Integration.Test` for the new namespaces/project references; remove the orphaned net6 `IntegrationTest` duplicate from the build.
- Perform all work on branch `lab1/clean` (cut from `main`), isolated from the `architecture/n-layer` and future vertical-slice implementations.

## Capabilities

### New Capabilities
None — this is a pure structural refactor. External behavior of every HTTP endpoint and its response codes is unchanged, so there are no new spec-level behaviors to describe. The change sets `skip_specs: true` in `.openspec.yaml` accordingly.

### Modified Capabilities
None — no requirement-level behavior changes.

## Impact

- **Projects/files moved and renamed**: `HackerRank1` → `LibraryService.API`; new `Domain`, `Application`, `Infrastructure` projects. `HackerRank1.csproj`, `Program.cs`, `Startup.cs`, `Controllers/*`, `Services/*`, `DTO/*`, `Data/LibraryContext.cs`, `Migrations/*`, `Helpers/TokenGenerator.cs`, `Entities/JwtSettings.cs` re-homed.
- **Namespaces change** across all four projects — search-replace across `.cs` files.
- **`Book` and `Library` entities move out of `Data/LibraryContext.cs`** into `Domain/Entities` (they currently have no business rules, so they move as-is without inventing domain logic).
- **csproj / solution references**: API → Application, Infrastructure, Domain; Infrastructure → Application, Domain; Application → Domain. `.sln` updated to five projects (including the test project).
- **Packages move per layer**: EF Core + Npgsql + EF Design → Infrastructure; JwtBearer, Swashbuckle, Newtonsoft, MSTest → API; `Microsoft.Extensions.DependencyInjection.Abstractions` + Newtonsoft (DTO attributes) → Application; Domain stays package-free.
- **`LibraryService.Integration.Test`** updated for new namespaces; orphaned net6 `IntegrationTest` removed.
- No database-migration *schema* change: `DbContext` moves but tables/keys/FK cascade are preserved.
- appsettings/config unchanged except file ownership movement.

## Out of Scope (non-goals)

- Do NOT implement the existing `NotImplementedException` stubs (`LibrariesService.Delete`, `BooksService.Add/Update/Delete`) or the missing `DELETE`/`POST` controller actions — fixing incomplete endpoints is a separate feature change, not this reorg pass.
- Do NOT introduce CQRS, Event Sourcing, advanced DDD (aggregates, value objects, domain events), or any pattern beyond what Clean Architecture requires.
- Do NOT add repositories/unit-of-work beyond what's needed to invert the `Service → DbContext` dependency; existing service methods map 1:1 to repository methods.
- Do NOT change auth flow, JWT settings, CORS origins, or the DB schema.
- Do NOT add a DI container library; use the built-in `IServiceCollection`.

## Acceptance Criteria / Validation

The migration is successful only when **all** of the following hold after each milestone and at the end:

- The full solution compiles with no errors (`dotnet build` clean, no new warnings from re-homing).
- Dependencies respect Clean Architecture direction: `API → Application → Domain` and `Infrastructure → Domain/Application`; **no** `Domain → Infrastructure/EF/PostgreSQL/HTTP/API` and **no** `Application → Infrastructure` concrete implementations.
- `Domain` compiles with zero references to EF Core, ASP.NET, HTTP, PostgreSQL, or Newtonsoft.
- `Application` compiles with zero references to EF Core/PostgreSQL and no concrete infrastructure types; all persistence access goes through Domain abstractions.
- `Infrastructure` implements the abstractions required by the inner layers (repository interfaces); no business rules live in Infrastructure.
- Controllers are thin: they only map HTTP to Application use cases; no direct `Controller → DbContext` and no business logic in controllers.
- `LibraryService.Integration.Test` still compiles and the existing behaviors (`TestAddBook`, `TestGetBooks`, `TestDeleteLibrary`) run unchanged.
- Existing endpoints keep their exact routes, verbs, request/response shapes, and status codes; auth (JWT `admin/1234`), CORS, and Swagger behave the same.
- Database connectivity works: migrations run and the app boots against the configured Supabase; schema (FK `Book→Library` CASCADE, PKs) preserved.
- No unnecessary architectural dependencies introduced: only layer-appropriate packages; no patterns beyond Clean Architecture.
- All work is confined to branch `lab1/clean`, isolated from `architecture/n-layer` and future vertical-slice work.
