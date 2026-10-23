# Browse Movies — foundation audit

Scope: story `refactoring-design` (MB-01). Records what already exists; nothing visible was changed.

## Framework

Angular 22 standalone app in `MoviePlatform/Frontend/frontend layer/`. Tailwind CSS 4 (`@tailwindcss/postcss`), Vitest via `ng test`, RxJS 7.8, TypeScript 6.

## Routes (`src/app/app.routes.ts`)

| Path | Guard | Component |
|---|---|---|
| `''` | — | redirect to `browse` |
| `login` | `publicOnlyGuard` | `LoginComponent` |
| `register` | `publicOnlyGuard` | `RegisterComponent` |
| `browse` | `authGuard` | `BrowseComponent` |
| `**` | — | redirect to `browse` |

There is no movie-detail route, so `DEFAULT_APP_CONFIG.movieDetail.resolve` returns `null` and cards are not links.

## Components

`features/browse/browse.component.ts` (only browse component), `features/auth/{auth-layout,login,register}`.

## Styles (`src/styles.css`)

Legacy tokens (lines 4–8) stay as they are: `--color-brand`, `--color-brand-dark`, `--color-surface`, `--font-sans`, `--animate-fade-in`. The `ds-` tokens fill gaps and already match the intake values.

| Intake token | CSS variable |
|---|---|
| bg | `--color-ds-bg` |
| surface | `--color-ds-surface` |
| surface-elevated | `--color-ds-elevated` |
| text-primary | `--color-ds-text` |
| text-secondary | `--color-ds-text-muted` |
| accent | `--color-ds-accent` |
| on-accent | `--color-ds-on-accent` |
| border-decorative | `--color-ds-border` |
| border-control | `--color-ds-control-border` |
| focus | `--color-ds-focus` |
| error | `--color-ds-error` |

Spacing uses Tailwind's default 4px scale (4, 8, 12, 16, 24, 32, 48, 64px), so no spacing tokens are added. Motion, focus and touch-target values are in `:root`. A global `prefers-reduced-motion` rule exists at the end of the file.

Precedence decision: browse keeps the legacy red brand and neutral palette it renders today. Switching it to the cyan `ds` accent would change visible output, which this story forbids. `ds` utilities (`ds-focus`, `ds-touch`, `font-system`) are used where they add behaviour only.

## Data fetching — two parallel paths

1. **Legacy `MovieService`** (`core/services/movie.service.ts`, `API_BASE_URL = '/api'`) calls `GET /api/movies`. It is used by `BrowseComponent`, delivered by story `show-real-movies-on-the-browse-page`.
2. **`CATALOG_ADAPTER`** is bound to `FixtureCatalogAdapter` by `provideCatalogAdapter()` (`core/catalog/catalog.providers.ts`), registered in `app.config.ts`. No component consumes it yet.

### Open item: browse still uses `MovieService`

The refactoring intake wants browse to read data only through `CATALOG_ADAPTER`. Doing that now would replace the live backend movies with fixture data, which is a visible behaviour change. This audit leaves browse on `MovieService`. The follow-up is a backend-backed `CatalogAdapter` that maps `MoviesListResponse` to `CatalogPage`; browse can then switch with no visible change.

## Mapping of refactoring steps

| Step | Status |
|---|---|
| 1 Audit | This document |
| 2 Design tokens | Already satisfied: `styles.css` `@theme`, `:root`, reduced-motion rule |
| 3 Platform config | Already satisfied: `core/config/app-config.ts` (`AppConfig`, `provideAppConfig`, `resolveBrandName`, `brandMonogram`); browse reads brand and navigation from it |
| 4 `CatalogAdapter` + fixture adapter | Already satisfied: `catalog-adapter.ts`, `fixture-catalog.adapter.ts`, `fixtures/catalog.fixtures.ts` |
| 5 Replace inline usages | Partly: no raw hex or brand string in browse, data source deferred (see above) |
| 6 Verify | Added in this story: stub-adapter contract spec, DI swap spec, hex guard test in `browse.component.spec.ts`. Unit tests only; no visual diff tooling exists in the repo |

## Deviations from the intake

- Fixtures are at `src/app/core/catalog/fixtures/catalog.fixtures.ts`, not `fixtures/catalog/`; kept to avoid import churn.
- The detail resolver is `MovieDetailResolver(movie: CatalogMovieRef)`, not `movieDetailHref(id)`; the id arrives through `CatalogMovieRef`.
- Browse remains on `MovieService` (see above).

## Open questions and current interim answers

| Question | Interim answer in code |
|---|---|
| Branding / logo | `BRAND_FALLBACK_NAME = 'Movie Platform'`, text monogram |
| Region | `RegionSourceKind`, default `US` (`FIXTURE_REGION`) |
| Popularity sort | `CatalogSortKey` includes `'popularity'`, backed by `FIXTURE_POPULARITY` |
| Detail route | `MovieDetailResolver` in `DEFAULT_APP_CONFIG`, returns `null` |
| Rating provider | Unresolved; fixtures use `Fixture rating` on scale 10 |
| Licensed images | Unresolved; fixtures have `poster: null` |
