import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';

import { API_BASE_URL } from '../config/api.config';
import { Movie, MoviesListResponse } from '../models/movie.models';

@Injectable({ providedIn: 'root' })
export class MovieService {
  private readonly http = inject(HttpClient);

  private readonly _movies = signal<Movie[]>([]);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  readonly movies = this._movies.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();

  load(page = 1, pageSize = 20): void {
    this._loading.set(true);
    this._error.set(null);

    this.http.get<MoviesListResponse>(`${API_BASE_URL}/movies`, { params: { page, pageSize } }).subscribe({
      next: (res) => {
        // The API reports some failures as HTTP 200 with success: false.
        if (res.success) {
          this._movies.set(res.data ?? []);
        } else {
          this._movies.set([]);
          this._error.set(res.message || 'Could not load movies.');
        }
        this._loading.set(false);
      },
      error: (err) => {
        this._movies.set([]);
        this._error.set(this.toErrorMessage(err));
        this._loading.set(false);
      },
    });
  }

  private toErrorMessage(err: unknown): string {
    if (err instanceof HttpErrorResponse && err.status === 0) {
      return 'Cannot reach the server. Is the API running?';
    }
    return 'Could not load movies.';
  }
}
