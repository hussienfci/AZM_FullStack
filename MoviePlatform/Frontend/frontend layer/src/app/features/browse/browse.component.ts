import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { formatRating } from '../../core/catalog/catalog-display';
import { API_BASE_URL } from '../../core/config/api.config';
import { APP_CONFIG, brandMonogram, resolveBrandName } from '../../core/config/app-config';
import { Movie } from '../../core/models/movie.models';
import { AuthService } from '../../core/services/auth.service';
import { MovieService } from '../../core/services/movie.service';

/** The movies API returns a bare 0-10 rating with no provider, so cards label it with the platform as the source. */
const RATING_SCALE = 10;
const RATING_SOURCE = 'Platform rating';

interface MeResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

/** Protected landing page. Calls GET /api/users/me so you can see the interceptor at work, and loads movies from GET /api/movies. */
@Component({
  selector: 'app-browse',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive],
  host: { 'data-theme': 'browse' },
  template: `
    <div class="ds-scope min-h-dvh">
      <header
        class="sticky top-0 z-20 flex items-center justify-between border-b border-ds-border bg-ds-bg/95 px-6 py-4 sm:px-12"
      >
        <div class="flex items-center gap-8">
          <div class="flex items-center gap-3">
            @if (logo && !logoFailed()) {
              <img [src]="logo.src" [alt]="logo.alt" (error)="logoFailed.set(true)" class="h-8 w-auto sm:h-9" />
            } @else {
              <span
                aria-hidden="true"
                class="flex h-8 w-8 items-center justify-center rounded-ds-control bg-ds-accent text-ds-meta font-black text-ds-on-accent sm:h-9 sm:w-9"
              >{{ monogram }}</span>
            }
            <span class="text-ds-card-title font-black tracking-tight text-ds-accent">{{ brandName }}</span>
          </div>
          <nav aria-label="Main" class="flex items-center gap-4">
            @for (item of navItems; track item.route) {
              <a
                [routerLink]="item.route"
                routerLinkActive="text-ds-text"
                [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
                class="ds-focus ds-touch ds-transition inline-flex items-center text-ds-meta text-ds-text-muted transition-colors hover:text-ds-text"
              >{{ item.label }}</a>
            }
          </nav>
        </div>
        <div class="flex items-center gap-4">
          <span class="hidden text-ds-meta text-ds-text-muted sm:inline">{{ auth.displayName() }}</span>
          <button type="button" (click)="auth.logout()" class="ds-btn-primary">Sign out</button>
        </div>
      </header>

      <section
        class="relative flex min-h-[55vh] items-end overflow-hidden bg-gradient-to-b from-ds-elevated via-ds-surface to-ds-bg px-6 pb-12 sm:px-12"
      >
        <div class="relative max-w-xl">
          <h1 class="text-ds-heading font-black">Welcome back{{ firstName() ? ', ' + firstName() : '' }}.</h1>
          <p class="mt-3 text-ds-text-muted">You're signed in. Your session is protected by a JWT sent on every API call.</p>

          <div class="mt-6 rounded-ds-card border border-ds-border bg-ds-surface p-4 text-ds-meta">
            @if (profile(); as me) {
              <p class="font-semibold text-ds-accent">GET /api/users/me → 200 OK</p>
              <p class="mt-1 text-ds-text-muted">{{ me.email }} · role: {{ me.role }} · id: {{ me.userId }}</p>
            } @else if (profileError()) {
              <p class="font-semibold text-ds-error">{{ profileError() }}</p>
            } @else {
              <p class="text-ds-text-muted">Loading your profile…</p>
            }
          </div>
        </div>
      </section>

      <section class="px-6 pb-12 pt-8 sm:px-12" aria-labelledby="latest-heading">
        <h2 id="latest-heading" class="mb-4 text-ds-heading font-semibold">Latest Movies</h2>

        @if (movieService.loading()) {
          <div class="flex gap-3 overflow-x-auto p-1" aria-busy="true" aria-label="Loading movies">
            @for (i of skeletons; track i) {
              <div class="w-40 shrink-0 sm:w-44">
                <div class="aspect-[2/3] animate-pulse rounded-ds-card bg-ds-elevated"></div>
                <div class="mt-2 h-4 w-3/4 animate-pulse rounded-ds-control bg-ds-elevated"></div>
                <div class="mt-1 h-3 w-1/2 animate-pulse rounded-ds-control bg-ds-elevated"></div>
              </div>
            }
          </div>
        } @else if (movieService.error(); as error) {
          <div
            class="flex items-center gap-4 rounded-ds-card border border-ds-control-border bg-ds-surface p-4 text-ds-meta"
            role="alert"
          >
            <p class="text-ds-error">{{ error }}</p>
            <button type="button" class="ds-btn-primary" (click)="movieService.load()">Retry</button>
          </div>
        } @else if (movieService.movies().length === 0) {
          <p class="text-ds-meta text-ds-text-muted">No movies available yet.</p>
        } @else {
          <div class="flex gap-3 overflow-x-auto p-1">
            @for (movie of movieService.movies(); track movie.id) {
              <article class="ds-transition w-40 shrink-0 transition-transform hover:scale-105 sm:w-44">
                @if (hasPoster(movie)) {
                  <img
                    [src]="movie.posterUrl"
                    [alt]="movie.title + ' poster'"
                    loading="lazy"
                    (error)="onPosterError(movie.posterUrl)"
                    class="aspect-[2/3] w-full rounded-ds-card bg-ds-elevated object-cover"
                  />
                } @else {
                  <div
                    role="img"
                    [attr.aria-label]="'No poster available for ' + movie.title"
                    class="flex aspect-[2/3] w-full items-center justify-center rounded-ds-card border border-ds-control-border bg-ds-elevated p-3 text-center text-ds-meta font-semibold text-ds-text-muted"
                  >
                    No poster
                  </div>
                }
                <h3 class="mt-2 line-clamp-2 text-ds-card-title" [title]="movie.title">{{ movie.title }}</h3>
                <p class="mt-1 flex flex-wrap gap-x-2 text-ds-meta text-ds-text-muted">
                  <span>{{ releaseYear(movie) ?? 'Year unknown' }}</span>
                  <span aria-hidden="true">·</span>
                  @if (ratingFor(movie); as rating) {
                    <span [attr.aria-label]="rating.ariaLabel">{{ rating.text }} · {{ ratingSource }}</span>
                  } @else {
                    <span>Not rated</span>
                  }
                </p>
              </article>
            }
          </div>
        }
      </section>
    </div>
  `,
})
export class BrowseComponent implements OnInit {
  private readonly http = inject(HttpClient);
  protected readonly auth = inject(AuthService);
  protected readonly movieService = inject(MovieService);
  protected readonly config = inject(APP_CONFIG);

