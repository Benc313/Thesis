import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { Meet, MeetRequest, MeetTags } from '../../models/meet';
@Component({
  selector: 'app-add-meet-dialog',
  imports: [CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatSelectModule,],
  templateUrl: './add-meet-dialog.component.html',
  styleUrl: './add-meet-dialog.component.css'
})
export class AddMeetDialogComponent {
  meetForm: FormGroup;
  isSubmitting = false;
  isEditMode = false;
  meetTags = MeetTags; // For template access to enum
  meetTagsOptions = Object.keys(MeetTags).filter(key => isNaN(Number(key))); // Get enum string values

  constructor(
    public dialogRef: MatDialogRef<AddMeetDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { meet?: Meet },
    private fb: FormBuilder
  ) {
    this.isEditMode = !!data?.meet;
    this.meetForm = this.fb.group({
      name: [data?.meet?.name || '', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      description: [data?.meet?.description || '', [Validators.maxLength(500)]],
      crewId: [data?.meet?.crewId || null],
      location: [data?.meet?.location || '', [Validators.required, Validators.maxLength(200)]],
      coordinates: [data?.meet?.coordinates || '', [Validators.required, Validators.pattern(/^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?),\s*[-+]?(180(\.0+)?|((1[0-7]\d)|([1-9]?\d))(\.\d+)?)$/)]],
      date: [data?.meet?.date ? new Date(data.meet.date) : new Date(), [Validators.required]],
      private: [data?.meet?.private || false],
      tags: [data?.meet?.tags || [], [Validators.required]],
    });
  }

  onSubmit() {
    if (this.meetForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    const meetRequest: MeetRequest = {
      name: this.meetForm.value.name,
      description: this.meetForm.value.description,
      crewId: this.meetForm.value.crewId ? Number(this.meetForm.value.crewId) : undefined,
      location: this.meetForm.value.location,
      coordinates: this.meetForm.value.coordinates,
      date: this.meetForm.value.date.toISOString(),
      private: this.meetForm.value.private,
      tags: this.meetForm.value.tags.map((tag: string) => MeetTags[tag as keyof typeof MeetTags]),
    };

    this.dialogRef.close(meetRequest);
  }

  onCancel(): void {
    if (this.meetForm.dirty && !this.isSubmitting) {
      const confirmCancel = confirm('You have unsaved changes. Are you sure you want to cancel?');
      if (!confirmCancel) return;
    }
    this.dialogRef.close();
  }
}
