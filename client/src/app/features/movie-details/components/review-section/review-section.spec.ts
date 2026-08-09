import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReviewSection } from './review-section';

describe('ReviewSection', () => {
  let component: ReviewSection;
  let fixture: ComponentFixture<ReviewSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReviewSection],
    }).compileComponents();

    fixture = TestBed.createComponent(ReviewSection);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
