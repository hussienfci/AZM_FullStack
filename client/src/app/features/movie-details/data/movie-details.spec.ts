import { TestBed } from '@angular/core/testing';

import { MovieDetails } from './movie-details';

describe('MovieDetails', () => {
  let service: MovieDetails;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MovieDetails);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
