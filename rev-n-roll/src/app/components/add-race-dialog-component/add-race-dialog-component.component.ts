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
import { GoogleMapsModule } from '@angular/google-maps';
import { Race, RaceRequest, RaceType } from '../../models/race';
import { MapDialogComponent } from '../meets/meets.component'; // Reusing MapDialogComponent

@Component({
  selector: 'app-add-race-dialog-component',
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
  templateUrl: './add-race-dialog-component.component.html',
  styleUrls: ['./add-race-dialog-component.component.css']
})
export class AddRaceDialogComponent {
  raceForm: FormGroup;
  isSubmitting = false;
  isEditMode = false;
  raceTypes = RaceType; // For template access to enum
  raceTypeOptions = Object.keys(RaceType).filter(key => isNaN(Number(key))); // Get enum string values

  constructor(
    public dialogRef: MatDialogRef<AddRaceDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { race?: Race },
    private fb: FormBuilder,
    private dialog: MatDialog
  ) {
    this.isEditMode = !!data?.race;

    let initialLatitude: number | null = null;
    let initialLongitude: number | null = null;
    if (data?.race?.coordinates) {
      const [lat, lng] = data.race.coordinates.split(',').map(coord => parseFloat(coord.trim()));
      if (!isNaN(lat) && !isNaN(lng)) {
        initialLatitude = lat;
        initialLongitude = lng;
      }
    }

    let initialDate: Date | null = null;
    let initialTime: string | null = null;
    if (data?.race?.date) {
      const raceDate = new Date(data.race.date);
      initialDate = raceDate;
      initialTime = raceDate.toTimeString().slice(0, 5); // e.g., "14:30"
    }
    
    let initialRaceType: string = this.raceTypeOptions[0]; // Default to Drag
    if (this.isEditMode && data?.race?.raceType !== undefined) {
      initialRaceType = RaceType[data.race.raceType] as string;
    }

    this.raceForm = this.fb.group({
      name: [data?.race?.name || '', [Validators.required, Validators.minLength(2), Validators.maxLength(64)]],
      description: [data?.race?.description || '', [Validators.maxLength(500)]],
      crewId: [data?.race?.crewId || null],
      raceType: [initialRaceType, [Validators.required]],
      location: [data?.race?.location || '', [Validators.required, Validators.maxLength(128)]],
      latitude: [initialLatitude, [Validators.required, Validators.min(-90), Validators.max(90)]],
      longitude: [initialLongitude, [Validators.required, Validators.min(-180), Validators.max(180)]],
      date: [initialDate || new Date(), [Validators.required]],
      time: [initialTime || '', [Validators.required]], // Required time field in HH:mm format
      private: [data?.race?.private || false],
    });
  }

  openMapDialog() {
    const dialogRef = this.dialog.open(MapDialogComponent, {
      width: '600px',
      data: {
        latitude: this.raceForm.get('latitude')?.value || undefined,
        longitude: this.raceForm.get('longitude')?.value || undefined,
        distanceInKm: 50 
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.raceForm.patchValue({
          latitude: result.latitude,
          longitude: result.longitude
        });
      }
    });
  }

  onSubmit() {
    if (this.raceForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    const latitude = Number(this.raceForm.value.latitude.toFixed(6));
    const longitude = Number(this.raceForm.value.longitude.toFixed(6));
    const coordinates = `${latitude},${longitude}`; 

    const date = new Date(this.raceForm.value.date);
    const [hours, minutes] = this.raceForm.value.time.split(':').map(Number);
    date.setHours(hours, minutes, 0, 0); 

    const raceRequest: RaceRequest = {
      name: this.raceForm.value.name,
      description: this.raceForm.value.description,
      crewId: this.raceForm.value.crewId ? Number(this.raceForm.value.crewId) : undefined,
      raceType: RaceType[this.raceForm.value.raceType as keyof typeof RaceType], // Convert string back to enum value
      location: this.raceForm.value.location,
      coordinates: coordinates,
      date: date.toISOString(), 
      private: this.raceForm.value.private,
    };

    this.dialogRef.close(raceRequest);
  }

  onCancel(): void {
    if (this.raceForm.dirty && !this.isSubmitting) {
      const confirmCancel = confirm('You have unsaved changes. Are you sure you want to cancel?');
      if (!confirmCancel) return;
    }
    this.dialogRef.close();
  }
}
