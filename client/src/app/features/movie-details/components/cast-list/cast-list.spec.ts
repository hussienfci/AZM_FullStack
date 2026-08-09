import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CastList } from './cast-list';

describe('CastList', () => {
  let component: CastList;
  let fixture: ComponentFixture<CastList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CastList],
    }).compileComponents();

    fixture = TestBed.createComponent(CastList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
