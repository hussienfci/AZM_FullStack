---
id: MOVIE-001
feature: moviegenres
title: Implement Movie-Genre Many-to-Many Relationship
status: planned
priority: high
created: 2026-08-23
---

## Context

The Movie Platform API has separate Movie and Genre entities. We need to establish a many-to-many relationship so each movie can have multiple genres and each genre can belong to multiple movies.

## Requirements

1. Join entity `MovieGenre` linking `Movie` and `Genre`
2. Prevent duplicate genre assignments to the same movie
3. Get all genres for a movie
4. Get all movies for a genre
5. Admin-only CRUD for managing links
6. Public read access for movie/genre listings

## Acceptance Criteria

- [ ] Can assign multiple genres to a movie
- [ ] Can view all genres for a specific movie
- [ ] Can view all movies for a specific genre
- [ ] Duplicate assignments are prevented
- [ ] Admin can create/delete links
- [ ] Database migration creates `MovieGenres` table