// frontend/src/app/services/movie.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Movie, MovieListItem, PagedResult } from '../models/movie.model';

@Injectable({ providedIn: 'root' })
export class MovieService {
  constructor(private api: ApiService) {}

  getAll(page: number = 1, pageSize: number = 20): Observable<PagedResult<MovieListItem>> {
    return this.api.get<PagedResult<MovieListItem>>(`/movies?page=${page}&pageSize=${pageSize}`);
  }

  getById(id: string): Observable<Movie> {
    return this.api.get<Movie>(`/movies/${id}`);
  }

  getTrending(count: number = 10): Observable<MovieListItem[]> {
    return this.api.get<MovieListItem[]>(`/movies/trending?count=${count}`);
  }

  getNewReleases(count: number = 10): Observable<MovieListItem[]> {
    return this.api.get<MovieListItem[]>(`/movies/new-releases?count=${count}`);
  }

  getByGenre(genreId: string, page: number = 1, pageSize: number = 20): Observable<PagedResult<MovieListItem>> {
    return this.api.get<PagedResult<MovieListItem>>(`/movies/genre/${genreId}?page=${page}&pageSize=${pageSize}`);
  }

  getSimilar(movieId: string, count: number = 6): Observable<MovieListItem[]> {
    return this.api.get<MovieListItem[]>(`/movies/similar/${movieId}?count=${count}`);
  }

  create(movie: any): Observable<Movie> {
    return this.api.post<Movie>('/movies', movie);
  }

  update(id: string, movie: any): Observable<Movie> {
    return this.api.put<Movie>(`/movies/${id}`, movie);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`/movies/${id}`);
  }
}