import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddMeetDialogComponent } from './add-meet-dialog.component';

describe('AddMeetDialogComponent', () => {
  let component: AddMeetDialogComponent;
  let fixture: ComponentFixture<AddMeetDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddMeetDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddMeetDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
