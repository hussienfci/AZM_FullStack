import { CatalogError, CatalogMovie, CatalogQuery } from './catalog.models';
import { applyCatalogQuery } from './catalog-query';

const ORIGIN = { kind: 'fixture', label: 'Test data' } as const;

function movie(id: string, title: string, extra: Partial<CatalogMovie> = {}): CatalogMovie {
  return {
    id,
    title,
    releaseYear: null,
    poster: null,
    rating: null,
    genreIds: [],
    availability: { status: 'unknown', region: 'US' },
    origin: ORIGIN,
    ...extra,
  };
}

function query(overrides: Partial<CatalogQuery> = {}): CatalogQuery {
  return {
    region: 'US',
    sort: { key: 'title', direction: 'asc' },
    page: { cursor: null, size: 20 },
    ...overrides,
  };
}

function captureError(fn: () => unknown): CatalogError {
  try {
    fn();
  } catch (e) {
    return e as CatalogError;
  }
  throw new Error('expected the call to throw');
}

const ids = (items: CatalogMovie[]) => items.map((m) => m.id);

describe('applyCatalogQuery: search', () => {
  const movies = [movie('1', 'Amélie Returns'), movie('2', 'Other Title')];

  it('matches titles regardless of case and diacritics', () => {
    expect(ids(applyCatalogQuery(movies, query({ search: 'amelie' })).items)).toEqual(['1']);
    expect(ids(applyCatalogQuery(movies, query({ search: 'AMÉLIE' })).items)).toEqual(['1']);
  });

  it('treats an empty or whitespace search as no filter', () => {
    expect(applyCatalogQuery(movies, query({ search: '   ' })).total).toBe(2);
    expect(applyCatalogQuery(movies, query({ search: '' })).total).toBe(2);
  });
});

describe('applyCatalogQuery: group filters', () => {
  const movies = [
    movie('g1', 'Genre one', { genreIds: ['drama'] }),
    movie('g2', 'Genre two', { genreIds: ['comedy'] }),
    movie('g3', 'Genre three', { genreIds: ['horror'] }),
    movie('p1', 'Provider one', {
      genreIds: ['drama'],
      availability: { status: 'available', region: 'US', providerIds: ['a'] },
    }),
    movie('p2', 'Provider two', {
      genreIds: ['drama'],
      availability: { status: 'available', region: 'US', providerIds: ['b'] },
    }),
    movie('p3', 'Provider three', {
      genreIds: ['comedy'],
      availability: { status: 'available', region: 'US', providerIds: ['b'] },
    }),
    movie('p4', 'Provider unknown', {
      genreIds: ['drama'],
      availability: { status: 'unknown', region: 'US' },
    }),
  ];

  it('uses OR within the genre group', () => {
    // Default sort is title ascending: "Genre three" < "Genre two" < "Provider three".
    expect(ids(applyCatalogQuery(movies, query({ genreIds: ['comedy', 'horror'] })).items)).toEqual([
      'g3',
      'g2',
      'p3',
    ]);
  });

  it('uses OR within the provider group', () => {
    expect(ids(applyCatalogQuery(movies, query({ providerIds: ['a', 'b'] })).items)).toEqual(['p1', 'p3', 'p2']);
  });

  it('uses AND across the genre and provider groups', () => {
    expect(
      ids(applyCatalogQuery(movies, query({ genreIds: ['drama'], providerIds: ['b'] })).items),
    ).toEqual(['p2']);
  });

  it('excludes unknown availability when a provider filter is active', () => {
    expect(ids(applyCatalogQuery(movies, query({ providerIds: ['a'] })).items)).not.toContain('p4');
  });
});

