## Feature
- **Feature name (display):** Browse Movies
- **Feature slug:** `browse-movies`

## Title
Show real movies on the Browse page

## Description
The Angular app at `MoviePlatform/movie-auth-app` has a protected `/browse` page
(`src/app/features/browse/browse.component.ts`) that currently shows placeholder
gradient cards. Replace them with real movies from the backend
`GET /api/movies?page=1&pageSize=20` (MoviesController in
`MoviePlatform/UserManagementApi`). Requests go through the existing
`jwtInterceptor`, so no auth code changes are needed.

## Acceptance criteria
1. A new `MovieService` in `src/app/core/services/` calls `GET /api/movies`
   and exposes the result as Angular signals (movies, loading, error).
2. TypeScript models in `src/app/core/models/movie.models.ts` mirror
   `MovieDto` and `MoviesListResponseDto` from the backend.
3. `/browse` shows a horizontal row of movie cards: poster (`posterUrl`,
   with a dark fallback when empty), title, release year, and rating.
4. Loading state shows skeleton cards; errors show a message with a Retry button.
5. Clicking a card does nothing yet (out of scope).
6. Keep the existing dark theme (`bg-surface`, `text-brand`) and Tailwind classes.
7. Unit test for `MovieService` using `HttpTestingController`.
8. `npm run build` and `npm test` pass.

## Out of scope
- Movie details page, search, pagination UI, backend changes.