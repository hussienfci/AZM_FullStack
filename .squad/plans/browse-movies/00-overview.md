# browse-movies — plan overview

Entry point for the **browse-movies** feature. Stories execute in order by their `NN` prefix.

## Stories

| NN | File | Title | Tracker id | Depends on |
|----|------|-------|------------|------------|
| 02 | [02-story-show-real-movies-on-the-browse-page.md](02-story-show-real-movies-on-the-browse-page.md) | Show real movies on the Browse page | — | auth 01 (JWT + `jwtInterceptor`) |

## Dependency notes

Frontend-only. Consumes the existing `GET /api/movies` contract (`MoviesListResponseDto`) unchanged. The backend returns HTTP 200 with `success: false` on server errors, so the client must check `success`.
