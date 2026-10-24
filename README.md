# AZM_FullStack

**AZMFLIX** — a full-stack movie platform. Users register, sign in, and browse a movie catalog; the API also supports genres, reviews, watchlists, and poster uploads.

- **Backend:** ASP.NET Core Web API on .NET 10, Entity Framework Core, SQL Server
- **Frontend:** Angular 22 (standalone components, lazy-loaded routes), Tailwind CSS 4, Vitest
- **Auth:** JWT bearer tokens with role-based access (`Admin` / user), BCrypt password hashing

## Repository layout

```
AZM_FullStack/
├── MoviePlatform/
│   ├── backendLayer/              # ASP.NET Core API (UserManagementApi.csproj)
│   │   ├── Controllers/           # Users, Movies, Genres, Reviews, Watchlists, Media
│   │   ├── Services/              # Business logic + interfaces, local image storage
│   │   ├── Models/ (+ DTOs/)      # EF entities and DTOs
│   │   ├── Data/                  # AppDbContext, DatabaseSeeder
│   │   ├── Migrations/            # EF Core migrations
│   │   └── Mappings/              # AutoMapper profile
│   ├── backendLayer.Tests/        # xUnit tests for the API
│   ├── Frontend/frontend layer/   # Angular app
│   │   └── src/app/
│   │       ├── core/              # catalog adapters, config, guards, interceptors, services
│   │       └── features/          # auth (login/register), browse
│   └── Movie_schema.png           # Database schema diagram
├── docs/browse-movies/            # Browse feature design audit
└── .squad/                        # squad-kit story intakes and implementation plans
```

## Features

- **Authentication** — register, login, `GET /api/users/me`; JWT interceptor and route guards on the client (`authGuard`, `publicOnlyGuard`).
- **Browse** — authenticated catalog page backed by a catalog adapter layer (real API, fixture, and stub adapters).
- **Movies** — CRUD, search, filter by genre, top-rated; admin-only poster upload/removal.
- **Genres, Reviews, Watchlists** — CRUD endpoints per resource, plus per-movie and per-user lookups.
- **Media** — poster files served from `GET /api/media/posters/{fileName}`.

## Getting started

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (or another SQL Server instance)
- Node.js with npm 10+

### 1. Run the API

```bash
cd MoviePlatform/backendLayer
dotnet ef database update      # apply migrations
dotnet run --launch-profile http
```

The API listens on `http://localhost:5247` (HTTPS profile: `https://localhost:7188`). The default connection string targets `(localdb)\MSSQLLocalDB` and database `UserManagementDb`; change it in `appsettings.json` or via user secrets / environment variables.

> **Note:** `appsettings.json` contains a development JWT secret. Replace `JwtSettings:SecretKey` with your own secret outside local development.

Poster uploads are stored under `App_Data/uploads` (max 5 MB), configurable via the `ImageStorage` section.

### 2. Run the frontend

```bash
cd "MoviePlatform/Frontend/frontend layer"
npm install
npm start
```

The dev server runs on `http://localhost:4200` and proxies `/api` to the API at `http://localhost:5247` (see `proxy.conf.json`).

### 3. Run the tests

```bash
# Backend
dotnet test MoviePlatform/backendLayer.Tests

# Frontend
cd "MoviePlatform/Frontend/frontend layer"
npm test
```

## API overview

| Resource | Base route | Notes |
| --- | --- | --- |
| Users / auth | `/api/users` | `POST login`, `POST register`, `GET me`; list/create/delete are Admin-only |
| Movies | `/api/movies` | CRUD, `POST search`, `GET genre/{genre}`, `GET top-rated`, `PUT/DELETE {id}/poster` (Admin) |
| Genres | `/api/genres` | CRUD, `GET name/{name}` |
| Reviews | `/api/reviews` | CRUD, `GET movie/{movieId}`, `GET user/{userId}` |
| Watchlists | `/api/watchlists` | CRUD, `GET user/{userId}` |
| Media | `/api/media` | `GET posters/{fileName}` |

Swagger UI is available when the API runs in Development. See `MoviePlatform/backendLayer/UserManagementApi.http` for sample requests and `MoviePlatform/Movie_schema.png` for the data model.

## Development workflow

Feature work is planned with [squad-kit](https://github.com/AzmSquad/squad-kit): story intakes live in `.squad/stories/` and implementation plans in `.squad/plans/` (auth, browse-movies, movie-image-attachment, moviegenres). See `.squad/README.md` for details.

### Plan summary

| Feature | Goal |
| --- | --- |
| **auth** | JWT authentication for the user module (login, register, `me`, role-based access). Everything else builds on it. |
| **moviegenres** | Many-to-many `Movie` ↔ `Genre` via an explicit `MovieGenre` join entity (unique `(MovieId, GenreId)`, soft-delete, cascade). Public reads, Admin-only writes, exposed at `/api/moviegenres`. The legacy `Movie.Genre` string stays for now. |
| **browse-movies** | Frontend-only Browse page work, in four steps: (1) show real movies from `GET /api/movies` with loading skeletons, an error + Retry state and a poster fallback; (2) a design foundation: Tailwind tokens, a typed `APP_CONFIG` (brand, nav), and a backend-independent `CatalogAdapter` contract with a fixture adapter; (3) a refactor and audit (`docs/browse-movies/audit.md`) so browse components use only tokens, config and the adapter, with tests for adapter swapping and rating display; (4) MB-02, a dark-navy and cyan visual theme for `/browse` only, with ratings shown as value, scale and source. |
| **movie-image-attachment** | Admin-only poster upload, replace and remove (JPEG/PNG/WebP, max 5 MB) stored behind an `IImageStorage` abstraction and served anonymously from `/api/media/posters/{file}`. The URL is written to the existing `Movie.PosterUrl` (no migration), and Browse cards show the uploaded poster with the title fallback. |

Known follow-up: the existing movie create/update/delete endpoints are still anonymous and should be locked down in a separate story.
