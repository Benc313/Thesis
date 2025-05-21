import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MeetDetailsDialogComponent } from './meet-details-dialog.component';

describe('MeetDetailsDialogComponent', () => {
  let component: MeetDetailsDialogComponent;
  let fixture: ComponentFixture<MeetDetailsDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MeetDetailsDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MeetDetailsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
