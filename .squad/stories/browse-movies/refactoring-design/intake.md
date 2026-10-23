# MB-01 — Refactor: Extract Design Foundation and Catalog Integration Contract

| Field | Value |
|---|---|
| Feature | Browse Movies |
| Slug | `browse-movies` |
| Story ID | MB-01 |
| Type | Refactoring |
| Dependencies | None |

## User story

As a visitor, I want browsing to match the platform's visual language so the experience feels consistent — and as a developer, I want branding, tokens, and catalog access centralised so that future browse work doesn't touch scattered, endpoint-specific code.

## Motivation

Styling, branding, and data-fetching conventions currently live inline across components and routes. Before building browse features, consolidate them into a single foundation so that MB-02 and later stories compose existing pieces rather than duplicate them.

## Scope

Extract and consolidate only. Do not change visible behaviour and do not implement backend endpoints.

## Refactoring steps

1. **Audit** the existing framework, routes, components, styles, and data-fetching conventions. Record findings in `docs/browse-movies/audit.md`.
2. **Extract design tokens** into one theme module (CSS variables or the framework's equivalent). Where the platform already defines a value, keep it; use the proposed tokens below only to fill gaps.
3. **Extract platform config** into a single typed config object: brand name, logo, navigation, region, and a `movieDetailHref(id)` resolver. Default brand label: "Movie Platform" (temporary).
4. **Introduce a `CatalogAdapter` interface** and a clearly labelled **fixture adapter** (`FixtureCatalogAdapter`, data under `fixtures/catalog/`). No real endpoint paths are assumed or referenced.
5. **Replace inline usages** of hard-coded colours, brand strings, and ad-hoc fetch calls with the token, config, and adapter abstractions. Reuse compatible existing components; do not fork them.
6. **Verify** no visual regressions (snapshot or visual diff) and no behaviour changes.

## Design tokens (fallbacks only — existing platform branding takes precedence)

| Token | Value |
|---|---|
| `bg` | `#0B1020` |
| `surface` | `#151D30` |
| `surface-elevated` | `#202C43` |
| `text-primary` | `#F4F7FC` |
| `text-secondary` | `#B6C2D6` |
| `accent` | `#67E8F9` |
| `on-accent` | `#07141C` |
| `border-decorative` | `#34425A` |
| `border-control` | `#71839E` |
| `focus` | `#FCD34D` |
| `error` | `#FDA4AF` |

- Font: system sans-serif.
- Body: 1rem / line-height 1.5.
- Metadata: 0.875rem / line-height 1.45.
- Card titles: 1rem / weight 600.
- Page heading: `clamp(1.5rem, 1rem + 2vw, 2.5rem)`.
- Spacing scale: 4, 8, 12, 16, 24, 32, 48, 64 px.
- Radii: controls 8px; cards 12px; dialogs 16px; chips 999px.
- Focus ring: 3px with 3px offset.
- Touch targets: minimum 44 × 44 CSS px.
- Nonessential transitions: 120–180ms; respect `prefers-reduced-motion`.

## Catalog contract

### Movie

| Field | Notes |
|---|---|
| `id` | Stable, unique |
| `title` | Required |
| `year` | Nullable |
| `poster` | Nullable; `src` plus optional responsive candidates |
| `rating` | Nullable; `value`, `scale`, `sourceLabel` |
| `genreIds` | Array |
| `availability` | Region-specific, or explicit `unknown` |
| `detailHref` | Resolved via configuration |

### Adapter capabilities

- Title search.
- OR matching within each genre/provider group; AND matching between groups.
- Inclusive year range and minimum rating.
- Global deterministic sorting.
- Pagination with end-of-results indication.
- Optional exact total count.
- Genre/provider taxonomies.
- Structured errors.

No existing endpoint paths are assumed.

## Non-goals

- No new UI features.
- No backend endpoints.
- No JustWatch branding, text, or assets.

## Acceptance criteria

- Given existing components, when the refactor lands, then no component is duplicated and visible output is unchanged.
- Given no platform branding, when rendering, then the configurable placeholder branding is shown.
- Given a second adapter implementing `CatalogAdapter`, when swapped in, then no UI component requires changes.
- Given a rendered rating, then its source label and scale are visible and unambiguous.
- Given a grep for raw hex colours or brand strings in browse components, then zero matches remain.

## Open questions

- Confirm branding.
- Confirm framework.
- Confirm region source.
- Confirm rating provider.
- Confirm popularity capability.
- Confirm detail route.
- Confirm licensed image sources.