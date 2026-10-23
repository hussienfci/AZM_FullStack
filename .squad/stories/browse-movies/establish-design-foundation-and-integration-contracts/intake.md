# MB-01 — Establish Design Foundation and Integration Contracts

## Feature
- Name: Browse Movies
- Slug: browse-movies
- Dependencies: None

## User Story
As a visitor, I want browsing to match the platform's visual language so that the experience feels consistent.

## Objective
Establish the visual design foundation, configurable branding, shared configuration, and catalog integration contracts for the Browse Movies feature.

## Scope
- Inspect the existing framework, routes, components, styling system, and data-fetching conventions.
- Reuse compatible existing components and patterns.
- Define reusable design tokens and accessible UI defaults.
- Make branding, navigation, region, and movie-detail link generation configurable.
- Define a backend-independent catalog adapter contract.
- Use clearly identified fixtures until real catalog integration is available.

## Out of Scope
- Implementing backend endpoints.
- Assuming undocumented API paths.
- Copying JustWatch branding, text, or assets.
- Introducing unnecessary dependencies or replacing compatible existing components.

## Design Requirements
Use existing platform branding where appropriate. Otherwise, use the proposed tokens:

- Background: #0B1020
- Surface: #151D30
- Elevated surface: #202C43
- Primary text: #F4F7FC
- Secondary text: #B6C2D6
- Accent: #67E8F9
- On accent: #07141C
- Decorative border: #34425A
- Control border: #71839E
- Focus: #FCD34D
- Error: #FDA4AF

Typography:
- System sans-serif.
- Body: 1rem, line-height 1.5.
- Metadata: 0.875rem, line-height 1.45.
- Card titles: 1rem, weight 600.
- Page heading: clamp(1.5rem, 1rem + 2vw, 2.5rem).

Spacing: 4, 8, 12, 16, 24, 32, 48, 64px.

Radii: controls 8px, cards 12px, dialogs 16px, chips 999px.

Focus ring: 3px with 3px offset.
Minimum touch target: 44 × 44 CSS px.
Nonessential transitions: 120–180ms; respect reduced-motion preferences.

## Configuration Requirements
Provide configurable values for:
- Brand name, using "Movie Platform" only as a temporary fallback.
- Logo or brand mark.
- Navigation items.
- Active region and its source.
- Movie-detail destination generation.
- Catalog adapter selection and configuration where appropriate.

Keep environment-specific settings separate from reusable application configuration.

## Catalog Data Contract
Each movie must provide:
- Stable unique ID.
- Title.
- Nullable release year.
- Nullable poster with source and optional responsive candidates.
- Nullable rating with value, scale, and source label.
- Genre IDs.
- Region-specific availability or an explicit unknown state.
- Detail destination resolved through configuration.

The catalog adapter must support:
- Title search.
- OR matching within genre and provider filter groups.
- AND matching between groups.
- Inclusive year range and minimum rating.
- Global deterministic sorting.
- Pagination and end-of-results indication.
- Optional exact total count.
- Genre and provider taxonomies.
- Structured errors.

Do not assume existing endpoint paths or implement backend endpoints.

## Acceptance Criteria
1. Existing compatible platform components are reused.
2. Missing branding falls back to configurable placeholder branding.
3. The UI can use another adapter implementing the catalog contract without endpoint-specific UI changes.
4. Displayed ratings clearly identify their source and scale.
5. Unknown availability is distinguishable from confirmed unavailability.
6. Fixture data is clearly identified and not represented as live catalog data.
7. Configuration changes do not require rewriting browsing components.
8. Relevant accessibility requirements, including focus visibility and reduced motion, are respected.

## Open Questions
Confirm:
- Existing brand identity and logo.
- Framework and current design system.
- Region source and fallback.
- Rating provider and rating scale.
- Whether popularity sorting is supported.
- Movie-detail route and URL format.
- Licensed poster/image sources.

## Implementation Notes
Inspect the repository before selecting implementation locations. Document the actual files, reusable components, proposed configuration structure, catalog interface, and verification steps in the generated plan.

Resolve ambiguities where possible from existing code. Clearly flag decisions requiring product or stakeholder confirmation.

Do not implement backend endpoints or expand scope without approval.