import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { API_BASE_URL } from '../../core/config/api.config';
import { APP_CONFIG, brandMonogram, resolveBrandName } from '../../core/config/app-config';
import { Movie } from '../../core/models/movie.models';
import { AuthService } from '../../core/services/auth.service';
import { MovieService } from '../../core/services/movie.service';

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
  template: `
    <div class="min-h-dvh bg-surface font-system text-white">
      <header
        class="sticky top-0 z-20 flex items-center justify-between bg-gradient-to-b from-black/90 to-transparent px-6 py-4 sm:px-12"
      >
        <div class="flex items-center gap-8">
          <div class="flex items-center gap-3">
            @if (logo && !logoFailed()) {
              <img [src]="logo.src" [alt]="logo.alt" (error)="logoFailed.set(true)" class="h-8 w-auto sm:h-9" />
            } @else {
              <span
                aria-hidden="true"
                class="flex h-8 w-8 items-center justify-center rounded-md bg-brand text-sm font-black text-white sm:h-9 sm:w-9"
              >{{ monogram }}</span>
            }
            <span class="text-2xl font-black tracking-tight text-brand sm:text-3xl">{{ brandName }}</span>
          </div>
          <nav aria-label="Main" class="flex items-center gap-4">
            @for (item of navItems; track item.route) {
              <a
                [routerLink]="item.route"
                routerLinkActive="text-white"
                [routerLinkActiveOptions]="{ exact: item.exact ?? false }"
                class="ds-focus ds-touch inline-flex items-center text-sm text-neutral-300 hover:text-white"
              >{{ item.label }}</a>
            }
          </nav>
        </div>
        <div class="flex items-center gap-4">
          <span class="hidden text-sm text-neutral-300 sm:inline">{{ auth.displayName() }}</span>
          <button type="button" (click)="auth.logout()" class="btn-ghost ds-focus">Sign out</button>
        </div>
      </header>

      <section class="relative flex min-h-[55vh] items-end overflow-hidden px-6 pb-14 sm:px-12">
        <div class="poster-wall absolute inset-0 opacity-50" aria-hidden="true"></div>
        <div class="absolute inset-0 bg-gradient-to-t from-surface via-surface/60 to-transparent" aria-hidden="true"></div>
        <div class="relative max-w-xl">
          <h1 class="text-4xl font-black sm:text-5xl">Welcome back{{ firstName() ? ', ' + firstName() : '' }}.</h1>
          <p class="mt-3 text-neutral-300">You're signed in. Your session is protected by a JWT sent on every API call.</p>

          <div class="mt-6 rounded-md bg-black/60 p-4 text-sm ring-1 ring-white/10">
            @if (profile(); as me) {
              <p class="font-semibold text-emerald-400">GET /api/users/me → 200 OK</p>
              <p class="mt-1 text-neutral-300">{{ me.email }} · role: {{ me.role }} · id: {{ me.userId }}</p>
            } @else if (profileError()) {
              <p class="font-semibold text-red-400">{{ profileError() }}</p>
            } @else {
              <p class="text-neutral-400">Loading your profile…</p>
            }
          </div>
        </div>
      </section>

      <section class="px-6 pb-10 sm:px-12" aria-labelledby="latest-heading">
        <h2 id="latest-heading" class="mb-3 text-lg font-semibold">Latest Movies</h2>

        @if (movieService.loading()) {
          <div class="flex gap-3 overflow-x-auto pb-2" aria-busy="true" aria-label="Loading movies">
            @for (i of skeletons; track i) {
              <div class="w-40 shrink-0 sm:w-44">
                <div class="aspect-[2/3] animate-pulse rounded-md bg-neutral-800"></div>
                <div class="mt-2 h-4 w-3/4 animate-pulse rounded bg-neutral-800"></div>
                <div class="mt-1 h-3 w-1/2 animate-pulse rounded bg-neutral-800"></div>
              </div>
            }
          </div>
        } @else if (movieService.error(); as error) {
          <div class="flex items-center gap-4 rounded-md bg-black/60 p-4 text-sm ring-1 ring-white/10" role="alert">
            <p class="text-red-400">{{ error }}</p>
            <button type="button" class="btn-ghost ds-focus" (click)="movieService.load()">Retry</button>
          </div>
        } @else if (movieService.movies().length === 0) {
          <p class="text-sm text-neutral-400">No movies available yet.</p>
        } @else {
          <div class="flex gap-3 overflow-x-auto pb-2">
            @for (movie of movieService.movies(); track movie.id) {
              <article class="w-40 shrink-0 transition-transform duration-300 hover:scale-105 sm:w-44">
                @if (hasPoster(movie)) {
                  <img
                    [src]="movie.posterUrl"
                    [alt]="movie.title + ' poster'"
                    loading="lazy"
                    (error)="onPosterError(movie.posterUrl)"
                    class="aspect-[2/3] w-full rounded-md bg-neutral-800 object-cover"
                  />
                } @else {
                  <div
                    class="flex aspect-[2/3] w-full items-center justify-center rounded-md bg-neutral-800 p-3 text-center text-sm font-semibold text-neutral-400 ring-1 ring-white/10"
                  >
                    {{ movie.title }}
                  </div>
                }
                <h3 class="mt-2 truncate text-sm font-semibold" [title]="movie.title">{{ movie.title }}</h3>
                <p class="text-xs text-neutral-400">
                  @if (releaseYear(movie); as year) {
                    <span>{{ year }}</span>
                  }
                  @if (releaseYear(movie) && movie.rating != null) {
                    <span> · </span>
                  }
                  @if (movie.rating != null) {
                    <span class="text-amber-400">★ {{ movie.rating }}</span>
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

  protected hasPoster(movie: Movie): boolean {
    const url = movie.posterUrl?.trim();
    return !!url && !this.brokenPosters().has(url);
  }

  protected onPosterError(url: string | null | undefined): void {
    const key = url?.trim();
    if (key) this.brokenPosters.update((set) => new Set(set).add(key));
  }
}