describe('applyCatalogQuery: years and minimum rating', () => {
  it('includes both boundary years', () => {
    const movies = [
      movie('a', 'Before', { releaseYear: 1999 }),
      movie('b', 'Start', { releaseYear: 2000 }),
      movie('c', 'End', { releaseYear: 2010 }),
      movie('d', 'After', { releaseYear: 2011 }),
    ];
    // Titles: "End" sorts before "Start".
    expect(ids(applyCatalogQuery(movies, query({ yearFrom: 2000, yearTo: 2010 })).items)).toEqual(['c', 'b']);
  });

  it('excludes a null year when a bound is set, but keeps it with no year filter', () => {
    const movies = [movie('a', 'No year')];
    expect(applyCatalogQuery(movies, query({ yearFrom: 2000 })).total).toBe(0);
    expect(applyCatalogQuery(movies, query()).total).toBe(1);
  });

  it('compares minimum rating across scales 5, 10 and 100', () => {
    const movies = [
      movie('s5', 'Scale five', { rating: { value: 4, scale: 5, source: 'x' } }), // 8.0
      movie('s10', 'Scale ten', { rating: { value: 7.5, scale: 10, source: 'x' } }), // 7.5
      movie('s100', 'Scale hundred', { rating: { value: 70, scale: 100, source: 'x' } }), // 7.0
    ];
    expect(ids(applyCatalogQuery(movies, query({ minRating: 7.5 })).items).sort()).toEqual(['s10', 's5']);
  });

  it('excludes a null rating when minRating is set', () => {
    const movies = [movie('a', 'No rating')];
    expect(applyCatalogQuery(movies, query({ minRating: 1 })).total).toBe(0);
  });

  it('rejects yearFrom greater than yearTo', () => {
    const error = captureError(() => applyCatalogQuery([], query({ yearFrom: 2020, yearTo: 2010 })));
    expect(error.code).toBe('invalid-query');
  });
});

describe('applyCatalogQuery: sorting', () => {
  const movies = [
    movie('late', 'Late', { releaseYear: 2020 }),
    movie('null', 'Null year', { releaseYear: null }),
    movie('early', 'Early', { releaseYear: 2000 }),
  ];

  it('puts null values last in ascending order', () => {
    const result = applyCatalogQuery(movies, query({ sort: { key: 'releaseYear', direction: 'asc' } }));
    expect(ids(result.items)).toEqual(['early', 'late', 'null']);
  });

  it('puts null values last in descending order', () => {
    const result = applyCatalogQuery(movies, query({ sort: { key: 'releaseYear', direction: 'desc' } }));
    expect(ids(result.items)).toEqual(['late', 'early', 'null']);
  });

  it('breaks ties by title and then id, whatever the input order', () => {
    const duplicates = [movie('b', 'Same', { releaseYear: 2016 }), movie('a', 'Same', { releaseYear: 2016 })];
    const result = applyCatalogQuery(duplicates, query({ sort: { key: 'releaseYear', direction: 'asc' } }));
    expect(ids(result.items)).toEqual(['a', 'b']);
  });

  it('sorts by popularity from the supplied map', () => {
    const popularity = new Map([
      ['a', 10],
      ['b', 90],
    ]);
    const pair = [movie('a', 'A'), movie('b', 'B')];
    const result = applyCatalogQuery(pair, query({ sort: { key: 'popularity', direction: 'desc' } }), popularity);
    expect(ids(result.items)).toEqual(['b', 'a']);
  });
});

describe('applyCatalogQuery: pagination', () => {
  const movies = ['a', 'b', 'c', 'd', 'e'].map((id) => movie(id, `Title ${id}`));

  it('walks pages until nextCursor is null', () => {
    const first = applyCatalogQuery(movies, query({ page: { cursor: null, size: 2 } }));
    expect(ids(first.items)).toEqual(['a', 'b']);
    expect(first.nextCursor).toBe('2');

    const second = applyCatalogQuery(movies, query({ page: { cursor: first.nextCursor, size: 2 } }));
    expect(second.nextCursor).toBe('4');

    const last = applyCatalogQuery(movies, query({ page: { cursor: second.nextCursor, size: 2 } }));
    expect(ids(last.items)).toEqual(['e']);
    expect(last.nextCursor).toBeNull();
    expect(last.total).toBe(5);
  });

  it('rejects an invalid cursor', () => {
    const error = captureError(() => applyCatalogQuery(movies, query({ page: { cursor: 'abc', size: 2 } })));
    expect(error.code).toBe('invalid-query');
  });

  it('clamps the page size to between 1 and 100', () => {
    expect(applyCatalogQuery(movies, query({ page: { cursor: null, size: 0 } })).items).toHaveLength(1);

    const many = Array.from({ length: 150 }, (_, i) => movie(`m${i}`, `Movie ${i}`));
    expect(applyCatalogQuery(many, query({ page: { cursor: null, size: 500 } })).items).toHaveLength(100);
  });
});
