# MoviePlatform Backend

## Project Structure

```
MoviePlatform/
├── API/                          # ASP.NET Core Web API Entry Point
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── Modules/
│   └── Catalog/                  # Catalog Module (Movies, Genres)
│       ├── Catalog.Domain/       # Entities, Interfaces, Domain Events
│       ├── Catalog.Application/  # CQRS Handlers, DTOs, Validators, Mappings
│       ├── Catalog.Infrastructure/ # EF Core, Repositories, DbContext
│       └── Catalog.Contracts/    # Integration Events
└── Shared/
    ├── Shared.Kernel/            # Base Entity, Result Pattern, Pagination
    └── Shared.Infrastructure/    # UnitOfWork, MediatR Dispatcher
```

## Architecture

- **Clean Architecture**: Domain -> Application -> Infrastructure -> API
- **CQRS**: Separate Commands (writes) and Queries (reads) via MediatR
- **Repository Pattern**: Abstractions in Domain, implementations in Infrastructure
- **Result Pattern**: `Result<T>` for consistent API responses
- **Pagination**: `PagedList<T>` for all list endpoints
- **Soft Deletes**: `IsDeleted` flag with global query filters

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/movies` | Search & filter movies |
| GET | `/api/v1/movies/featured` | Get featured movies |
| GET | `/api/v1/movies/trending` | Get trending movies |
| GET | `/api/v1/movies/{id}` | Get movie details |
| POST | `/api/v1/movies` | Create new movie |
| PUT | `/api/v1/movies/{id}` | Update movie |
| DELETE | `/api/v1/movies/{id}` | Delete movie (soft) |
| GET | `/api/v1/movies/genres` | Get all genres |

## Setup

1. Ensure SQL Server LocalDB is installed (or update connection string)
2. Run migrations (auto-applied on startup):
   ```bash
   dotnet ef migrations add InitialCreate --project src/Modules/Catalog/Catalog.Infrastructure --startup-project src/API
   ```
3. Run the API:
   ```bash
   dotnet run --project src/API
   ```
4. Open Swagger: `https://localhost:7001/swagger`

## Database

- **Provider**: SQL Server (LocalDB in dev)
- **ORM**: Entity Framework Core 9
- **Migrations**: Auto-applied on startup in Development
- **Seed Data**: 8 sample movies + 12 genres auto-created
