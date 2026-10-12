<!-- squad-kit: story-id=show-real-movies-on-the-browse-page, feature=browse-movies, title="Show real movies on the Browse page", planner=claude-opus-5-5, generated=2026-10-11 -->

# Plan: Show real movies on the Browse page

## 1. Overview

Replace the placeholder gradient cards on the protected `/browse` page with real movies loaded from
`GET /api/movies?page=1&pageSize=20`. Add typed models, a signal-based `MovieService`, a horizontal row
of movie cards with loading skeletons and an error + Retry state, and a unit test for the service.

**Feature:** browse-movies
**Story:** show-real-movies-on-the-browse-page
**Scope:** Frontend only. No auth changes.

> **Do not modify or duplicate the backend.** `GET /api/movies` already exists in
> `MoviePlatform/UserManagementApi` and returns everything this story needs. Do not add, copy or
> change any controller, service, DTO, migration or other file under `MoviePlatform/UserManagementApi`.
> The backend files below are listed **for reference only**, so the frontend models match the existing contract.
**Depends on:** auth story (JWT login + `jwtInterceptor`), already merged.

> **Path correction:** the intake says `MoviePlatform/movie-auth-app`. The app actually lives at
> **`MoviePlatform/Frontend/movie-auth-app`**. All paths below are relative to that folder
> unless they start with `MoviePlatform/`.

---

## 2. Context & Current State

### 2.1 Stack (from `package.json`)
- Angular 22 (standalone components, signals, `@if`/`@for` control flow, `ChangeDetectionStrategy.OnPush`)
- Tailwind CSS 4 via `@tailwindcss/postcss`; theme tokens are in `src/styles.css` (`bg-surface`, `text-brand`, `bg-brand`, `btn-ghost`, `spinner`, `animate-fade-in`)
- Tests: `ng test` → `@angular/build:unit-test` with **Vitest** (globals: `describe`, `it`, `expect`, `vi`). Spec files are `*.spec.ts` next to the code.
- HTTP: `provideHttpClient(withFetch(), withInterceptors([jwtInterceptor]))` in `src/app/app.config.ts`. The interceptor adds `Authorization: Bearer …` to every `/api/...` request and logs out on 401. **Do not touch it.**
- API base: `API_BASE_URL = '/api'` in `src/app/core/config/api.config.ts`; `proxy.conf.json` forwards `/api` to `http://localhost:5247`.

### 2.2 Backend contract (read-only, do not change)
`MoviePlatform/UserManagementApi/Controllers/MoviesController.cs`:
```
GET /api/movies?page=1&pageSize=20  →  200 MoviesListResponseDto
```
ASP.NET serializes to camelCase JSON. `MoviePlatform/UserManagementApi/Models/DTOs/MovieDto.cs`:
```csharp
public class MovieDto {
    int Id; string Title; string? Description; string Genre; int? DurationMinutes;
    DateTime? ReleaseDate; string? Director; string? PosterUrl; string? TrailerUrl;
    decimal? Rating; string? Language; string? Country; bool IsActive; DateTime CreatedAt;
}
public class MoviesListResponseDto {
    bool Success; string Message; List<MovieDto> Data; int TotalCount;
}
```
**Important gotcha:** `MovieService.GetAllAsync` catches exceptions and returns **HTTP 200 with
`success: false`** and an empty `data` array. The frontend must treat `success === false` as an error,
not just non-2xx responses.

### 2.3 Existing conventions to copy
- Models: `src/app/core/models/auth.models.ts` — interfaces with a `/** Mirrors UserManagementApi.Models.DTOs.X */` doc comment, nullable C# fields typed `?: T | null`, `DateTime` typed as `string`.
- Services: `src/app/core/services/auth.service.ts` — `@Injectable({ providedIn: 'root' })`, `inject(HttpClient)`, private writable `signal`s exposed via `.asReadonly()`, URLs built with `` `${API_BASE_URL}/...` ``, error text via `HttpErrorResponse` (`status === 0` → "Cannot reach the server. Is the API running?").
- Specs: `src/app/core/interceptors/jwt.interceptor.spec.ts` — `TestBed` + `provideHttpClient()` + `provideHttpClientTesting()`, `HttpTestingController.expectOne(...)`, `afterEach(() => httpTesting.verify())`.

