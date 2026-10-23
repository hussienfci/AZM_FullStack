/** Where a catalog result came from. Fixture data must never be presented as live catalog data. */
export type DataOrigin = { kind: 'live'; adapter: string } | { kind: 'fixture'; label: string };

export interface PosterCandidate {
  src: string;
  width: number;
}

export interface Poster {
  src: string;
  alt: string;
  candidates?: PosterCandidate[];
}

/** Always shown together with its source and scale. */
export interface Rating {
  value: number;
  scale: number;
  source: string;
}

export type Availability =
  | { status: 'unknown'; region: string }
  | { status: 'unavailable'; region: string }
  | { status: 'available'; region: string; providerIds: string[] };

export interface CatalogMovie {
  /** Stable and unique. */
  id: string;
  title: string;
  releaseYear: number | null;
  poster: Poster | null;
  rating: Rating | null;
  genreIds: string[];
  /** Never undefined. Use status 'unknown' when the adapter cannot tell. */
  availability: Availability;
  origin: DataOrigin;
}

export type CatalogMovieRef = Pick<CatalogMovie, 'id' | 'title'>;

export interface TaxonomyItem {
  id: string;
  label: string;
}

export type CatalogSortKey = 'title' | 'releaseYear' | 'rating' | 'popularity';

export interface CatalogSort {
  key: CatalogSortKey;
  direction: 'asc' | 'desc';
}

export interface CatalogQuery {
  search?: string;
  /** OR within the group. An empty array means no filter. */
  genreIds?: string[];
  /** OR within the group, AND with genreIds. */
  providerIds?: string[];
  /** Inclusive. */
  yearFrom?: number | null;
  /** Inclusive. */
  yearTo?: number | null;
  /** Normalised to a 0–10 scale. */
  minRating?: number | null;
  region: string;
  sort: CatalogSort;
  page: { cursor: string | null; size: number };
}

export interface CatalogPage {
  items: CatalogMovie[];
  /** Null means end of results. */
  nextCursor: string | null;
  hasMore: boolean;
  /** Null when the adapter cannot give an exact count. */
  totalCount: number | null;
  origin: DataOrigin;
}

export type CatalogErrorCode = 'network' | 'timeout' | 'invalid-query' | 'unsupported' | 'not-found' | 'unknown';

export interface CatalogError {
  code: CatalogErrorCode;
  message: string;
  retryable: boolean;
  cause?: unknown;
}

export interface CatalogCapabilities {
  sorts: CatalogSortKey[];
  exactTotal: boolean;
  providerFilter: boolean;
}
