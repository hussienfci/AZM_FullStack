import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MovieGrid } from './movie-grid';

describe('MovieGrid', () => {
  let component: MovieGrid;
  let fixture: ComponentFixture<MovieGrid>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MovieGrid],
    }).compileComponents();

    fixture = TestBed.createComponent(MovieGrid);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