### 2.4 Current Browse page
`src/app/features/browse/browse.component.ts` (inline template):
- Header (logo + display name + Sign out) — **keep unchanged**.
- Hero section with the `GET /api/users/me` profile panel — **keep unchanged**.
- A `ROWS` constant with two rows of gradient `<div>`s rendered with `@for (row of rows ...)` — **this is what gets replaced**.

---

## 3. Architecture Decisions

1. **State lives in `MovieService` as signals** (`movies`, `loading`, `error`) per AC 1. The component reads them; it does not subscribe itself.
2. **`load()` method, not constructor fetch.** The component calls `movieService.load()` in `ngOnInit`; Retry calls the same method. This keeps the service test simple and avoids fetching on injection.
3. **Error mapping:** non-2xx / network → message string; `success: false` → `res.message || fallback`. On error, `movies` is set to `[]`.
4. **One row** titled "Latest Movies" (backend orders by `CreatedAt` desc). Remove the `ROWS` constant entirely.
5. **Card layout:** portrait poster (`aspect-[2/3]`, `w-40 sm:w-44`), title (truncated), `year · ★ rating`. Missing year or rating is simply omitted.
6. **Poster fallback:** when `posterUrl` is null/empty/whitespace **or the image fails to load**, render a dark `bg-neutral-800` box with the title centered. Track failed ids in a `signal<Set<number>>` in the component.
7. **Cards are not clickable** (AC 5): render as `<article>`, no `(click)`, no `routerLink`, no `cursor-pointer`. Keep the existing `hover:scale-105` transition for feel.
8. **No pagination UI** — hard-code `page=1&pageSize=20` as service defaults.

---

## 4. Implementation Steps (In Order)

### Step 1: Create models
**New file:** `src/app/core/models/movie.models.ts`

```ts
/** Mirrors UserManagementApi.Models.DTOs.MovieDto */
export interface Movie {
  id: number;
  title: string;
  description?: string | null;
  genre: string;
  durationMinutes?: number | null;
  releaseDate?: string | null;
  director?: string | null;
  posterUrl?: string | null;
  trailerUrl?: string | null;
  rating?: number | null;
  language?: string | null;
  country?: string | null;
  isActive: boolean;
  createdAt: string;
}

/** Mirrors UserManagementApi.Models.DTOs.MoviesListResponseDto */
export interface MoviesListResponse {
  success: boolean;
  message: string;
  data: Movie[];
  totalCount: number;
}
```

### Step 2: Create the service
**New file:** `src/app/core/services/movie.service.ts`

Requirements:
- `@Injectable({ providedIn: 'root' })`, `private readonly http = inject(HttpClient);`
- Private writable signals: `_movies = signal<Movie[]>([])`, `_loading = signal(false)`, `_error = signal<string | null>(null)`.
- Public readonly: `movies`, `loading`, `error` (via `.asReadonly()`).
- `load(page = 1, pageSize = 20): void`
  1. `_loading.set(true); _error.set(null);`
  2. `this.http.get<MoviesListResponse>(`${API_BASE_URL}/movies`, { params: { page, pageSize } })`
  3. `next`: if `res.success` → `_movies.set(res.data ?? [])`; else `_movies.set([])` and `_error.set(res.message || 'Could not load movies.')`. Then `_loading.set(false)`.
  4. `error`: `_movies.set([])`, `_error.set(this.toErrorMessage(err))`, `_loading.set(false)`.
- `private toErrorMessage(err: unknown): string` — `HttpErrorResponse` with `status === 0` → `'Cannot reach the server. Is the API running?'`; otherwise `'Could not load movies.'`.

Imports: `HttpClient, HttpErrorResponse` from `@angular/common/http`; `inject, Injectable, signal` from `@angular/core`; `API_BASE_URL` from `../config/api.config`; types from `../models/movie.models`.

Note: with `params: { page, pageSize }` Angular serializes the URL as `/api/movies?page=1&pageSize=20`.

### Step 3: Unit test the service
**New file:** `src/app/core/services/movie.service.spec.ts`

