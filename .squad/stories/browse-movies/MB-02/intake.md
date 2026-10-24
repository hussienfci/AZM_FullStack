# MB-02 — Apply Browse Design Tokens

| Field | Value |
|---|---|
| Feature | Browse Movies |
| Slug | `browse-movies` |
| Story ID | MB-02 |
| Type | UI change (visual) |
| Dependencies | MB-01 completed (theme module, platform config, `CatalogAdapter` + fixture adapter in place) |

## User story

As a visitor, I want the browse experience to use the new visual language (dark navy surfaces, cyan accent, consistent typography and spacing) so that browsing feels polished and consistent across every browse screen.

## Motivation

MB-01 centralised colours, typography, spacing, and branding into a single theme module but deliberately preserved the current look (near-black background, red accent). This story switches the browse surface to the proposed design tokens. Because MB-01 already routed all browse components through the theme module, this is primarily a token-value change plus targeted component adjustments — not a rewrite.

## Scope

**In scope**

- The `/browse` route and every component rendered on it (header/navigation as shown on browse, page heading, movie rows/grids, movie cards, chips, controls, dialogs, empty/error states).
- The theme module values for the browse scope.
- Typography, spacing, radii, focus ring, touch targets, and motion rules for those components.

**Out of scope**

- Any route other than browse (home/dashboard, auth pages, account pages) keeps the current look.
- No changes to data fetching, the `CatalogAdapter` contract, routing, or backend.
- No new features (search, filters, pagination UI belong to later stories).

## Implementation requirements

1. **Scope the tokens.** Add a browse theme scope in the MB-01 theme module (e.g. a `[data-theme="browse"]` root or a scoped CSS-variable layer applied by the browse layout) so that only browse consumes the new values. Do not override global tokens used by other routes.
2. **Set the token values** (authoritative for browse — they replace, not fall back to, the existing values):

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

3. **Typography.** System sans-serif. Body 1rem / 1.5. Metadata (year, rating, genre chips) 0.875rem / 1.45. Card titles 1rem / weight 600. Page heading `clamp(1.5rem, 1rem + 2vw, 2.5rem)`.
4. **Spacing scale.** Use only 4, 8, 12, 16, 24, 32, 48, 64 px via the spacing tokens; remove any ad-hoc margins/paddings in browse components.
5. **Radii.** Controls 8px, cards 12px, dialogs 16px, chips 999px.
6. **Focus and touch.** Visible 3px focus ring with 3px offset using the `focus` token on every interactive element. All interactive elements at least 44 × 44 CSS px.
7. **Motion.** Nonessential transitions 120–180ms; disable them under `prefers-reduced-motion: reduce`.
8. **Header on browse.** Brand name, logo, and navigation continue to come from the MB-01 platform config. Only the colours and spacing change; the brand label stays whatever config supplies ("Movie Platform" placeholder by default). The primary action button (currently red) uses `accent` with `on-accent` text.
9. **Movie card.** Poster with 12px radius; title below in card-title style; metadata line in `text-secondary`. The rating must show value, scale, and source label from the MB-01 data contract (e.g. "6.5 / 10 · IMDb"), never a bare number. Missing poster/year/rating render an explicit placeholder, not a blank.
10. **Images.** Keep whatever poster source MB-01's fixture adapter provides; do not add new remote image sources.
11. **Contrast.** All text/background pairs meet WCAG AA (4.5:1 body, 3:1 large text/UI borders). Verify with the token values above; adjust usage, not the tokens, if a pairing fails.

## Acceptance criteria

- Given a visitor opens `/browse`, when the page renders, then the background is `#0B1020`, surfaces and cards use the surface tokens, and the accent is `#67E8F9` — no red accent remains on browse.
- Given a visitor opens any non-browse route, when the page renders, then it is visually identical to before this story (snapshot/visual diff passes).
- Given keyboard navigation on browse, when an element receives focus, then a 3px `#FCD34D` ring with 3px offset is visible.
- Given a movie card, when rendered, then the rating displays value, scale, and source label unambiguously.
- Given a movie with a null poster, year, or rating, when rendered, then a clearly labelled placeholder is shown and layout does not break.
- Given `prefers-reduced-motion: reduce`, when hovering or focusing cards, then no nonessential transitions run.
- Given a grep of browse components, then no raw hex colours, pixel spacing literals, or font-size literals remain — all come from tokens.
- Given the browse header, then brand name, logo, and nav items still originate from the platform config, not hard-coded strings.

## Verification hints for the planner

- Build and run the app; open `/browse` and the dashboard side by side.
- Run existing snapshot/visual tests; update only browse snapshots.
- Axe/Lighthouse accessibility pass on `/browse` with no contrast or target-size violations.

## Open questions

- Should the shared header adopt the new tokens app-wide in a later story, or stay dual-themed?
- Confirm the rating source label to display (fixture currently implies a 0–10 scale).