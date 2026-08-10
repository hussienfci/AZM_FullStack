// frontend/src/app/models/movie.model.ts
export interface Movie {
  id: string;
  title: string;
  description?: string;
  synopsis?: string;
  releaseYear?: number;
  durationMinutes?: number;
  posterUrl?: string;
  backdropUrl?: string;
  trailerUrl?: string;
  rating?: number;
  ageRating?: number;
  type: ContentType;
  viewCount: number;
  genres: Genre[];
  cast: CastMember[];
  recentReviews: ReviewSummary[];
}

export interface MovieListItem {
  id: string;
  title: string;
  releaseYear?: number;
  posterUrl?: string;
  rating?: number;
  type: string;
  genreNames: string[];
}

export interface Genre {
  id: string;
  name: string;
  description?: string;
}

export interface CastMember {
  id: string;
  name: string;
  characterName?: string;
  photoUrl?: string;
  role: string;
  order?: number;
}

export type ContentType = 'Movie' | 'TVShow' | 'Documentary' | 'ShortFilm';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}