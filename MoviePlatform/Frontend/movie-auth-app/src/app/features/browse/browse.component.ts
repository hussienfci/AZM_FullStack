import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';

import { API_BASE_URL } from '../../core/config/api.config';
import { AuthService } from '../../core/services/auth.service';

interface MeResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

const ROWS = [
  { title: 'Trending Now', hues: [350, 20, 200, 260, 140, 40] },
  { title: 'Top Picks for You', hues: [220, 300, 10, 170, 50, 280] },
];

/** Protected landing page. Calls GET /api/users/me so you can see the interceptor at work. */
@Component({
  selector: 'app-browse',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="min-h-dvh bg-surface text-white">
      <header
        class="sticky top-0 z-20 flex items-center justify-between bg-gradient-to-b from-black/90 to-transparent px-6 py-4 sm:px-12"
      >
        <span class="text-2xl font-black tracking-tight text-brand sm:text-3xl">AZM<span class="text-white">FLIX</span></span>
        <div class="flex items-center gap-4">
          <span class="hidden text-sm text-neutral-300 sm:inline">{{ auth.displayName() }}</span>
          <button type="button" (click)="auth.logout()" class="btn-ghost">Sign out</button>
        </div>
      </header>

      <section class="relative flex min-h-[55vh] items-end overflow-hidden px-6 pb-14 sm:px-12">
        <div class="poster-wall absolute inset-0 opacity-50" aria-hidden="true"></div>
        <div class="absolute inset-0 bg-gradient-to-t from-surface via-surface/60 to-transparent" aria-hidden="true"></div>
        <div class="animate-fade-in relative max-w-xl">
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

      @for (row of rows; track row.title) {
        <section class="px-6 pb-10 sm:px-12">
          <h2 class="mb-3 text-lg font-semibold">{{ row.title }}</h2>
          <div class="flex gap-3 overflow-x-auto pb-2">
            @for (hue of row.hues; track $index) {
              <div
                class="aspect-video w-56 shrink-0 rounded-md transition-transform duration-300 hover:scale-105"
                [style.background]="'linear-gradient(135deg, hsl(' + hue + ' 70% 35%), hsl(' + (hue + 40) + ' 60% 12%))'"
              ></div>
            }
          </div>
        </section>
      }
    </div>
  `,
})
export class BrowseComponent implements OnInit {
  private readonly http = inject(HttpClient);
  protected readonly auth = inject(AuthService);

  protected readonly rows = ROWS;
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
  }
}
