import { Injectable } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Observable, defer, of } from 'rxjs';

import { runCatalogAdapterContract } from './catalog-adapter.contract.spec';
import { CATALOG_ADAPTER, CatalogAdapter } from './catalog-adapter';
import { applyCatalogQuery } from './catalog-query';
import { CatalogCapabilities, CatalogMovie, CatalogPage, CatalogQuery, TaxonomyItem } from './catalog.models';
import { formatRating } from './catalog-display';

const STUB_ORIGIN = { kind: 'live', adapter: 'stub' } as const;

const STUB_MOVIES: CatalogMovie[] = Array.from({ length: 12 }, (_, i) => ({
  id: `stub-${i + 1}`,
  title: `Stub Adapter Movie ${i + 1}`,
  releaseYear: 2000 + i,
  poster: null,
  rating: { value: 4, scale: 5, source: 'Stub Source' },
  genreIds: ['stub-genre'],
  availability: { status: 'unknown', region: 'US' },
  origin: STUB_ORIGIN,
}));

/** A second, independent CatalogAdapter. It proves the contract does not depend on the fixture adapter. */
@Injectable()
class StubCatalogAdapter implements CatalogAdapter {
  readonly id = 'stub';
  readonly capabilities: CatalogCapabilities = { sorts: ['title', 'releaseYear', 'rating'], exactTotal: true, providerFilter: false };

  search(query: CatalogQuery): Observable<CatalogPage> {
    return defer(() => {
      const result = applyCatalogQuery(STUB_MOVIES, query);
      return of<CatalogPage>({
        items: result.items,
        nextCursor: result.nextCursor,
        hasMore: result.nextCursor !== null,
        totalCount: result.total,
        origin: STUB_ORIGIN,
      });
    });
  }

  genres(): Observable<TaxonomyItem[]> {
    return of([{ id: 'stub-genre', label: 'Stub Genre' }]);
  }

  providers(_region: string): Observable<TaxonomyItem[]> {
    return of([{ id: 'stub-provider', label: 'Stub Provider' }]);
  }
}

runCatalogAdapterContract(() => new StubCatalogAdapter());

describe('swapping the CatalogAdapter through DI', () => {
  it('resolves the stub with no consumer changes', () => {
    TestBed.configureTestingModule({ providers: [{ provide: CATALOG_ADAPTER, useClass: StubCatalogAdapter }] });
    expect(TestBed.inject(CATALOG_ADAPTER).id).toBe('stub');
  });

  it('keeps rating source and scale visible for stub data', () => {
    const formatted = formatRating(STUB_MOVIES[0].rating);
    expect(formatted?.text).toBe('4/5');
    expect(formatted?.ariaLabel).toContain('Stub Source');
  });
});
