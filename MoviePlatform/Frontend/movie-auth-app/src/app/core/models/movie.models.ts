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
