import { firstValueFrom } from 'rxjs';

import { CatalogAdapter, isCatalogError } from './catalog-adapter';
import { CatalogPage, CatalogQuery } from './catalog.models';
import { FixtureCatalogAdapter } from './fixture-catalog.adapter';

/**
 * Reusable contract for any CatalogAdapter. Future adapters call this with their own factory,
 * so every adapter is held to the same behaviour.
 */
export function runCatalogAdapterContract(createAdapter: () => CatalogAdapter): void {
  const baseQuery = (overrides: Partial<CatalogQuery> = {}): CatalogQuery => ({
    region: 'US',
    sort: { key: 'title', direction: 'asc' },
    page: { cursor: null, size: 100 },
    ...overrides,
  });

  describe('CatalogAdapter contract', () => {
    it('declares an id and capabilities', () => {
      const adapter = createAdapter();
      expect(adapter.id.length).toBeGreaterThan(0);
      expect(adapter.capabilities.sorts).toContain('title');
    });

    it('returns a page whose items carry the page origin', async () => {
      const page = await firstValueFrom(createAdapter().search(baseQuery()));
      expect(page.items.length).toBeGreaterThan(0);
      for (const item of page.items) {
        expect(item.origin).toEqual(page.origin);
      }
    });

    it('signals end of results with a null cursor', async () => {
      const page = await firstValueFrom(createAdapter().search(baseQuery()));
      expect(page.nextCursor).toBeNull();
      expect(page.hasMore).toBe(false);
    });

    it('pages through results with cursors that cover every match once', async () => {
      const adapter = createAdapter();
      const seen: string[] = [];
      let cursor: string | null = null;
      do {
        const page: CatalogPage = await firstValueFrom(adapter.search(baseQuery({ page: { cursor, size: 5 } })));
        seen.push(...page.items.map((item) => item.id));
        cursor = page.nextCursor;
      } while (cursor !== null);

      const everything = await firstValueFrom(adapter.search(baseQuery()));
      expect(seen).toEqual(everything.items.map((item) => item.id));
      expect(new Set(seen).size).toBe(seen.length);
    });

    it('returns genre and provider taxonomies', async () => {
      const adapter = createAdapter();
      const genres = await firstValueFrom(adapter.genres());
      const providers = await firstValueFrom(adapter.providers('US'));
      expect(genres.length).toBeGreaterThan(0);
      expect(providers.length).toBeGreaterThan(0);
    });

    it('emits a structured CatalogError for an invalid query', async () => {
      const adapter = createAdapter();
      let caught: unknown = null;
      try {
        await firstValueFrom(adapter.search(baseQuery({ page: { cursor: 'not-a-cursor', size: 5 } })));
      } catch (e) {
        caught = e;
      }
      expect(isCatalogError(caught)).toBe(true);
    });
  });
}

runCatalogAdapterContract(() => new FixtureCatalogAdapter());
