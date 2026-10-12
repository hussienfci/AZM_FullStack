# movie-image-attachment — plan overview

Entry point for the **movie-image-attachment** feature ([intake](../../stories/movie-image-attachment/feature.md)). Stories execute in order by their `NN` prefix.

## Stories

| NN | File | Title | Tracker id | Depends on |
|----|------|-------|------------|------------|
| 03 | [03-story-poster-upload-api.md](03-story-poster-upload-api.md) | Poster upload, replace and remove API | — | — |
| 04 | [04-story-poster-display-on-cards.md](04-story-poster-display-on-cards.md) | Show uploaded posters on Browse movie cards | — | 03 (manual check only), browse-movies 02 |

## Shared contract

- `Movie.PosterUrl` is unchanged (`nvarchar(500)`, nullable). **No migration.**
- A managed poster URL is relative: `/api/media/posters/{32-hex}.{jpg|png|webp}`. It is served anonymously by `GET /api/media/posters/{file}`, and the Angular dev proxy covers it through `/api`.
- Only URLs matching that exact pattern are ever deleted. External `http(s)` URLs stay valid and are never touched.
- Uploading and removing a poster need the `Admin` role. Files are capped at 5 MB, and only JPEG, PNG and WebP are accepted (detected from the file's magic bytes).

## Acceptance criteria → story

| AC | Covered by |
|----|------------|
| 1 One poster per existing movie | 03 |
| 2 JPEG/PNG/WebP, documented max size | 03 |
| 3 Clear errors, no change on failure | 03 |
| 4 Stable URL in `MovieDto` | 03 |
| 5 Never delete non-owned files | 03 |
| 6 `PosterUrl` create/update compatibility | 03 |
| 7 Card shows poster, keeps fallback | 04 |

## Dependency notes

The existing movie write endpoints (`POST/PUT/DELETE/PATCH /api/movies`) are **anonymous** today. This feature protects only its own new endpoints. Locking down the existing ones is recommended as a separate follow-up story.
