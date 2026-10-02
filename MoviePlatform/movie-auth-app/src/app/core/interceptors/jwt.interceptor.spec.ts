import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { jwtInterceptor } from './jwt.interceptor';

describe('jwtInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  const auth = { getToken: vi.fn(() => 'test-token'), logout: vi.fn() };

  beforeEach(() => {
    auth.getToken.mockClear();
    auth.logout.mockClear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([jwtInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('adds the Bearer token to protected API calls', () => {
    http.get('/api/users/me').subscribe();
    const req = httpTesting.expectOne('/api/users/me');
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
    req.flush({});
  });

  it('does not add the token to public endpoints', () => {
    http.post('/api/users/login', {}).subscribe({ error: () => undefined });
    const req = httpTesting.expectOne('/api/users/login');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(auth.logout).not.toHaveBeenCalled();
  });

  it('does not leak the token to third-party URLs', () => {
    http.get('https://example.com/data').subscribe();
    const req = httpTesting.expectOne('https://example.com/data');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('logs out when a protected call returns 401', () => {
    http.get('/api/users/me').subscribe({ error: () => undefined });
    httpTesting.expectOne('/api/users/me').flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(auth.logout).toHaveBeenCalledOnce();
  });
});
