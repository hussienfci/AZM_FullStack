import { TestBed } from '@angular/core/testing';

import {
  APP_CONFIG,
  BRAND_FALLBACK_NAME,
  brandMonogram,
  DEFAULT_APP_CONFIG,
  provideAppConfig,
  resolveBrandName,
  resolveRegion,
} from './app-config';

describe('resolveBrandName', () => {
  it.each([null, '', '   '])('falls back to the placeholder name for %j', (name) => {
    expect(resolveBrandName({ name, logo: null })).toBe('Movie Platform');
  });

  it('returns a configured name trimmed', () => {
    expect(resolveBrandName({ name: '  Acme  ', logo: null })).toBe('Acme');
  });

  it('uses the placeholder name constant', () => {
    expect(BRAND_FALLBACK_NAME).toBe('Movie Platform');
  });
});

describe('resolveRegion', () => {
  it('uppercases and trims a configured code', () => {
    expect(resolveRegion({ code: ' gb ', source: 'config', fallbackCode: 'US' })).toEqual({
      code: 'GB',
      source: 'config',
    });
  });

  it.each([null, '  '])('falls back to fallbackCode with source default for %j', (code) => {
    expect(resolveRegion({ code, source: 'config', fallbackCode: 'US' })).toEqual({
      code: 'US',
      source: 'default',
    });
  });
});

describe('brandMonogram', () => {
  it('uses the first letters of the first two words', () => {
    expect(brandMonogram('Movie Platform')).toBe('MP');
  });

  it('uses one letter for a single word', () => {
    expect(brandMonogram('Acme')).toBe('A');
  });

  it('ignores extra words and extra spaces', () => {
    expect(brandMonogram('  the   big  screen ')).toBe('TB');
  });
});

describe('provideAppConfig', () => {
  it('merges overrides with the defaults', () => {
    TestBed.configureTestingModule({
      providers: [provideAppConfig({ brand: { name: 'Acme', logo: null } })],
    });
    const config = TestBed.inject(APP_CONFIG);

    expect(config.brand.name).toBe('Acme');
    expect(config.navigation).toEqual(DEFAULT_APP_CONFIG.navigation);
    expect(config.catalog).toEqual(DEFAULT_APP_CONFIG.catalog);
  });

  it('provides the defaults when no overrides are given', () => {
    TestBed.configureTestingModule({ providers: [provideAppConfig()] });
    expect(TestBed.inject(APP_CONFIG)).toEqual(DEFAULT_APP_CONFIG);
  });
});
