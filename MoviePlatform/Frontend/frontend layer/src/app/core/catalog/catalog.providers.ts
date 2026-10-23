import { EnvironmentProviders, inject, makeEnvironmentProviders, provideEnvironmentInitializer } from '@angular/core';

import { APP_CONFIG } from '../config/app-config';
import { CATALOG_ADAPTER } from './catalog-adapter';
import { FixtureCatalogAdapter } from './fixture-catalog.adapter';

/**
 * Selects the catalog adapter from APP_CONFIG.catalog.kind, so UI code never names an adapter.
 * An unknown kind fails at bootstrap rather than on first use.
 */
export function provideCatalogAdapter(): EnvironmentProviders {
  return makeEnvironmentProviders([
    {
      provide: CATALOG_ADAPTER,
      useFactory: () => {
        const { kind } = inject(APP_CONFIG).catalog;
        switch (kind) {
          case 'fixture':
            return new FixtureCatalogAdapter();
          default:
            throw new Error(`Unknown catalog adapter "${kind}"`);
        }
      },
    },
    provideEnvironmentInitializer(() => inject(CATALOG_ADAPTER)),
  ]);
}