Setup (mirror `jwt.interceptor.spec.ts`, but **without** the interceptor):
```ts
TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
service = TestBed.inject(MovieService);
httpTesting = TestBed.inject(HttpTestingController);
afterEach(() => httpTesting.verify());
```
Match requests with
`httpTesting.expectOne((r) => r.url === '/api/movies' && r.params.get('page') === '1' && r.params.get('pageSize') === '20')`.

Test cases:
1. **requests page 1 / pageSize 20 with GET** — assert `req.request.method === 'GET'` and params; flush a success body.
2. **sets `loading` true while in flight, false after** — `service.load()` → `expect(service.loading()).toBe(true)` → flush → `false`.
3. **populates `movies` on success** — flush `{ success: true, message: 'ok', data: [movieA, movieB], totalCount: 2 }`; expect `movies()` toEqual `[movieA, movieB]` and `error()` toBeNull.
4. **`success: false` body is an error** — flush `{ success: false, message: 'An error occurred while retrieving movies', data: [], totalCount: 0 }` (status 200); expect `error()` to be that message, `movies()` `[]`, `loading()` false.
5. **HTTP 500 sets a generic error** — `req.flush(null, { status: 500, statusText: 'Server Error' })`; expect `error()` `'Could not load movies.'`.
6. **network failure** — `req.error(new ProgressEvent('error'))` (status 0); expect the "Cannot reach the server" message.
7. **retry clears the previous error** — fail once, call `load()` again, assert `error()` is `null` while the second request is pending, then flush success.

Build a `movieA` fixture object of type `Movie` at the top of the spec (all required fields filled).

### Step 4: Update the Browse component
**Modified file:** `src/app/features/browse/browse.component.ts`

TypeScript changes:
- Delete the `ROWS` constant and the `rows` field.
- `import { MovieService } from '../../core/services/movie.service';` and `import { Movie } from '../../core/models/movie.models';`
- Add fields:
  ```ts
  protected readonly movieService = inject(MovieService);
  protected readonly skeletons = Array.from({ length: 8 }, (_, i) => i);
  private readonly brokenPosters = signal<ReadonlySet<number>>(new Set());
  ```
- In `ngOnInit`, after the existing `/users/me` call, add `this.movieService.load();`
- Helpers:
  ```ts
  protected releaseYear(movie: Movie): number | null {
    return movie.releaseDate ? new Date(movie.releaseDate).getFullYear() : null;
  }
  protected hasPoster(movie: Movie): boolean {
    return !!movie.posterUrl?.trim() && !this.brokenPosters().has(movie.id);
  }
  protected onPosterError(id: number): void {
    this.brokenPosters.update((set) => new Set(set).add(id));
  }
  ```
- Update the doc comment above `@Component` to mention it also loads movies from `GET /api/movies`.

