import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';
import { publicOnlyGuard } from './public-only.guard';

describe('route guards', () => {
  let loggedIn = false;
  const route = {} as ActivatedRouteSnapshot;
  const state = { url: '/browse?tab=new' } as RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { isAuthenticated: () => loggedIn } },
      ],
    });
  });

  const run = (guard: typeof authGuard) =>
    TestBed.runInInjectionContext(() => guard(route, state));
  const serialize = (result: unknown) =>
    TestBed.inject(Router).serializeUrl(result as UrlTree);

  it('authGuard redirects guests to /login with a returnUrl', () => {
    loggedIn = false;
    expect(serialize(run(authGuard))).toBe('/login?returnUrl=%2Fbrowse%3Ftab%3Dnew');
  });

  it('authGuard lets signed-in users through', () => {
    loggedIn = true;
    expect(run(authGuard)).toBe(true);
  });

  it('publicOnlyGuard sends signed-in users to /browse', () => {
    loggedIn = true;
    expect(serialize(run(publicOnlyGuard))).toBe('/browse');
  });

  it('publicOnlyGuard lets guests through', () => {
    loggedIn = false;
    expect(run(publicOnlyGuard)).toBe(true);
  });
});
