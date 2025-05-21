import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { Car } from '../../models/car';

@Component({
  selector: 'app-add-car-dialog-component',
  imports: [
    CommonModule,
    ReactiveFormsModule, // For formGroup, formControlName
    MatDialogModule, // For MatDialogRef
    MatFormFieldModule, // For mat-form-field
    MatInputModule, // For matInput
    MatButtonModule, // For mat-button, mat-raised-button
    MatProgressSpinnerModule, // For mat-spinner
    MatIconModule // For mat-icon
  ],
  standalone: true,
  templateUrl: './add-car-dialog-component.component.html',
  styleUrl: './add-car-dialog-component.component.css'
})

export class AddCarDialogComponent {
  carForm: FormGroup;
  isSubmitting = false;
  isEditMode = false;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<AddCarDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: {car:Car
    }
  ) {
    this.carForm = this.fb.group({
      brand: ['', [Validators.required, Validators.minLength(2)]],
      model: ['', [Validators.required, Validators.minLength(2)]],
      description: ['', Validators.maxLength(500)],
      engine: ['', Validators.maxLength(50)],
      horsePower: [0, [Validators.required, Validators.min(0), Validators.pattern('^[0-9]*$')]],
    });
  }

  ngOnInit(): void {
    if (this.data?.car) {
      this.isEditMode = true;
      this.carForm.patchValue(this.data.car);
    }
  }

  onSubmit() {
    if (this.carForm.invalid) return;

    this.isSubmitting = true;
    const carData: Car = {
      ...this.carForm.value,
      id: this.data?.car?.id, // Preserve the ID for editing
    };
    this.dialogRef.close(carData);
  }

  onCancel(): void {
    if (this.carForm.dirty && !this.isSubmitting) {
      const confirmCancel = confirm('You have unsaved changes. Are you sure you want to cancel?');
      if (!confirmCancel) return;
    }
    this.dialogRef.close();
  }
}