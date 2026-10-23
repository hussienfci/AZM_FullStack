import { Availability, CatalogMovie, DataOrigin, Rating, TaxonomyItem } from '../catalog.models';

/*
 * Sample data for development only. Titles, genres, providers and ratings are invented.
 * They must not be treated as live catalog data.
 */

export const FIXTURE_LABEL = 'Sample data — not live catalog';

export const FIXTURE_ORIGIN: DataOrigin = { kind: 'fixture', label: FIXTURE_LABEL };

/** Every fixture availability uses this region. Queries for other regions see 'unknown'. */
export const FIXTURE_REGION = 'US';

const RATING_SOURCE = 'Fixture rating';

export const FIXTURE_GENRES: TaxonomyItem[] = [
  { id: 'genre-drama', label: 'Drama' },
  { id: 'genre-comedy', label: 'Comedy' },
  { id: 'genre-thriller', label: 'Thriller' },
  { id: 'genre-documentary', label: 'Documentary' },
  { id: 'genre-animation', label: 'Animation' },
  { id: 'genre-sci-fi', label: 'Science fiction' },
];

export const FIXTURE_PROVIDERS: TaxonomyItem[] = [
  { id: 'provider-a', label: 'Streaming Service A' },
  { id: 'provider-b', label: 'Streaming Service B' },
  { id: 'provider-c', label: 'Streaming Service C' },
];

const available = (...providerIds: string[]): Availability => ({
  status: 'available',
  region: FIXTURE_REGION,
  providerIds,
});
const unavailable: Availability = { status: 'unavailable', region: FIXTURE_REGION };
const unknown: Availability = { status: 'unknown', region: FIXTURE_REGION };

const rating = (value: number, scale: number): Rating => ({ value, scale, source: RATING_SOURCE });

function fixtureMovie(
  id: string,
  title: string,
  releaseYear: number | null,
  ratingValue: Rating | null,
  genreIds: string[],
  availability: Availability,
): CatalogMovie {
  return { id, title, releaseYear, poster: null, rating: ratingValue, genreIds, availability, origin: FIXTURE_ORIGIN };
}

/**
 * Covers: null year, null rating, scales 5 / 10 / 100, unknown and unavailable availability,
 * non-ASCII titles, and two movies with the same title and year (tie-break).
 */
export const FIXTURE_MOVIES: CatalogMovie[] = [
  fixtureMovie('fx-001', 'The Silent Harbor', 2019, rating(8.1, 10), ['genre-drama'], available('provider-a')),
  fixtureMovie('fx-002', 'Night Lantern', 2021, rating(7.4, 10), ['genre-thriller'], available('provider-a', 'provider-b')),
  fixtureMovie('fx-003', 'Paper Kites', 2015, rating(6.2, 10), ['genre-comedy'], available('provider-b')),
  fixtureMovie('fx-004', 'Glass Orchard', 2020, rating(4.2, 5), ['genre-drama'], available('provider-c')),
  fixtureMovie('fx-005', 'Amélie Returns', 2018, rating(7.8, 10), ['genre-comedy'], available('provider-a')),
  fixtureMovie('fx-006', 'Ärger im Ort', 2017, rating(5.5, 10), ['genre-comedy'], unavailable),
  fixtureMovie('fx-007', 'Tidewater', null, rating(6.9, 10), ['genre-drama'], unknown),
  fixtureMovie('fx-008', 'The Unnamed', 2010, null, ['genre-thriller'], available('provider-b')),
  fixtureMovie('fx-009', 'Long Way Down', 2022, rating(71, 100), ['genre-documentary'], available('provider-c')),
  fixtureMovie('fx-010', 'Copper Sky', 2016, rating(6.8, 10), ['genre-sci-fi'], unknown),
  fixtureMovie('fx-011', 'Echo Chamber', 2016, rating(6.8, 10), ['genre-thriller'], available('provider-a')),
  fixtureMovie('fx-012', 'Echo Chamber', 2016, rating(6.8, 10), ['genre-thriller'], available('provider-c')),
  fixtureMovie('fx-013', 'Little Mountain', 2012, rating(5.0, 10), ['genre-animation'], available('provider-a', 'provider-c')),
  fixtureMovie('fx-014', 'Starfall', 2023, rating(8.8, 10), ['genre-sci-fi'], available('provider-a')),
  fixtureMovie('fx-015', 'Seaside Hotel', 2014, null, ['genre-comedy'], unavailable),
  fixtureMovie('fx-016', 'Quiet Engines', null, rating(3.5, 5), ['genre-sci-fi'], unknown),
  fixtureMovie('fx-017', 'Crème Brûlée Club', 2021, rating(7.0, 10), ['genre-comedy'], available('provider-b')),
  fixtureMovie('fx-018', 'Northern Lights', 2008, rating(6.1, 10), ['genre-documentary'], unavailable),
  fixtureMovie('fx-019', 'The Last Ferry', 2019, rating(7.2, 10), ['genre-drama'], available('provider-b', 'provider-c')),
  fixtureMovie('fx-020', 'Red Canyon', 2024, rating(6.6, 10), ['genre-thriller'], unknown),
  fixtureMovie('fx-021', 'Bright Side', 2013, rating(88, 100), ['genre-drama'], available('provider-a')),
  fixtureMovie('fx-022', 'Small Hours', 2011, rating(2.9, 5), ['genre-drama'], available('provider-b')),
  fixtureMovie('fx-023', 'Gardeners', 2020, null, ['genre-documentary'], available('provider-c')),
  fixtureMovie('fx-024', 'Moonlit Dance', 2017, rating(7.6, 10), ['genre-animation'], available('provider-a')),
  fixtureMovie('fx-025', 'Kingdom of Glass', 2009, rating(5.9, 10), ['genre-sci-fi'], unavailable),
  fixtureMovie('fx-026', 'Zero Hour Café', 2022, rating(6.4, 10), ['genre-comedy'], available('provider-c')),
  fixtureMovie('fx-027', 'Ñandú', 2015, rating(4.8, 10), ['genre-animation'], available('provider-b')),
  fixtureMovie('fx-028', 'Harbor Lights', 2020, rating(7.9, 10), ['genre-drama'], unknown),
  fixtureMovie('fx-029', 'Final Signal', 2001, rating(45, 100), ['genre-sci-fi'], available('provider-a')),
  fixtureMovie('fx-030', 'Open Water Tales', 2018, rating(6.0, 10), ['genre-documentary'], available('provider-b')),
];

/** Fixture-only popularity, kept out of CatalogMovie. Values are deterministic. */
export const FIXTURE_POPULARITY: ReadonlyMap<string, number> = new Map(
  FIXTURE_MOVIES.map((movie, index) => [movie.id, (index * 37) % 101]),
);
