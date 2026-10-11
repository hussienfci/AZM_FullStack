import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/** Cinematic backdrop + centered card shared by the login and register pages. */
@Component({
  selector: 'app-auth-layout',
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="relative min-h-dvh overflow-hidden bg-surface text-white">
      <!-- Backdrop: blurred poster wall + vignette -->
      <div class="poster-wall pointer-events-none absolute inset-0 opacity-40" aria-hidden="true"></div>
      <div
        class="pointer-events-none absolute inset-0 bg-gradient-to-b from-black/80 via-black/60 to-black/90"
        aria-hidden="true"
      ></div>

      <header class="relative z-10 px-6 py-5 sm:px-12">
        <a routerLink="/" class="text-3xl font-black tracking-tight text-brand sm:text-4xl">AZM<span class="text-white">FLIX</span></a>
      </header>

      <main class="relative z-10 flex justify-center px-4 pb-16 pt-4 sm:pt-10">
        <section
          class="animate-fade-in w-full max-w-md rounded-lg bg-black/75 px-6 py-10 shadow-2xl ring-1 ring-white/10 backdrop-blur-sm sm:px-14 sm:py-14"
        >
          <h1 class="mb-2 text-3xl font-bold">{{ title() }}</h1>
          @if (subtitle()) {
            <p class="mb-8 text-sm text-neutral-400">{{ subtitle() }}</p>
          }
          <ng-content />
        </section>
      </main>
    </div>
  `,
})
export class AuthLayoutComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
}
