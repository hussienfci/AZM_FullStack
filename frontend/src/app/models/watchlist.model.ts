// frontend/src/app/models/watchlist.model.ts
export interface Watchlist {
  id: string;
  name: string;
  description?: string;
  isDefault: boolean;
  isPublic: boolean;
  createdAt: Date;
  itemCount: number;
  items: WatchlistItem[];
}

export interface WatchlistItem {
  id: string;
  sortOrder: number;
  addedAt: Date;
  notes?: string;
  status: WatchStatus;
  movie: MovieListItem;
}

export type WatchStatus = 'NotStarted' | 'Watching' | 'Completed' | 'Dropped';

export interface CreateWatchlistRequest {
  name: string;
  description?: string;
  isPublic: boolean;
}

export interface AddToWatchlistRequest {
  movieId: string;
  notes?: string;
}

export interface UpdateWatchlistItemRequest {
  status: WatchStatus;
  notes?: string;
}