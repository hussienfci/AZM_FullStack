import { CatalogError, CatalogMovie, CatalogQuery, Rating } from './catalog.models';

export const MIN_PAGE_SIZE = 1;
export const MAX_PAGE_SIZE = 100;

export interface CatalogQueryResult {
  items: CatalogMovie[];
  /** Number of matches before pagination. */
  total: number;
  /** Null means end of results. */
  nextCursor: string | null;
}

type SortValue = string | number | null;

/**
 * Pure query engine shared by catalog adapters.
 * Filters are applied in this order: search, genres, providers, years, minimum rating.
 * Sorting is deterministic: primary key, then title, then id. Null values sort last in both directions.
 * The cursor is the stringified offset.
 *
 * `popularity` is supplied by the adapter, because popularity is not part of CatalogMovie.
 */
export function applyCatalogQuery(
  movies: readonly CatalogMovie[],
  query: CatalogQuery,
  popularity: ReadonlyMap<string, number> = new Map(),
): CatalogQueryResult {
  if (query.yearFrom != null && query.yearTo != null && query.yearFrom > query.yearTo) {
    throw invalidQuery('yearFrom must be less than or equal to yearTo');
  }

  const offset = parseCursor(query.page.cursor);
  const size = clampPageSize(query.page.size);
  const needle = normalizeText(query.search ?? '');
  const genreIds = new Set(query.genreIds ?? []);
  const providerIds = new Set(query.providerIds ?? []);
  const direction = query.sort.direction === 'asc' ? 1 : -1;

  const matches = movies
    .filter((movie) => matchesQuery(movie, query, needle, genreIds, providerIds))
    .sort(
      (a, b) =>
        compareValues(
          sortValueOf(a, query.sort.key, popularity),
          sortValueOf(b, query.sort.key, popularity),
          direction,
        ) ||
        a.title.localeCompare(b.title, 'en') ||
        a.id.localeCompare(b.id),
    );

  const end = offset + size;
  return {
    items: matches.slice(offset, end),
    total: matches.length,
    nextCursor: end < matches.length ? String(end) : null,
  };
}

/** Maps a rating onto a 0–10 scale so ratings with different scales can be compared. */
export function normalizedRating(rating: Rating): number {
  return (rating.value / rating.scale) * 10;
}

function matchesQuery(
  movie: CatalogMovie,
  query: CatalogQuery,
  needle: string,
  genreIds: ReadonlySet<string>,
  providerIds: ReadonlySet<string>,
): boolean {
  if (needle && !normalizeText(movie.title).includes(needle)) return false;

  if (genreIds.size > 0 && !movie.genreIds.some((id) => genreIds.has(id))) return false;

  if (providerIds.size > 0) {
    // Unknown availability is excluded, not treated as unavailable.
    if (movie.availability.status !== 'available') return false;
    if (!movie.availability.providerIds.some((id) => providerIds.has(id))) return false;
  }

  if (query.yearFrom != null || query.yearTo != null) {
    if (movie.releaseYear === null) return false;
    if (query.yearFrom != null && movie.releaseYear < query.yearFrom) return false;
    if (query.yearTo != null && movie.releaseYear > query.yearTo) return false;
  }

  if (query.minRating != null) {
    if (!movie.rating) return false;
    if (normalizedRating(movie.rating) < query.minRating) return false;
  }

  return true;
}

function sortValueOf(movie: CatalogMovie, key: CatalogQuery['sort']['key'], popularity: ReadonlyMap<string, number>): SortValue {
  switch (key) {
    case 'title':
      return movie.title;
    case 'releaseYear':
      return movie.releaseYear;
    case 'rating':
      return movie.rating ? normalizedRating(movie.rating) : null;
    case 'popularity':
      return popularity.get(movie.id) ?? null;
  }
}

function compareValues(a: SortValue, b: SortValue, direction: 1 | -1): number {
  // Nulls sort last whichever direction is chosen.
  if (a === null && b === null) return 0;
  if (a === null) return 1;
  if (b === null) return -1;
  const result = typeof a === 'number' && typeof b === 'number' ? a - b : String(a).localeCompare(String(b), 'en');
  return direction * result;
}

/** Case-insensitive, with diacritics removed on both sides. */
function normalizeText(value: string): string {
  return value.normalize('NFKD').replace(/\p{M}/gu, '').trim().toLowerCase();
}

function parseCursor(cursor: string | null): number {
  if (cursor === null) return 0;
  if (!/^\d+$/.test(cursor)) throw invalidQuery(`Invalid cursor "${cursor}"`);
  return Number(cursor);
}

function clampPageSize(size: number): number {
  return Number.isFinite(size) ? Math.min(MAX_PAGE_SIZE, Math.max(MIN_PAGE_SIZE, Math.trunc(size))) : MIN_PAGE_SIZE;
}

function invalidQuery(message: string): CatalogError {
  return { code: 'invalid-query', message, retryable: false };
}
