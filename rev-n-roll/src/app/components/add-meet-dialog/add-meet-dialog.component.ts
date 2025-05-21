import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { GoogleMapsModule, MapInfoWindow, MapMarker } from '@angular/google-maps';
import { Meet, MeetRequest, MeetTags } from '../../models/meet';
import { MapDialogComponent } from '../meets/meets.component';

@Component({
  selector: 'app-add-meet-dialog',
  standalone: true,
  imports: [
    CommonModule,
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
    MatSelectModule,
    GoogleMapsModule,
  ],
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
    private fb: FormBuilder,
    private dialog: MatDialog
  ) {
    this.isEditMode = !!data?.meet;

    // Parse coordinates if in edit mode
    let initialLatitude: number | null = null;
    let initialLongitude: number | null = null;
    if (data?.meet?.coordinates) {
      const [lat, lng] = data.meet.coordinates.split(',').map(coord => parseFloat(coord.trim()));
      if (!isNaN(lat) && !isNaN(lng)) {
        initialLatitude = lat;
        initialLongitude = lng;
      }
    }

    // Extract date and time for edit mode
    let initialDate: Date | null = null;
    let initialTime: string | null = null;
    if (data?.meet?.date) {
      const meetDate = new Date(data.meet.date);
      initialDate = meetDate;
      initialTime = meetDate.toTimeString().slice(0, 5); // e.g., "14:30"
    }

    this.meetForm = this.fb.group({
      name: [data?.meet?.name || '', [Validators.required, Validators.minLength(2), Validators.maxLength(64)]],
      description: [data?.meet?.description || '', [Validators.maxLength(500)]],
      crewId: [data?.meet?.crewId || null],
      location: [data?.meet?.location || '', [Validators.required, Validators.maxLength(128)]],
      latitude: [initialLatitude, [Validators.required, Validators.min(-90), Validators.max(90)]],
      longitude: [initialLongitude, [Validators.required, Validators.min(-180), Validators.max(180)]],
      date: [initialDate || new Date(), [Validators.required]],
      time: [initialTime || '', [Validators.required]], // Required time field in HH:mm format
      private: [data?.meet?.private || false],
      tags: [data?.meet?.tags || [], [Validators.required]],
    });
  }

  openMapDialog() {
    const dialogRef = this.dialog.open(MapDialogComponent, {
      width: '600px',
      data: {
        latitude: this.meetForm.get('latitude')?.value || undefined,
        longitude: this.meetForm.get('longitude')?.value || undefined,
        distanceInKm: 50 // Not needed for meet creation, but required by MapDialogComponent
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.meetForm.patchValue({
          latitude: result.latitude,
          longitude: result.longitude
        });
      }
    });
  }

  onSubmit() {
    if (this.meetForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    // Limit precision to 6 decimal places to keep coordinates under 32 characters
    const latitude = Number(this.meetForm.value.latitude.toFixed(6));
    const longitude = Number(this.meetForm.value.longitude.toFixed(6));
    const coordinates = `${latitude},${longitude}`; // e.g., "47.497912,19.040236" (20 characters)

    // Combine date and time into a single Date object
    const date = new Date(this.meetForm.value.date);
    const [hours, minutes] = this.meetForm.value.time.split(':').map(Number);
    date.setHours(hours, minutes, 0, 0); // Set hours and minutes, reset seconds and milliseconds

    const meetRequest: MeetRequest = {
      name: this.meetForm.value.name,
      description: this.meetForm.value.description,
      crewId: this.meetForm.value.crewId ? Number(this.meetForm.value.crewId) : undefined,
      location: this.meetForm.value.location,
      coordinates: coordinates,
      date: date.toISOString(), // Combined date and time in ISO format
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