// frontend/src/app/models/search.model.ts
export interface SearchRequest {
  query: string;
  genreIds?: string[];
  minYear?: number;
  maxYear?: number;
  minRating?: number;
  type?: ContentType;
  sortBy?: string;
  page: number;
  pageSize: number;
}

export interface SearchResponse {
  items: MovieListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface SuggestionsResponse {
  suggestions: string[];
}

type ContentType = 'Movie' | 'TVShow' | 'Documentary' | 'ShortFilm';