// rev-n-roll/src/app/components/add-crew-dialog/add-crew-dialog.component.ts

import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { Crew, CrewRequest } from '../../models/crew';

@Component({
  selector: 'app-add-crew-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule, 
    MatDialogModule, 
    MatFormFieldModule, 
    MatInputModule, 
    MatButtonModule, 
    MatProgressSpinnerModule, 
    MatIconModule
  ],
  standalone: true,
  templateUrl: './add-crew-dialog.component.html',
  styleUrl: './add-crew-dialog.component.css'
})

export class AddCrewDialogComponent implements OnInit {
  crewForm: FormGroup;
  isSubmitting = false;
  isEditMode = false;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<AddCrewDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { crew?: Crew }
  ) {
    this.crewForm = this.fb.group({
      // Crew name validation based on C# Crew model (StringLength(32)) and Validator (Length(3, 32))
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(32)]],
      // Description validation based on C# CrewRequestValidator (MaximumLength(500))
      description: [null, Validators.maxLength(500)],
      // ImageLocation validation based on C# Crew model (StringLength(64)) and Validator (Required/NotNull)
      imageLocation: ['', [Validators.required, Validators.maxLength(64)]],
    });
  }

  ngOnInit(): void {
    if (this.data?.crew) {
      this.isEditMode = true;
      this.crewForm.patchValue(this.data.crew);
    }
  }

  onSubmit() {
    if (this.crewForm.invalid) return;

    this.isSubmitting = true;
    const crewRequest: CrewRequest = this.crewForm.value;
    
    this.dialogRef.close(crewRequest);
  }

  onCancel(): void {
    if (this.crewForm.dirty && !this.isSubmitting) {
      const confirmCancel = confirm('You have unsaved changes. Are you sure you want to cancel?');
      if (!confirmCancel) return;
    }
    this.dialogRef.close();
  }
}