Template: replace the whole `@for (row of rows; ...) { ... }` block with:
```html
<section class="px-6 pb-10 sm:px-12" aria-labelledby="latest-heading">
  <h2 id="latest-heading" class="mb-3 text-lg font-semibold">Latest Movies</h2>

  @if (movieService.loading()) {
    <div class="flex gap-3 overflow-x-auto pb-2" aria-busy="true" aria-label="Loading movies">
      @for (i of skeletons; track i) {
        <div class="w-40 shrink-0 sm:w-44">
          <div class="aspect-[2/3] animate-pulse rounded-md bg-neutral-800"></div>
          <div class="mt-2 h-4 w-3/4 animate-pulse rounded bg-neutral-800"></div>
          <div class="mt-1 h-3 w-1/2 animate-pulse rounded bg-neutral-800"></div>
        </div>
      }
    </div>
  } @else if (movieService.error(); as error) {
    <div class="flex items-center gap-4 rounded-md bg-black/60 p-4 text-sm ring-1 ring-white/10" role="alert">
      <p class="text-red-400">{{ error }}</p>
      <button type="button" class="btn-ghost" (click)="movieService.load()">Retry</button>
    </div>
  } @else if (movieService.movies().length === 0) {
    <p class="text-sm text-neutral-400">No movies available yet.</p>
  } @else {
    <div class="flex gap-3 overflow-x-auto pb-2">
      @for (movie of movieService.movies(); track movie.id) {
        <article class="w-40 shrink-0 transition-transform duration-300 hover:scale-105 sm:w-44">
          @if (hasPoster(movie)) {
            <img
              [src]="movie.posterUrl"
              [alt]="movie.title + ' poster'"
              loading="lazy"
              (error)="onPosterError(movie.id)"
              class="aspect-[2/3] w-full rounded-md bg-neutral-800 object-cover"
            />
          } @else {
            <div class="flex aspect-[2/3] w-full items-center justify-center rounded-md bg-neutral-800 p-3 text-center text-sm font-semibold text-neutral-400 ring-1 ring-white/10">
              {{ movie.title }}
            </div>
          }
          <h3 class="mt-2 truncate text-sm font-semibold" [title]="movie.title">{{ movie.title }}</h3>
          <p class="text-xs text-neutral-400">
            @if (releaseYear(movie); as year) { <span>{{ year }}</span> }
            @if (releaseYear(movie) && movie.rating != null) { <span> · </span> }
            @if (movie.rating != null) { <span class="text-amber-400">★ {{ movie.rating }}</span> }
          </p>
        </article>
      }
    </div>
  }
</section>
```
Notes:
- Do **not** add `NgOptimizedImage` (poster URLs are arbitrary external hosts with unknown dimensions).
- The `HttpClient` injection stays — it is still used for `/users/me`.
- Empty-list state is not in the AC but prevents a blank section; keep it.

### Step 5: Build & test
From `MoviePlatform/Frontend/movie-auth-app`:
```bash
npm run build
npm test -- --watch=false
```
Both must pass with zero errors. Fix any strict-template type errors (e.g. `movie.rating != null` comparisons) rather than loosening `tsconfig`.

---

## 5. Testing Checklist

### 5.1 Automated
- [ ] `npm test` — all existing specs plus the 7 new `MovieService` specs pass.
- [ ] `npm run build` — succeeds with no errors.

### 5.2 Manual (API on `http://localhost:5247`, `npm start`)
1. Sign in → `/browse` shows 8 grey skeleton cards briefly, then the "Latest Movies" row.
2. DevTools → Network: request is `GET /api/movies?page=1&pageSize=20` with an `Authorization: Bearer …` header.
3. A movie with an empty `posterUrl` shows the dark fallback with its title; a movie with a broken URL also falls back.
4. Each card shows title, release year, and `★ rating` (missing values are omitted cleanly).
5. Clicking a card does nothing (no navigation, no console error).
6. Stop the API → reload → red "Cannot reach the server…" message with a **Retry** button. Start the API → click Retry → movies appear.
7. Row scrolls horizontally on narrow viewports; header and hero/profile panel look unchanged.

---

## 6. Files Summary

### New Files (3)
| File | Purpose |
|------|---------|
| `src/app/core/models/movie.models.ts` | `Movie`, `MoviesListResponse` interfaces mirroring backend DTOs |
| `src/app/core/services/movie.service.ts` | Fetches `/api/movies`, exposes `movies` / `loading` / `error` signals + `load()` |
| `src/app/core/services/movie.service.spec.ts` | `HttpTestingController` unit tests for the service |

### Modified Files (1)
| File | Changes |
|------|---------|
| `src/app/features/browse/browse.component.ts` | Remove `ROWS`; add `MovieService`, skeleton / error+Retry / empty / cards row, poster fallback helpers |

### Not touched
`jwt.interceptor.ts`, `auth.service.ts`, `app.config.ts`, `app.routes.ts`, `styles.css`, all backend files.

---

## 7. Rollback Plan
1. `git checkout -- src/app/features/browse/browse.component.ts`
2. Delete `movie.models.ts`, `movie.service.ts`, `movie.service.spec.ts`.
No data, config, or backend changes to revert.

---

## 8. Out of Scope / Future Enhancements
- Movie details page and card click behaviour
- Search, genre rows (`GET /api/movies/genre/{genre}`), top-rated row (`GET /api/movies/top-rated`)
- Pagination / infinite scroll using `totalCount`
- Any backend work (the existing `success: false` on 200 behaviour is handled on the client)
