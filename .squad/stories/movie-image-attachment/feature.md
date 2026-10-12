# Feature: Movie poster image attachments

## Goal

Allow a movie to have an uploaded poster image attached to its `Movie` record, so the browse and movie detail views can display the correct image for that film.

## Current state

`Movie` already has a nullable `PosterUrl` string (maximum 500 characters), and `MovieDto` exposes it. The API currently accepts a URL as part of movie create/update; it does not accept or store an uploaded image file.

## Proposed behavior

- An authorized movie manager can upload one poster image for a movie using a multipart form request.
- The image is stored through an image-storage service; the database stores only its stable relative URL or storage key in `Movie.PosterUrl`.
- The poster URL is returned by the existing movie DTO and can be rendered by the frontend.
- Replacing a poster updates the movie’s URL and removes the previous managed file when safe to do so.
- Removing a poster clears `PosterUrl`; movies without an image remain valid and use the frontend fallback.

## Acceptance criteria

1. A poster upload is associated with exactly one existing movie.
2. Only supported image formats (JPEG, PNG, or WebP) are accepted, with a documented maximum file size.
3. Invalid files and unknown movie IDs return clear client errors and do not change the existing poster.
4. The stored image can be fetched through a stable URL, and that URL is included in `MovieDto`.
5. Replacing/removing a managed poster does not delete files that are not owned by this feature.
6. Existing create/update requests that provide `PosterUrl` continue to work during migration, or a documented compatibility path is provided.
7. The frontend movie card displays the attached poster and retains its fallback when no poster is set or loading fails.

## Implementation notes

- Keep binary data out of the relational database. Use a storage abstraction so local development storage can later be replaced with object storage without changing the movie API contract.
- Persist a relative URL/storage key, not a machine-specific absolute filesystem path.
- Decide authorization policy and storage provider before implementation; current movie write endpoints should be reviewed before exposing uploads.
- Add migration/API/UI coverage for upload, replacement, removal, invalid input, and missing-poster behavior.

## Out of scope

- Multiple gallery images per movie.
- Image editing, cropping, or transformation.
- Importing posters from a third-party movie catalog.