  protected readonly brandName = resolveBrandName(this.config.brand);
  protected readonly monogram = brandMonogram(this.brandName);
  protected readonly logo = this.config.brand.logo;
  protected readonly logoFailed = signal(false);
  protected readonly navItems = this.config.navigation;

  protected readonly ratingSource = RATING_SOURCE;
  protected readonly skeletons = Array.from({ length: 8 }, (_, i) => i);
  private readonly brokenPosters = signal<ReadonlySet<string>>(new Set());
  protected readonly profile = signal<MeResponse | null>(null);
  protected readonly profileError = signal<string | null>(null);
  protected readonly firstName = computed(
    () => this.profile()?.firstName ?? this.auth.currentUser()?.firstName ?? '',
  );

  ngOnInit(): void {
    this.http.get<MeResponse>(`${API_BASE_URL}/users/me`).subscribe({
      next: (me) => this.profile.set(me),
      error: () => this.profileError.set('Could not load your profile from the API.'),
    });
    this.movieService.load();
  }

  protected releaseYear(movie: Movie): number | null {
    return movie.releaseDate ? new Date(movie.releaseDate).getFullYear() : null;
  }

  protected ratingFor(movie: Movie): { text: string; ariaLabel: string } | null {
    return movie.rating == null
      ? null
      : formatRating({ value: movie.rating, scale: RATING_SCALE, source: RATING_SOURCE });
  }

  protected hasPoster(movie: Movie): boolean {
    const url = movie.posterUrl?.trim();
    return !!url && !this.brokenPosters().has(url);
  }

  protected onPosterError(url: string | null | undefined): void {
    const key = url?.trim();
    if (key) this.brokenPosters.update((set) => new Set(set).add(key));
  }
}
