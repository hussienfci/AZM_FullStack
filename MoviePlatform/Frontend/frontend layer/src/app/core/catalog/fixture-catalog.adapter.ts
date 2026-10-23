import { Injectable } from '@angular/core';
import { defer, Observable, of } from 'rxjs';

import { CatalogAdapter } from './catalog-adapter';
import { applyCatalogQuery } from './catalog-query';
import {
  CatalogCapabilities,
  CatalogError,
  CatalogMovie,
  CatalogPage,
  CatalogQuery,
  TaxonomyItem,
} from './catalog.models';
import {
  FIXTURE_GENRES,
  FIXTURE_MOVIES,
  FIXTURE_ORIGIN,
  FIXTURE_POPULARITY,
  FIXTURE_PROVIDERS,
} from './fixtures/catalog.fixtures';

/** Adapter backed by fixture data. It implements the full query contract, so UI and tests can run without a backend. */
@Injectable()
export class FixtureCatalogAdapter implements CatalogAdapter {
  readonly id = 'fixture';
  readonly capabilities: CatalogCapabilities = {
    sorts: ['title', 'releaseYear', 'rating', 'popularity'],
    exactTotal: true,
    providerFilter: true,
  };

  search(query: CatalogQuery): Observable<CatalogPage> {
    // defer turns a thrown CatalogError into an error notification.
    return defer(() => {
      if (!this.capabilities.sorts.includes(query.sort.key)) {
        throw unsupportedSort(query.sort.key);
      }
      const result = applyCatalogQuery(
        FIXTURE_MOVIES.map((movie) => forRegion(movie, query.region)),
        query,
        FIXTURE_POPULARITY,
      );
      return of<CatalogPage>({
        items: result.items,
        nextCursor: result.nextCursor,
        hasMore: result.nextCursor !== null,
        totalCount: result.total,
        origin: FIXTURE_ORIGIN,
      });
    });
  }

  genres(): Observable<TaxonomyItem[]> {
    return of(FIXTURE_GENRES);
  }

  providers(_region: string): Observable<TaxonomyItem[]> {
    return of(FIXTURE_PROVIDERS);
  }
}

/** Availability is only known for the region it was recorded for. Other regions become 'unknown', never 'unavailable'. */
function forRegion(movie: CatalogMovie, region: string): CatalogMovie {
  if (movie.availability.region === region) return movie;
  return { ...movie, availability: { status: 'unknown', region } };
}

function unsupportedSort(key: string): CatalogError {
  return { code: 'unsupported', message: `Sort "${key}" is not supported by the fixture adapter`, retryable: false };
}
