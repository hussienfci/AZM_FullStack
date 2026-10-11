# AZMFLIX – Movie Auth App

Angular 22 front end for the `UserManagementApi` backend: login, registration, JWT handling and route protection, with a dark streaming-service look.

Built with standalone components, signals, reactive forms, a functional `HttpInterceptorFn`, `CanActivateFn` guards, zoneless change detection and Tailwind CSS v4.

## How this project was generated

From `E:\AZM_FullStack\AZM_FullStack\MoviePlatform\`:

```powershell
npx @angular/cli@22 new movie-auth-app --style=tailwind --routing --ssr=false --zoneless --skip-git --ai-config=none
```

- `--style=tailwind` sets up Tailwind (`.postcssrc.json` + `@import 'tailwindcss'`).
- `--skip-git` is used because the repo root is already a git repository.
- Angular 22 needs **Node.js 22.22.3+ or 24.15+**. Check with `node -v`.

## Running it

1. Start the API with the **http** profile (it listens on `http://localhost:5247`):
   ```powershell
   cd E:\AZM_FullStack\AZM_FullStack\MoviePlatform\UserManagementApi
   dotnet run --launch-profile http
   ```
2. Start the Angular app:
   ```powershell
   cd E:\AZM_FullStack\AZM_FullStack\MoviePlatform\movie-auth-app
   npm install
   npm start
   ```
3. Open http://localhost:4200.

`ng serve` forwards every `/api/*` request to `http://localhost:5247` using `proxy.conf.json`, so you don't need CORS or HTTPS certificate setup during development. If your API runs on a different port, change `target` in `proxy.conf.json`.

Run the unit tests with `npm test`.

## Folder structure

```
src/app/
├── app.ts                          Root component (<router-outlet />)
├── app.config.ts                   Router + provideHttpClient(withInterceptors([jwtInterceptor]))
├── app.routes.ts                   Lazy routes with authGuard / publicOnlyGuard
├── core/
│   ├── config/api.config.ts        API_BASE_URL and PUBLIC_ENDPOINTS
│   ├── models/auth.models.ts       Types that mirror the backend DTOs
│   ├── services/auth.service.ts    Signals-based auth state, login/register/getToken/logout
│   ├── interceptors/
│   │   ├── jwt.interceptor.ts      Adds the Bearer token; logs out on 401
│   │   └── jwt.interceptor.spec.ts
│   └── guards/
│       ├── auth.guard.ts           Sends guests to /login?returnUrl=...
│       ├── public-only.guard.ts    Sends signed-in users to /browse
│       └── guards.spec.ts
└── features/
    ├── auth/
    │   ├── auth-layout/            Shared cinematic backdrop + card
    │   ├── login/                  LoginComponent (.ts + .html)
    │   ├── register/               RegisterComponent (.ts + .html)
    │   └── validators/             passwordStrengthValidator, matchFieldsValidator (+ spec)
    └── browse/browse.component.ts  Protected page; calls GET /api/users/me
```

## How it works

| Piece | Behaviour |
|---|---|
| `AuthService` | Holds the token and user in signals. `isAuthenticated` is a `computed` that also checks the JWT `exp` claim. Restores the session on page reload and logs you out automatically when the token expires. |
| Remember me | Checked: the session is kept in `localStorage` and survives a browser restart. Unchecked: it's kept in `sessionStorage` and ends when the tab closes. |
| `jwtInterceptor` | Adds `Authorization: Bearer <token>` only to `/api/*` calls. It never adds it to login/register or to third-party URLs. A 401 from a protected call logs you out and sends you to `/login?returnUrl=…`. A 401 from login itself is treated as "wrong password", not as a logout. |
| `authGuard` / `publicOnlyGuard` | Return a `UrlTree` redirect instead of calling `navigate()` imperatively. |
| Login | Email + password (min 6), remember me, show/hide password, inline errors, server error banner. Only same-site `returnUrl` values are followed, which blocks open redirects. |
| Register | Full name (split into `firstName` / `lastName` for the API), email, password rules (8+ characters, uppercase, number, special character) with a live strength meter, confirm password with a live match check, and a required terms checkbox. A successful registration signs you in. |
| Social buttons | Google and Apple buttons are UI only. The API has no OAuth endpoints yet, so clicking them shows a "coming soon" message. |

## Backend endpoints used

| Method | URL | Auth |
|---|---|---|
| POST | `/api/users/login` | public |
| POST | `/api/users/register` | public |
| GET | `/api/users/me` | Bearer token |
