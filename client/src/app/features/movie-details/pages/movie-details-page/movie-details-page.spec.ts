import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MovieDetailsPage } from './movie-details-page';

describe('MovieDetailsPage', () => {
  let component: MovieDetailsPage;
  let fixture: ComponentFixture<MovieDetailsPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MovieDetailsPage],
    }).compileComponents();

    fixture = TestBed.createComponent(MovieDetailsPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
