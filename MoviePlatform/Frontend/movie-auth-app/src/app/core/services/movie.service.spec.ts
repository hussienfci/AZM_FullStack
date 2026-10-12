import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { Movie } from '../models/movie.models';
import { MovieService } from './movie.service';

const movieA: Movie = {
  id: 1,
  title: 'Movie A',
  description: 'First movie',
  genre: 'Drama',
  durationMinutes: 120,
  releaseDate: '2020-05-01T00:00:00',
  director: 'Someone',
  posterUrl: 'https://example.com/a.jpg',
  trailerUrl: null,
  rating: 8.1,
  language: 'English',
  country: 'US',
  isActive: true,
  createdAt: '2024-01-01T00:00:00',
};
const movieB: Movie = { ...movieA, id: 2, title: 'Movie B', posterUrl: null, rating: null };

describe('MovieService', () => {
  let service: MovieService;
  let httpTesting: HttpTestingController;

  const expectMoviesRequest = () =>
    httpTesting.expectOne(
      (r) => r.url === '/api/movies' && r.params.get('page') === '1' && r.params.get('pageSize') === '20',
    );

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(MovieService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('requests page 1 / pageSize 20 with GET', () => {
    service.load();
    const req = expectMoviesRequest();
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, message: 'ok', data: [], totalCount: 0 });
  });

  it('sets loading true while in flight, false after', () => {
    service.load();
    expect(service.loading()).toBe(true);
    expectMoviesRequest().flush({ success: true, message: 'ok', data: [], totalCount: 0 });
    expect(service.loading()).toBe(false);
  });

  it('populates movies on success', () => {
    service.load();
    expectMoviesRequest().flush({ success: true, message: 'ok', data: [movieA, movieB], totalCount: 2 });
    expect(service.movies()).toEqual([movieA, movieB]);
    expect(service.error()).toBeNull();
  });

  it('treats a success: false body as an error', () => {
    const message = 'An error occurred while retrieving movies';
    service.load();
    expectMoviesRequest().flush({ success: false, message, data: [], totalCount: 0 });
    expect(service.error()).toBe(message);
    expect(service.movies()).toEqual([]);
    expect(service.loading()).toBe(false);
  });

  it('sets a generic error on HTTP 500', () => {
    service.load();
    expectMoviesRequest().flush(null, { status: 500, statusText: 'Server Error' });
    expect(service.error()).toBe('Could not load movies.');
    expect(service.movies()).toEqual([]);
  });

  it('sets a reachability error on network failure', () => {
    service.load();
    expectMoviesRequest().error(new ProgressEvent('error'));
    expect(service.error()).toBe('Cannot reach the server. Is the API running?');
  });

  it('clears the previous error on retry', () => {
    service.load();
    expectMoviesRequest().flush(null, { status: 500, statusText: 'Server Error' });
    expect(service.error()).not.toBeNull();

    service.load();
    expect(service.error()).toBeNull();
    expectMoviesRequest().flush({ success: true, message: 'ok', data: [movieA], totalCount: 1 });
    expect(service.movies()).toEqual([movieA]);
  });
});
