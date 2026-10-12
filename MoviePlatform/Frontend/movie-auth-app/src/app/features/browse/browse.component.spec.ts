import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';

import { Movie } from '../../core/models/movie.models';
import { AuthService } from '../../core/services/auth.service';
import { MovieService } from '../../core/services/movie.service';
import { BrowseComponent } from './browse.component';

const BASE_MOVIE: Movie = {
  id: 1,
  title: 'Test Movie',
  genre: 'Action',
  isActive: true,
  createdAt: '2024-01-01T00:00:00',
};

const AUTH_STUB = {
  displayName: signal('Ann B'),
  currentUser: signal(null as null),
  logout: vi.fn(),
};

describe('BrowseComponent poster rendering', () => {
  let fixture: ComponentFixture<BrowseComponent>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BrowseComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AuthService, useValue: AUTH_STUB },
      ],
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(BrowseComponent);
    fixture.detectChanges(); // triggers ngOnInit

    httpTesting.expectOne('/api/users/me').flush({
      userId: '1',
      email: 'a@b.c',
      firstName: 'Ann',
      lastName: 'B',
      role: 'User',
    });
  });

  afterEach(() => httpTesting.verify());

  function flushMovies(movies: Movie[]): void {
    httpTesting
      .expectOne((r) => r.url === '/api/movies')
      .flush({ success: true, message: 'ok', data: movies, totalCount: movies.length });
    fixture.detectChanges();
  }

  it('renders a managed relative poster URL as-is', () => {
    const url = '/api/media/posters/0123456789abcdef0123456789abcdef.png';
    flushMovies([{ ...BASE_MOVIE, posterUrl: url }]);

    const cards = fixture.nativeElement.querySelectorAll('article');
    expect(cards.length).toBe(1);
    const img: HTMLImageElement | null = cards[0].querySelector('img');
    expect(img).not.toBeNull();
    expect(img!.getAttribute('src')).toBe(url);
    expect(img!.alt).toBe('Test Movie poster');
  });

  it('renders a legacy absolute URL', () => {
    const url = 'https://example.com/p.jpg';
    flushMovies([{ ...BASE_MOVIE, posterUrl: url }]);

    const img: HTMLImageElement | null = fixture.nativeElement.querySelector('article img');
    expect(img).not.toBeNull();
    expect(img!.getAttribute('src')).toBe(url);
  });

  it('falls back when posterUrl is null', () => {
    flushMovies([{ ...BASE_MOVIE, posterUrl: null }]);

    const card: HTMLElement = fixture.nativeElement.querySelector('article');
    const img: HTMLImageElement | null = card.querySelector('img');
    expect(img).toBeNull();
    expect(card.textContent).toContain(BASE_MOVIE.title);
  });

  it('falls back when posterUrl is whitespace', () => {
    flushMovies([{ ...BASE_MOVIE, posterUrl: '   ' }]);

    const card: HTMLElement = fixture.nativeElement.querySelector('article');
    expect(card.querySelector('img')).toBeNull();
  });

  it('falls back when the image fails to load', () => {
    const url = 'https://example.com/broken.jpg';
    flushMovies([{ ...BASE_MOVIE, posterUrl: url }]);

    const img: HTMLImageElement = fixture.nativeElement.querySelector('article img');
    expect(img).not.toBeNull();

    img.dispatchEvent(new Event('error'));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('article img')).toBeNull();
    const card: HTMLElement = fixture.nativeElement.querySelector('article');
    expect(card.textContent).toContain(BASE_MOVIE.title);
  });

  it('tries a replaced poster URL again after a prior URL failed', () => {
    const urlA = 'https://example.com/old.jpg';
    const urlB = 'https://example.com/new.jpg';

    flushMovies([{ ...BASE_MOVIE, id: 1, posterUrl: urlA }]);

    // Simulate the first URL failing
    const imgA: HTMLImageElement = fixture.nativeElement.querySelector('article img');
    imgA.dispatchEvent(new Event('error'));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('article img')).toBeNull();

    // MovieService reloads with a new URL for the same movie id
    TestBed.inject(MovieService).load();
    flushMovies([{ ...BASE_MOVIE, id: 1, posterUrl: urlB }]);

    const imgB: HTMLImageElement | null = fixture.nativeElement.querySelector('article img');
    expect(imgB).not.toBeNull();
    expect(imgB!.getAttribute('src')).toBe(urlB);
  });
});
