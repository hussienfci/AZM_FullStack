// frontend/src/app/services/review.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Review, ReviewSummary, CreateReviewRequest, UpdateReviewRequest } from '../models/review.model';
import { PagedResult } from '../models/movie.model';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  constructor(private api: ApiService) {}

  getByMovie(movieId: string, page: number = 1, pageSize: number = 20): Observable<PagedResult<Review>> {
    return this.api.get<PagedResult<Review>>(`/reviews/movie/${movieId}?page=${page}&pageSize=${pageSize}`);
  }

  getSummary(movieId: string): Observable<ReviewSummary> {
    return this.api.get<ReviewSummary>(`/reviews/movie/${movieId}/summary`);
  }

  getMyReviews(page: number = 1, pageSize: number = 20): Observable<PagedResult<Review>> {
    return this.api.get<PagedResult<Review>>(`/reviews/my-reviews?page=${page}&pageSize=${pageSize}`);
  }

  create(request: CreateReviewRequest): Observable<Review> {
    return this.api.post<Review>('/reviews', request);
  }

  update(id: string, request: UpdateReviewRequest): Observable<Review> {
    return this.api.put<Review>(`/reviews/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`/reviews/${id}`);
  }
}