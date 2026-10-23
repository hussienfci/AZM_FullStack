import { InjectionToken, Provider } from '@angular/core';

import type { CatalogMovieRef } from '../catalog/catalog.models';

/** Reusable application settings. Environment-specific values (API base URL) stay in api.config.ts. */

export interface BrandConfig {
  /** Null or blank falls back to BRAND_FALLBACK_NAME. */
  name: string | null;
  /** Null renders a text monogram built from the brand name. */
  logo: { src: string; alt: string } | null;
}

export interface NavItem {
  label: string;
  route: string;
  exact?: boolean;
}

export type RegionSourceKind = 'config' | 'user-preference' | 'browser-locale' | 'default';

export interface RegionConfig {
  code: string | null;
  source: RegionSourceKind;
  fallbackCode: string;
}

export type MovieDetailResolver = (movie: CatalogMovieRef) => string | unknown[] | null;

export interface CatalogAdapterConfig {
  kind: 'fixture' | string;
  options?: Record<string, unknown>;
}

export interface AppConfig {
  brand: BrandConfig;
  navigation: NavItem[];
  region: RegionConfig;
  movieDetail: { resolve: MovieDetailResolver };
  catalog: CatalogAdapterConfig;
}

export const BRAND_FALLBACK_NAME = 'Movie Platform';

export const DEFAULT_APP_CONFIG: AppConfig = {
  brand: { name: null, logo: null },
  navigation: [{ label: 'Browse', route: '/browse', exact: true }],
  region: { code: null, source: 'default', fallbackCode: 'US' },
  movieDetail: { resolve: () => null },
  catalog: { kind: 'fixture' },
};

export const APP_CONFIG = new InjectionToken<AppConfig>('APP_CONFIG', {
  providedIn: 'root',
  factory: () => DEFAULT_APP_CONFIG,
});

export function provideAppConfig(overrides: Partial<AppConfig> = {}): Provider {
  return { provide: APP_CONFIG, useValue: { ...DEFAULT_APP_CONFIG, ...overrides } };
}

export function resolveBrandName(cfg: BrandConfig): string {
  return cfg.name?.trim() || BRAND_FALLBACK_NAME;
}

export function resolveRegion(cfg: RegionConfig): { code: string; source: RegionSourceKind } {
  return cfg.code?.trim()
    ? { code: cfg.code.trim().toUpperCase(), source: cfg.source }
    : { code: cfg.fallbackCode, source: 'default' };
}

/** First letters of the first two words, uppercased. "Movie Platform" becomes "MP". */
export function brandMonogram(name: string): string {
  return name
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((word) => [...word][0].toUpperCase())
    .join('');
}
