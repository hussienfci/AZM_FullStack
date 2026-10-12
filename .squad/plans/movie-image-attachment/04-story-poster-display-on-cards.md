<!-- squad-kit: story-id=poster-display-on-cards, feature=movie-image-attachment, title="Show uploaded posters on Browse movie cards", planner=claude-opus-5-5, generated=2026-10-12 -->

# Plan: Show uploaded posters on Browse movie cards

## 1. Overview

Story 03 makes `MovieDto.posterUrl` hold either a **relative managed URL** (`/api/media/posters/<guid>.png`)
or a legacy **absolute URL** (`https://...`). The Browse page already renders `movie.posterUrl` in an
`<img>` and falls back to a title box when the URL is empty or fails to load. This story proves that
behaviour with a component spec (AC 7) and makes one small adjustment so the fallback resets when a
poster URL changes.

**Feature:** movie-image-attachment
**Story:** poster-display-on-cards
**Scope:** Frontend only (`MoviePlatform/Frontend/movie-auth-app`). **No backend changes** and no upload UI.
**Depends on:** 03 (poster upload API). The spec itself uses mocked HTTP, so it can be written before 03 is merged. The manual check needs 03.

All paths are relative to `MoviePlatform/Frontend/movie-auth-app` unless they say otherwise.

---

## 2. Context & Current State

- Angular 22 standalone + signals, Tailwind 4, Vitest via `ng test` (`describe/it/expect/vi` globals). Specs sit next to the code as `*.spec.ts`.
- `proxy.conf.json` forwards `/api` → `http://localhost:5247`. The relative poster URL `/api/media/posters/...` goes through the same proxy, so **no URL rewriting is needed**. Do not prefix it with a host.
- `jwtInterceptor` adds the bearer token to `/api/...` `HttpClient` calls only. `<img src>` requests do not go through `HttpClient`, which is why story 03 serves posters anonymously. **Do not** fetch posters through `HttpClient` / blob URLs.
- `src/app/features/browse/browse.component.ts`:
  - `ngOnInit` → `GET /api/users/me`, then `movieService.load()` → `GET /api/movies?page=1&pageSize=20`.
  - `hasPoster(movie)` = `!!movie.posterUrl?.trim() && !brokenPosters().has(movie.id)`.
  - `<img [src]="movie.posterUrl" [alt]="movie.title + ' poster'" (error)="onPosterError(movie.id)" ...>`. Otherwise a `div` shows the title.
  - `brokenPosters` is keyed by **movie id**. That is a bug once posters can be replaced: if movie 5's old URL failed, its new URL is never tried until a page reload.
- `src/app/core/models/movie.models.ts` already has `posterUrl?: string | null`. No model change is needed.
- `src/app/core/services/movie.service.spec.ts` shows the `TestBed` + `provideHttpClient()` + `provideHttpClientTesting()` + `HttpTestingController` pattern to copy.

---

## 3. Implementation Steps (In Order)

### Step 1: Key broken posters by URL, not by movie id
**Modify:** `src/app/features/browse/browse.component.ts`
```ts
private readonly brokenPosters = signal<ReadonlySet<string>>(new Set());

protected hasPoster(movie: Movie): boolean {
  const url = movie.posterUrl?.trim();
  return !!url && !this.brokenPosters().has(url);
}

protected onPosterError(url: string | null | undefined): void {
  const key = url?.trim();
  if (key) this.brokenPosters.update((set) => new Set(set).add(key));
}
```
In the template, change `(error)="onPosterError(movie.id)"` → `(error)="onPosterError(movie.posterUrl)"`. Nothing else in the template changes. Keep `loading="lazy"`, the classes, and the fallback `div` exactly as they are.

### Step 2: Component spec
**New file:** `src/app/features/browse/browse.component.spec.ts`

Setup:
```ts
await TestBed.configureTestingModule({
  imports: [BrowseComponent],
  providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
}).compileComponents();
httpTesting = TestBed.inject(HttpTestingController);
fixture = TestBed.createComponent(BrowseComponent);
fixture.detectChanges();                       // runs ngOnInit
httpTesting.expectOne('/api/users/me').flush({ userId: '1', email: 'a@b.c', firstName: 'Ann', lastName: 'B', role: 'User' });
```
Helper `flushMovies(movies: Movie[])`:
```ts
httpTesting.expectOne((r) => r.url === '/api/movies').flush({ success: true, message: 'ok', data: movies, totalCount: movies.length });
fixture.detectChanges();
```
`afterEach(() => httpTesting.verify())`. Build a `baseMovie: Movie` fixture with every required field filled.

If `AuthService` needs something at construction that the test lacks (for example `localStorage` or a router), give it a minimal stub with `{ provide: AuthService, useValue: { displayName: signal('Ann'), currentUser: signal(null), logout: vi.fn() } }` instead of changing the service.

Test cases. Query cards with `fixture.nativeElement.querySelectorAll('article')`:
1. **renders a managed relative poster URL as-is**: `posterUrl: '/api/media/posters/0123456789abcdef0123456789abcdef.png'` → the card has an `img` whose `getAttribute('src')` is exactly that string, and `alt` is `'<title> poster'`.
2. **renders a legacy absolute URL**: `posterUrl: 'https://example.com/p.jpg'` → `img[src]` equals it.
3. **falls back when posterUrl is null**: no `img` in the card; the card text contains the title in the fallback box.
4. **falls back when posterUrl is whitespace**: `'   '` → no `img`.
5. **falls back when the image fails to load**: render with a URL, then `img.dispatchEvent(new Event('error')); fixture.detectChanges();`. The `img` is gone and the fallback shows the title.
6. **a replaced poster URL is tried again after a failure**: render movie `id: 1` with URL A, dispatch `error`, then call `TestBed.inject(MovieService).load()`, flush the same movie with URL B, and run `detectChanges`. An `img` with `src` B is shown. This test covers the Step 1 fix.

### Step 3: Build & test
```bash
npm run build
npm test -- --watch=false
```
Both must pass. Fix strict-template errors in code; don't loosen `tsconfig`.

---

## 4. Testing Checklist

### 4.1 Automated
- [ ] All existing specs plus the 6 new `BrowseComponent` specs pass.
- [ ] `npm run build` succeeds.

### 4.2 Manual (API from story 03 running, `npm start`)
1. Upload a poster to a movie through Swagger (story 03 §5.2). Reload `/browse` and the card shows the image.
2. DevTools → Network: the image request goes to `/api/media/posters/<guid>.png` on the dev server origin, is proxied, and returns 200 with no `Authorization` header needed.
3. Remove the poster through Swagger and reload. The card shows the title fallback.
4. Delete the file from `UserManagementApi/App_Data/uploads/posters` while the URL is still set and reload. The card falls back and no broken-image icon appears.

---

## 5. Files Summary

| File | Change |
|------|--------|
| `src/app/features/browse/browse.component.ts` | `brokenPosters` keyed by URL; `(error)` passes `movie.posterUrl` |
| `src/app/features/browse/browse.component.spec.ts` | **New**: 6 poster render / fallback specs |

**Not touched:** `movie.models.ts`, `movie.service.ts`, `jwt.interceptor.ts`, `proxy.conf.json`, and all backend files.

## 6. Rollback Plan
`git checkout -- src/app/features/browse/browse.component.ts` and delete the new spec.

## 7. Out of Scope
- An admin upload/remove UI (no admin area exists yet). Add `uploadPoster` / `removePoster` to `MovieService` when that UI is planned.
- A movie detail page (the feature mentions it, but no detail route exists yet). It should reuse the same `hasPoster` / fallback logic when built.
