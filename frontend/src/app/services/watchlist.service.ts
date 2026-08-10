// frontend/src/app/services/watchlist.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Watchlist, CreateWatchlistRequest, AddToWatchlistRequest, UpdateWatchlistItemRequest } from '../models/watchlist.model';

@Injectable({ providedIn: 'root' })
export class WatchlistService {
  constructor(private api: ApiService) {}

  getMyWatchlists(): Observable<Watchlist[]> {
    return this.api.get<Watchlist[]>('/watchlists');
  }

  getDefaultWatchlist(): Observable<Watchlist> {
    return this.api.get<Watchlist>('/watchlists/default');
  }

  getById(id: string): Observable<Watchlist> {
    return this.api.get<Watchlist>(`/watchlists/${id}`);
  }

  create(request: CreateWatchlistRequest): Observable<Watchlist> {
    return this.api.post<Watchlist>('/watchlists', request);
  }

  update(id: string, request: CreateWatchlistRequest): Observable<Watchlist> {
    return this.api.put<Watchlist>(`/watchlists/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`/watchlists/${id}`);
  }

  addItem(watchlistId: string, request: AddToWatchlistRequest): Observable<any> {
    return this.api.post<any>(`/watchlists/${watchlistId}/items`, request);
  }

  removeItem(watchlistId: string, movieId: string): Observable<void> {
    return this.api.delete<void>(`/watchlists/${watchlistId}/items/${movieId}`);
  }

  updateItemStatus(itemId: string, request: UpdateWatchlistItemRequest): Observable<void> {
    return this.api.patch<void>(`/watchlists/items/${itemId}`, request);
  }

  isInWatchlist(movieId: string): Observable<boolean> {
    return this.api.get<boolean>(`/watchlists/check/${movieId}`);
  }
}