## Why

Today the entire solution lives in one `HackerRank1` project that mixes concerns: web infrastructure (`Startup`/DI/`Controllers`/`TokenGenerator`) sits beside EF Core persistence (`LibraryContext`, `Migrations`), business logic (`Services`), and entities — all sharing the `LibraryService.WebAPI` namespace while the auth side uses `HackerRank1.*`. The code works, but the coupling makes it hard to test services in isolation, swap persistence, or reuse the domain. Splitting it into a conventional .NET N-layer (Presentation / Application / Domain / Infrastructure) establishes clean dependency rules and testability without changing any externally visible API behavior.

## What Changes

- Split the solution into **four projects** following .NET N-Layer best practices:
  - `LibraryService.Domain` — entities (`Book`, `Library`, `JwtSettings`) and abstractions/contracts only, no EF or ASP.NET dependencies.
  - `LibraryService.Application` — business logic: service interfaces + implementations (`ILibrariesService`, `IBooksService`, `IAuthenticationService`), DTOs (`BookForm`, `LibraryForm`, `User`), application exceptions.
  - `LibraryService.Infrastructure` — persistence: EF Core `LibraryContext`, entity configuration/Fluent API, and `Migrations`; depends on Domain/Application, not on the web layer.
  - `LibraryService.Presentation` (renamed from `HackerRank1`) — web/API shell: `Controllers`, `Startup`, `Program`, `TokenGenerator` (helper), `appsettings`, Swagger, CORS.
- Reconcile the two **namespace families** (`LibraryService.WebAPI.*` and `HackerRank1.*`) into project-scoped namespaces (e.g. `LibraryService.Presentation`, `LibraryService.Application`, `LibraryService.Domain`, `LibraryService.Infrastructure`).
- Move **all** DI registrations (`DBContextPool`, auth, CORS, controllers, services) into the Presentation `Startup`, wiring Application + Infrastructure interfaces to implementations.
- Fix dependency direction rules and enforce the "no upward references" layering (Domain knows nothing; Presentation references Application + Infrastructure; Application references Domain; Infrastructure references Application + Domain).
- Update `LibraryService.Integration.Test` namespaces/usings to match the new project namespaces. Move the duplicate/orphaned `IntegrationTest` (net6.0) out of the build (it cannot reference the net8 web project). **The API contract and integration tests are unchanged.**
- De-duplicate any manufacturing between the two `IntegrationTest` / `LibraryService.Integration.Test` copies.
- Implement the migration on a dedicated git branch `architecture/n-layer`, kept separate from future `architecture/clean` and `architecture/vertical-slice` work.

## Capabilities

### New Capabilities
None — this is a pure structural refactor. External behavior of every HTTP endpoint and its response codes is unchanged, so there are no new spec-level behaviors to describe. The change sets `skip_specs: true` in `.openspec.yaml` accordingly.

### Modified Capabilities
None — no requirement-level behavior changes.

## Impact

- **Projects/files moved and renamed**: `HackerRank1` → `LibraryService.Presentation`; new `Domain`, `Application`, `Infrastructure` projects; `HackerRank1.csproj`, `Program.cs`, `Startup.cs`, `Controllers/*`, `Services/*`, `DTO/*`, `Data/LibraryContext.cs`, `Migrations/*`, `Helpers/TokenGenerator.cs`, `Entities/JwtSettings.cs` re-homed.
- **Namespaces change** across Presentation, Application, Domain, and Infrastructure — needs search-replace across `.cs` files.
- **`LibraryService.Integration.Test`** updated for new namespaces; orphaned `IntegrationTest` project removed.
- **csproj / solution references**: add new `ProjectReference`s (Presentation → Application, Infrastructure; Application → None (it drives to Domain via interfaces defined in Domain); Infrastructure → Application, Domain). Update `.sln` to reflect four projects.
- Requirements.json/appsettings unchanged except file ownership movement; it still reports `Supabase`, EF Core/Npgsql, JWT.
- No database-migration *schema* change: `DbContext` moves but tables/keys/FK cascade are preserved.

## Out of Scope (non-goals)

- Do NOT implement the existing `NotImplementedException` stubs (`LibrariesService.Delete`, `BooksService.Add/Update/Delete`) or the missing `DELETE`/`POST` controller actions — fixing incomplete endpoints is a separate feature change, not this reorg pass.
- Do NOT dockerize, add identity providers, or change the auth flow (JWT `admin/1234` stays as-is for now).

## Acceptance Criteria / Validation

The migration is successful only when **all** of the following hold after each milestone and at the end:

- The full solution compiles with no errors (`dotnet build` clean, including warnings from re-homing).
- Cross-layer references match the sanctioned set (Presentation → Application/Infrastructure/Domain; Infrastructure → Application/Domain; Application → Domain); no upward or sideways references, no `Controller → DbContext` bypass, and no business rules in the data-access layer.
- `LibraryService.Integration.Test` still compiles and the existing behaviors (`TestAddBook`, `TestGetBooks`, `TestDeleteLibrary`) pass unchanged.
- Existing endpoints keep their exact HTTP behavior, routes, and response contracts — no endpoint, DTO shape, or status code changes.
- Database connectivity still works: migrations run and the app boots against the configured Supabase; the schema (FK `Book→Library` CASCADE, PKs) is preserved.
- No unnecessary architectural dependencies introduced: only the packages required per layer (EF/Npgsql in Infrastructure; JWT/Swagger/Newtonsoft in Presentation); no new patterns beyond N-Layer.
- The final structure is genuinely N-Layer: `Presentation → Business/Application → Data Access → Database` with entities in the appropriate layer and a clear composition root.
- All work is contained in branch `architecture/n-layer`, isolated from `architecture/clean` and `architecture/vertical-slice`.