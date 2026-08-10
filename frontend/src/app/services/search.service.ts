// frontend/src/app/services/search.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { SearchRequest, SearchResponse, SuggestionsResponse } from '../models/search.model';
import { HttpParams } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class SearchService {
  constructor(private api: ApiService) {}

  search(request: SearchRequest): Observable<SearchResponse> {
    let params = new HttpParams()
      .set('query', request.query)
      .set('page', request.page.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.genreIds?.length) {
      request.genreIds.forEach(id => {
        params = params.append('genreIds', id);
      });
    }
    if (request.minYear) params = params.set('minYear', request.minYear.toString());
    if (request.maxYear) params = params.set('maxYear', request.maxYear.toString());
    if (request.minRating) params = params.set('minRating', request.minRating.toString());
    if (request.type) params = params.set('type', request.type);
    if (request.sortBy) params = params.set('sortBy', request.sortBy);

    return this.api.get<SearchResponse>('/search', params);
  }

  getSuggestions(query: string): Observable<SuggestionsResponse> {
    return this.api.get<SuggestionsResponse>(`/search/suggestions?query=${encodeURIComponent(query)}`);
  }
}