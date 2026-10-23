import { firstValueFrom } from 'rxjs';

import { isCatalogError } from './catalog-adapter';
import { CatalogQuery } from './catalog.models';
import { FixtureCatalogAdapter } from './fixture-catalog.adapter';
import { FIXTURE_GENRES, FIXTURE_LABEL, FIXTURE_MOVIES, FIXTURE_PROVIDERS } from './fixtures/catalog.fixtures';

const query = (overrides: Partial<CatalogQuery> = {}): CatalogQuery => ({
  region: 'US',
  sort: { key: 'title', direction: 'asc' },
  page: { cursor: null, size: 100 },
  ...overrides,
});

describe('FixtureCatalogAdapter', () => {
  let adapter: FixtureCatalogAdapter;

  beforeEach(() => {
    adapter = new FixtureCatalogAdapter();
  });

  it('marks every item and page as fixture data with the fixture label', async () => {
    const page = await firstValueFrom(adapter.search(query()));

    expect(page.origin).toEqual({ kind: 'fixture', label: FIXTURE_LABEL });
    for (const item of page.items) {
      expect(item.origin).toEqual({ kind: 'fixture', label: FIXTURE_LABEL });
    }
  });

  it('has no poster images, so no external image URLs are used', () => {
    expect(FIXTURE_MOVIES.every((movie) => movie.poster === null)).toBe(true);
  });

  it('returns the genre and provider taxonomies', async () => {
    expect(await firstValueFrom(adapter.genres())).toEqual(FIXTURE_GENRES);
    expect(await firstValueFrom(adapter.providers('US'))).toEqual(FIXTURE_PROVIDERS);
  });

  it('reports availability as unknown for a region other than the fixture region', async () => {
    const page = await firstValueFrom(adapter.search(query({ region: 'GB', page: { cursor: null, size: 100 } })));

    expect(page.items.length).toBeGreaterThan(0);
    for (const item of page.items) {
      expect(item.availability).toEqual({ status: 'unknown', region: 'GB' });
    }
  });

  it('emits a CatalogError that passes isCatalogError', async () => {
    let caught: unknown = null;
    try {
      await firstValueFrom(adapter.search(query({ page: { cursor: 'bad', size: 5 } })));
    } catch (e) {
      caught = e;
    }
    expect(isCatalogError(caught)).toBe(true);
    expect((caught as { code: string }).code).toBe('invalid-query');
  });

  it('sorts by popularity, which only the fixture adapter stores', async () => {
    const page = await firstValueFrom(adapter.search(query({ sort: { key: 'popularity', direction: 'desc' } })));
    expect(page.items.length).toBe(FIXTURE_MOVIES.length);
  });
});
