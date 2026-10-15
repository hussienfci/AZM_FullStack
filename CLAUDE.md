# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

AZMFLIX — a movie platform with a .NET 10 Web API backend and an Angular 22 SPA frontend. Everything lives under `MoviePlatform/`. There is no solution (`.sln`) file; each project is targeted individually.

## Commands

Run backend/test commands from each project directory (no `.sln`), and quote the frontend path — the folder name contains a space.

**Backend** (`MoviePlatform/backendLayer`):
```bash
dotnet run                      # starts API on http://localhost:5247 (Development: Swagger at /swagger)
dotnet build
dotnet ef migrations add <Name> # EF Core migration; applied automatically on next startup
```

**Tests** (`MoviePlatform/backendLayer.Tests`, xUnit + EF Core InMemory):
```bash
dotnet test
dotnet test --filter "FullyQualifiedName~LocalImageStorageTests"   # single class
dotnet test --filter "DisplayName~<method name>"                   # single test
```

**Frontend** (`"MoviePlatform/Frontend/frontend layer"`):
```bash
npm install
npm start          # ng serve; proxies /api -> http://localhost:5247 (backend must be running)
npm test           # vitest (NOT karma/jasmine)
npm run build
```

## Architecture

### Backend — layered, response-envelope pattern

Request flow: **Controller → Service (interface in `Services/Interfaces`) → `AppDbContext` (EF Core) → SQL Server LocalDB**. AutoMapper (`Mappings/MappingProfile.cs`) maps entities ↔ DTOs. All wiring is in `Program.cs`: services are `Scoped`, `IImageStorage` is `Singleton`.

- **Response envelope:** services never throw for expected failures. They return `XxxResponseDto` / `XxxListResponseDto` carrying `Success`, `Message`, and `Data`. Controllers branch on `response.Success` and translate a `"not found"` substring in `Message` into a 404 (otherwise 400). Follow this pattern when adding endpoints rather than throwing or returning raw entities.
- **Namespace vs. folder:** the backend folder is `backendLayer` but the project/assembly/namespace is still `UserManagementApi` (`UserManagementApi.csproj`). `MoviePlatform/UserManagementApi/` is a stale leftover (build artifacts only, no source) — ignore it; all backend source is under `backendLayer`.
- **Database:** `AppDbContext.Database.Migrate()` runs on every startup, so migrations auto-apply. In Development, `DatabaseSeeder` also seeds sample movies, genres, users, reviews, and watchlists (idempotent). Seeded users have random password hashes and cannot be used to log in. Connection string targets LocalDB `UserManagementDb`.
- **Auth:** JWT bearer tokens, BCrypt password hashing (`AuthService`). Two roles via the `UserRole` enum (`Admin`, `User`), stored as strings. Admin-only endpoints use `[Authorize(Roles = "Admin")]`. JWT config: `MapInboundClaims = false`, `NameClaimType = "email"`, `RoleClaimType = ClaimTypes.Role`, zero clock skew — preserve these when touching token generation/validation, since claim names must match between `AuthService.GenerateJwtToken` and the validation parameters.
- **Image storage:** `LocalImageStorage` writes poster files under `App_Data/uploads` (configured by the `ImageStorage` section). Poster upload/remove is admin-only, multipart, 5 MB max.

### Frontend — Angular standalone + signals

- Standalone components (no NgModules), bootstrapped in `main.ts` with `appConfig`. Routes in `app.routes.ts` are lazy-loaded via `loadComponent`.
- `core/` holds cross-cutting pieces: `AuthService` (signal-based: `currentUser`, `isAuthenticated`, `displayName`), `jwtInterceptor` (attaches the bearer token, skips `PUBLIC_ENDPOINTS`), and `authGuard` / `publicOnlyGuard`.
- Token persistence: "remember me" → `localStorage` (survives restart), otherwise `sessionStorage` (ends with the tab). The service decodes the JWT `exp` client-side and schedules auto-logout.
- All HTTP goes through `API_BASE_URL = '/api'`, forwarded to the backend by `proxy.conf.json` in dev (avoids CORS / self-signed cert issues). The backend must be running on port 5247.

## Conventions

- **NuGet versions are centralized** in `backendLayer/Directory.Packages.props`. Add `<PackageReference Include="X" />` (no version) to the `.csproj` and the version in `Directory.Packages.props`. The .NET SDK is pinned to 10.0.100 by `global.json`.

## Story workflow

`.claude/.squad/` is a [squad-kit](https://github.com/AzmSquad/squad-kit) workspace. Feature work is scaffolded as intake → plan → implementation files under `stories/` and `plans/`. When asked to implement a squad story, work from the generated `NN-story-*.md` plan file.
