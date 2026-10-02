import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, map, Observable, throwError } from 'rxjs';

import { API_BASE_URL } from '../config/api.config';
import { AuthUser, LoginCredentials, LoginResponse, RegisterPayload } from '../models/auth.models';

const TOKEN_KEY = 'movie_auth_token';
const USER_KEY = 'movie_auth_user';

/** Reads the `exp` claim (seconds since epoch) from a JWT, or null if it can't be decoded. */
function getTokenExpiry(token: string): number | null {
  try {
    const payload = token.split('.')[1];
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    const exp = JSON.parse(json).exp;
    return typeof exp === 'number' ? exp * 1000 : null;
  } catch {
    return null;
  }
}

function isTokenExpired(token: string): boolean {
  const expiry = getTokenExpiry(token);
  return expiry === null || expiry <= Date.now();
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _token = signal<string | null>(null);
  private readonly _user = signal<AuthUser | null>(null);
  private expiryTimer: ReturnType<typeof setTimeout> | undefined;

  readonly currentUser = this._user.asReadonly();
  readonly isAuthenticated = computed(() => {
    const token = this._token();
    return token !== null && !isTokenExpired(token);
  });
  readonly displayName = computed(() => {
    const user = this._user();
    return user ? `${user.firstName} ${user.lastName}`.trim() || user.email : '';
  });

  constructor() {
    this.restoreSession();
  }

  login(credentials: LoginCredentials): Observable<AuthUser> {
    return this.http
      .post<LoginResponse>(`${API_BASE_URL}/users/login`, {
        email: credentials.email.trim(),
        password: credentials.password,
      })
      .pipe(
        map((res) => this.handleAuthResponse(res, credentials.rememberMe)),
        catchError((err) => throwError(() => new Error(this.toErrorMessage(err, 'Login failed.')))),
      );
  }

  register(payload: RegisterPayload): Observable<AuthUser> {
    const [firstName, ...rest] = payload.fullName.trim().split(/\s+/);
    return this.http
      .post<LoginResponse>(`${API_BASE_URL}/users/register`, {
        firstName,
        lastName: rest.join(' '),
        email: payload.email.trim(),
        password: payload.password,
      })
      .pipe(
        map((res) => this.handleAuthResponse(res, true)),
        catchError((err) =>
          throwError(() => new Error(this.toErrorMessage(err, 'Registration failed.'))),
        ),
      );
  }

  /** Returns the current token, or null if there is none or it has expired. */
  getToken(): string | null {
    const token = this._token();
    return token && !isTokenExpired(token) ? token : null;
  }

  logout(options: { returnUrl?: string; redirect?: boolean } = {}): void {
    clearTimeout(this.expiryTimer);
    for (const storage of [localStorage, sessionStorage]) {
      storage.removeItem(TOKEN_KEY);
      storage.removeItem(USER_KEY);
    }
    this._token.set(null);
    this._user.set(null);

    if (options.redirect !== false) {
      const returnUrl = options.returnUrl;
      const keepReturnUrl = returnUrl && !returnUrl.startsWith('/login');
      this.router.navigate(['/login'], keepReturnUrl ? { queryParams: { returnUrl } } : {});
    }
  }

  private handleAuthResponse(res: LoginResponse, rememberMe: boolean): AuthUser {
    if (!res.success || !res.token || !res.user) {
      throw new Error(res.message || 'Authentication failed.');
    }
    this.persistSession(res.token, res.user, rememberMe);
    return res.user;
  }

  /**
   * "Remember me" keeps the session in localStorage (survives browser restarts);
   * otherwise sessionStorage is used and the session ends when the tab closes.
   */
  private persistSession(token: string, user: AuthUser, rememberMe: boolean): void {
    const storage = rememberMe ? localStorage : sessionStorage;
    (rememberMe ? sessionStorage : localStorage).removeItem(TOKEN_KEY);
    storage.setItem(TOKEN_KEY, token);
    storage.setItem(USER_KEY, JSON.stringify(user));
    this._token.set(token);
    this._user.set(user);
    this.scheduleAutoLogout(token);
  }

  private restoreSession(): void {
    const storage = localStorage.getItem(TOKEN_KEY) ? localStorage : sessionStorage;
    const token = storage.getItem(TOKEN_KEY);
    if (!token || isTokenExpired(token)) {
      if (token) this.logout({ redirect: false });
      return;
    }
    try {
      this._user.set(JSON.parse(storage.getItem(USER_KEY) ?? 'null'));
    } catch {
      this._user.set(null);
    }
    this._token.set(token);
    this.scheduleAutoLogout(token);
  }

  private scheduleAutoLogout(token: string): void {
    clearTimeout(this.expiryTimer);
    const expiry = getTokenExpiry(token);
    if (expiry === null) return;
    // setTimeout overflows above ~24.8 days; tokens here live 24h, so cap defensively.
    const delay = Math.min(expiry - Date.now(), 2_147_483_647);
    this.expiryTimer = setTimeout(() => this.logout({ returnUrl: this.router.url }), delay);
  }

  private toErrorMessage(err: unknown, fallback: string): string {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 0) return 'Cannot reach the server. Is the API running?';
      const body = err.error;
      if (body && typeof body === 'object' && typeof body.message === 'string') return body.message;
      return fallback;
    }
    return err instanceof Error ? err.message : fallback;
  }
}
