import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CrewDetailsDialogComponent } from './crew-details-dialog.component';

describe('CrewDetailsDialogComponent', () => {
  let component: CrewDetailsDialogComponent;
  let fixture: ComponentFixture<CrewDetailsDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrewDetailsDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CrewDetailsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
