## 1. Scaffold projects & structure

- [x] 1.0 Create and switch to branch `architecture/n-layer` from `lab1/layers`; keep all migration commits on it.
- [x] 1.1 Create new Class Library projects `LibraryService.Domain`, `LibraryService.Application`, `LibraryService.Infrastructure` (net8.0) and a web project `LibraryService.Presentation` (net8.0, web SDK) at the solution root using `dotnet new`.
- [x] 1.2 Rename/re-root the existing `HackerRank1` project and solution entry to `LibraryService.Presentation` (keep web SDK) and register all four projects in `HackerRank1.sln`.
- [x] 1.3 Wire `ProjectReference`s to match D1: Presentation → Domain, Application, Infrastructure; Infrastructure → Application, Domain; Application → Domain (no upward references).
- [x] 1.4 Add package references per D5: EF Core + Npgsql + EF Design to Infrastructure; JwtBearer, Swashbuckle, Newtonsoft.Json to Presentation; ensure nothing EF/xstock leaks into Domain/Application `csproj`.

## 2. Domain layer

- [x] 2.1 Create `LibraryService.Domain` and move `Book` and `Library` entities (from `Data/LibraryContext.cs`) plus `JwtSettings` (`Entities/JwtSettings.cs`) into `Domain/Entities`, renaming namespaces to `LibraryService.Domain.Entities`.
- [x] 2.2 Define persistence abstractions `ILibraryRepository` and `IBookRepository` under `Domain/Repositories` with methods mirroring today's context queries (Get/Add/AddRange/Update/Delete).
- [x] 2.3 Compile `Domain` and confirm it has zero references to EF Core, ASP.NET, or Newtonsoft.

## 3. Application layer

- [x] 3.1 Move `Services/` implementations and interfaces (`LibrariesService`+`ILibrariesService`, `BooksService`+`IBooksService`, `AuthenticationService`+`IAuthenticationService`) into `Application/Services`, namespacing as `LibraryService.Application.Services`.
- [x] 3.2 Rewire service constructors to depend on `ILibraryRepository`/`IBookRepository` (from Domain) instead of `LibraryContext`; preserve exact `Get` filtering and existing method behavior, including the intact `NotImplementedException` stubs.
- [x] 3.3 Move `DTO/BookForm`, `DTO/LibraryForm`, `DTO/User` into `Application/DTO` (`LibraryService.Application.DTO`), updating usages.
- [x] 3.4 Add an `Application/DependencyInjection` extension (`AddApplication(this IServiceCollection)`) registering Application services as **scoped**.

## 4. Infrastructure layer

- [x] 4.1 Move `Data/LibraryContext.cs` `DbContext` into `Infrastructure/Data/LibraryDbContext` and keep the `DbSet<Library>`/`DbSet<Book>` mappings.
- [x] 4.2 Move `Migrations/*` into `Infrastructure/Migrations`; verify the model snapshot still matches (FK `Book→Library` CASCADE, PK configurations).
- [x] 4.3 Implement `LibraryRepository` and `BookRepository` in `Infrastructure/Repositories` using Entity Framework, mapping existing service query to repo methods.
- [x] 4.4 Add `Infrastructure/DependencyInjection` extension (`AddInfrastructure(this IServiceCollection, IConfiguration)`) registering `DbContextPool<LibraryContext>` (Npgsql, enable retry, poolSize 20) and the EF repository implementations as **scoped**.

## 5. Presentation layer

- [x] 5.1 Move `Startup.cs`/`Program.cs` to `Presentation`, namespacing `LibraryService.Presentation`.
- [x] 5.2 Move `Controllers/AuthController`, `BooksController`, `LibrariesController` into `Presentation/Controllers`, adding `using` for the new Application/Domain namespaces.
- [x] 5.3 Move `Helpers/TokenGenerator.cs` into `Presentation/Helpers`; keep `appsettings.*`, `Properties`, Swagger, CORS, JWT wiring in Presentation.
- [x] 5.4 In `Startup.ConfigureServices`, replace manual service/DbContext registrations with `services.AddApplication()` + `services.AddInfrastructure(config)`; keep JWT/CORS/controllers/Swagger wiring and the `db.Database.Migrate()` boot call.
- [x] 5.5 Search-replace entire solution for legacy namespaces (`LibraryService.WebAPI.*`, `HackerRank1.*`) → new project namespaces; compile each project to catch strays.

## 6. Tests & cleanup

- [x] 6.1 Update `LibraryService.Integration.Test` usings/project-reference to the new namespaces (`LibraryService.Presentation`, etc.) and confirm its SQLite in-memory `LibraryContext` substitution still compiles.
- [x] 6.2 Remove the orphaned net6 `IntegrationTest` project folder and its `.sln` entry (duplicate; cannot reference the net8 web project).
- [x] 6.3 `dotnet clean && dotnet restore && dotnet build` — solution builds with no errors or warnings from re-homing.
- [x] 6.4 `dotnet test` on `LibraryService.Integration.Test` — the three behaviors (`TestAddBook`, `TestGetBooks`, `TestDeleteLibrary`) pass unchanged.
- [x] 6.5 Delete the leftover `LibraryService.Integration.Test` duplicate `Extensions/` class or keep single owner; remove any leftover empty `HackerRank1` artifacts.

## 7. Acceptance verification

- [x] 7.1 `dotnet clean && dotnet restore && dotnet build` succeeds solution-wide with no errors or new warnings.
- [x] 7.2 Audit `.sln` ProjectReferences match D1 (Presentation → Domain/App/Infra; Infra → App/Domain; App → Domain); grep confirms no `Controller → DbContext` and no business rules in DataAccess.
- [x] 7.3 `dotnet test` on `LibraryService.Integration.Test` — `TestAddBook`, `TestGetBooks`, `TestDeleteLibrary` pass unchanged.
- [x] 7.4 Confirm endpoints/routes/verbs/response shapes and status codes are unchanged (integration-test contract intact); API boots with migrations against Supabase.
- [x] 7.5 Verify package placement per D5 (EF/Npgsql only in Infrastructure; JWT/Swagger/Newtonsoft only in Presentation) and that only N-Layer patterns were introduced.
- [x] 7.6 Confirm all changes are confined to branch `architecture/n-layer`.