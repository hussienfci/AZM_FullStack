import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MovieFilters } from './movie-filters';

describe('MovieFilters', () => {
  let component: MovieFilters;
  let fixture: ComponentFixture<MovieFilters>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MovieFilters],
    }).compileComponents();

    fixture = TestBed.createComponent(MovieFilters);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
