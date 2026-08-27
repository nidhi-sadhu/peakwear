import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SizeRecommender } from './size-recommender';

describe('SizeRecommender', () => {
  let component: SizeRecommender;
  let fixture: ComponentFixture<SizeRecommender>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SizeRecommender],
    }).compileComponents();

    fixture = TestBed.createComponent(SizeRecommender);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
