import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';

import {
  CatalogCapabilities,
  CatalogError,
  CatalogErrorCode,
  CatalogPage,
  CatalogQuery,
  TaxonomyItem,
} from './catalog.models';

/** Backend-independent catalog contract. UI code depends on this, never on a specific endpoint. */
export interface CatalogAdapter {
  readonly id: string;
  readonly capabilities: CatalogCapabilities;
  /** Errors are emitted as CatalogError. */
  search(query: CatalogQuery): Observable<CatalogPage>;
  genres(): Observable<TaxonomyItem[]>;
  providers(region: string): Observable<TaxonomyItem[]>;
}

export const CATALOG_ADAPTER = new InjectionToken<CatalogAdapter>('CATALOG_ADAPTER');

const CATALOG_ERROR_CODES: readonly CatalogErrorCode[] = [
  'network',
  'timeout',
  'invalid-query',
  'unsupported',
  'not-found',
  'unknown',
];

export function isCatalogError(e: unknown): e is CatalogError {
  if (typeof e !== 'object' || e === null) return false;
  const candidate = e as Record<string, unknown>;
  return (
    typeof candidate['code'] === 'string' &&
    (CATALOG_ERROR_CODES as readonly string[]).includes(candidate['code']) &&
    typeof candidate['message'] === 'string' &&
    typeof candidate['retryable'] === 'boolean'
  );
}
