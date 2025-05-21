import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCarDialogComponentComponent } from './add-car-dialog-component.component';

describe('AddCarDialogComponentComponent', () => {
  let component: AddCarDialogComponentComponent;
  let fixture: ComponentFixture<AddCarDialogComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddCarDialogComponentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddCarDialogComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
