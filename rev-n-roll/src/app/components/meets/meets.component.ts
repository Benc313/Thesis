import { Component, OnInit, AfterViewInit, Inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MeetService } from '../../services/meet.service';
import { SmallEvent } from '../../models/event';
import { NavBarComponent } from '../nav-bar/nav-bar.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogRef, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AddMeetDialogComponent } from '../add-meet-dialog/add-meet-dialog.component';
import { MeetDetailsDialogComponent } from '../meet-details-dialog/meet-details-dialog.component'; // Add this import
import { MeetRequest } from '../../models/meet';
import { MatSpinner } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { UserService } from '../../services/user.service';
import { GoogleMap, GoogleMapsModule, MapInfoWindow, MapMarker } from '@angular/google-maps';

@Component({
  selector: 'app-meets',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NavBarComponent,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    AddMeetDialogComponent,
    MeetDetailsDialogComponent, // Add this to imports
    MatSpinner,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    GoogleMapsModule,
  ],
  templateUrl: './meets.component.html',
  styleUrls: ['./meets.component.css']
})
export class MeetsComponent implements OnInit, AfterViewInit {
  allMeets: SmallEvent[] = [];
  userMeets: SmallEvent[] = [];
  userId: number = parseInt(localStorage.getItem('id') || '0', 10);
  isLoadingAllMeets = false;
  isLoadingUserMeets = false;
  errorMessageAllMeets: string | null = null;
  errorMessageUserMeets: string | null = null;

  // Filter properties
  availableTags: string[] = ['Car Show', 'Drift', 'Cruise', 'Tuning', 'Classic Cars'];
  selectedTags: string[] = [];
  latitude: number | undefined;
  longitude: number | undefined;
  distanceInKm: number = 50;

  constructor(
    private meetService: MeetService,
    private userService: UserService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAllMeets();
    this.loadUserMeets();
  }

  ngAfterViewInit(): void {
    // Map will be initialized in the dialog
  }

  loadAllMeets() {
    this.isLoadingAllMeets = true;
    this.errorMessageAllMeets = null;

    this.meetService.getMeetsF(
      this.latitude,
      this.longitude,
      this.distanceInKm,
      this.selectedTags
    ).subscribe({
      next: (meets) => {
        this.allMeets = meets;
        this.isLoadingAllMeets = false;
      },
      error: (error) => {
        this.errorMessageAllMeets = 'Failed to load meets. Please try again later.';
        console.error('Error loading all meets:', error);
        this.isLoadingAllMeets = false;
      },
    });
  }

  loadUserMeets() {
    this.isLoadingUserMeets = true;
    this.errorMessageUserMeets = null;

    this.userService.getUserMeets(this.userId).subscribe({
      next: (meets) => {
        console.log('User meets:', meets, "User ID:", this.userId);
        this.userMeets = meets;
        this.isLoadingUserMeets = false;
      },
      error: (error) => {
        this.errorMessageUserMeets = 'Failed to load your meets. Please try again later.';
        console.error('Error loading user meets:', error);
        this.isLoadingUserMeets = false;
      },
    });
  }

  applyFilters() {
    this.loadAllMeets();
  }

  openMapDialog() {
    const dialogRef = this.dialog.open(MapDialogComponent, {
      width: '600px',
      data: {
        latitude: this.latitude,
        longitude: this.longitude,
        distanceInKm: this.distanceInKm
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.latitude = result.latitude;
        this.longitude = result.longitude;
        this.distanceInKm = result.distanceInKm;
        this.applyFilters();
      }
    });
  }

  viewMeet(meetId: number) {
    this.dialog.open(MeetDetailsDialogComponent, {
      width: '100%',
      height: '100%',
      maxWidth: '100%',
      maxHeight: '100%',
      panelClass: 'full-screen-dialog',
      data: { meetId }
    });
  }

  onCreateMeet() {
    const dialogRef = this.dialog.open(AddMeetDialogComponent, {
      width: '500px',
      panelClass: 'custom-dialog-container',
    });

    dialogRef.componentInstance.isSubmitting = false;
    dialogRef.afterClosed().subscribe((result: MeetRequest) => {
      if (result) {
        dialogRef.componentInstance.isSubmitting = true;
        this.meetService.addMeet(this.userId, result).subscribe({
          next: (newMeet) => {
            this.loadUserMeets();
            this.snackBar.open('Meet created successfully!', 'Close', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              panelClass: ['success-snackbar'],
            });
            dialogRef.componentInstance.isSubmitting = false;
          },
          error: (error) => {
            this.errorMessageUserMeets = 'Failed to create meet. Please try again.';
            console.error('Error creating meet:', error);
            dialogRef.componentInstance.isSubmitting = false;
          },
        });
      }
    });
  }

  onEditMeet(meet: SmallEvent) {
    const dialogRef = this.dialog.open(AddMeetDialogComponent, {
      width: '500px',
      panelClass: 'custom-dialog-container',
      data: { meet },
    });

    dialogRef.componentInstance.isSubmitting = false;
    dialogRef.afterClosed().subscribe((result: MeetRequest) => {
      if (result) {
        dialogRef.componentInstance.isSubmitting = true;
        this.meetService.updateMeet(meet.id, result).subscribe({
          next: () => {
            this.loadUserMeets();
            this.snackBar.open('Meet updated successfully!', 'Close', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              panelClass: ['success-snackbar'],
            });
            dialogRef.componentInstance.isSubmitting = false;
          },
          error: (error) => {
            this.errorMessageUserMeets = 'Failed to update meet. Please try again.';
            console.error('Error updating meet:', error);
            dialogRef.componentInstance.isSubmitting = false;
          },
        });
      }
    });
  }

  onDeleteMeet(meetId: number, event: Event) {
    event.stopPropagation();
    this.meetService.deleteMeet(meetId).subscribe({
      next: () => {
        this.loadUserMeets();
        this.loadAllMeets();
        this.snackBar.open('Meet deleted successfully!', 'Close', {
          duration: 3000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar'],
        });
      },
      error: (error) => {
        this.errorMessageUserMeets = 'Failed to delete meet. Please try again.';
        console.error('Error deleting meet:', error);
      },
    });
  }
}

@Component({
  selector: 'app-map-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    GoogleMapsModule,
  ],
  template: `
    <h2 mat-dialog-title>Select Location</h2>
    <mat-dialog-content>
      <google-map
        #map
        height="400px"
        width="100%"
        [center]="center"
        [zoom]="zoom"
        (mapClick)="onMapClick($event)"
      >
        <map-marker
          *ngIf="markerPosition"
          [position]="markerPosition"
        ></map-marker>
      </google-map>
      <div class="mt-4 flex items-center gap-4">
        <mat-form-field appearance="fill" class="w-32">
          <mat-label>Distance (km)</mat-label>
          <input matInput type="number" [(ngModel)]="distanceInKm" min="0">
        </mat-form-field>
        <p *ngIf="latitude && longitude" class="text-gray-300">
          Selected: ({{ latitude.toFixed(4) }}, {{ longitude.toFixed(4) }})
        </p>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="accent" (click)="onSave()">Save</button>
    </mat-dialog-actions>
  `,
  styles: [`
    google-map { display: block; }
  `]
})
export class MapDialogComponent implements AfterViewInit {
  @ViewChild('map') map!: GoogleMap;
  latitude: number | undefined;
  longitude: number | undefined;
  distanceInKm: number;
  center: google.maps.LatLngLiteral = { lat: 47.4979, lng: 19.0402 }; // Default to Budapest
  zoom = 10;
  markerPosition: google.maps.LatLngLiteral | undefined;

  constructor(
    public dialogRef: MatDialogRef<MapDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { latitude: number | undefined, longitude: number | undefined, distanceInKm: number }
  ) {
    this.latitude = data.latitude;
    this.longitude = data.longitude;
    this.distanceInKm = data.distanceInKm;

    if (this.latitude && this.longitude) {
      this.center = { lat: this.latitude, lng: this.longitude };
      this.markerPosition = { lat: this.latitude, lng: this.longitude };
    }
  }

  ngAfterViewInit(): void {
    // Map is initialized via the google-map component
  }

  onMapClick(event: google.maps.MapMouseEvent) {
    if (event.latLng) {
      this.latitude = event.latLng.lat();
      this.longitude = event.latLng.lng();
      this.markerPosition = { lat: this.latitude, lng: this.longitude };
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    this.dialogRef.close({
      latitude: this.latitude,
      longitude: this.longitude,
      distanceInKm: this.distanceInKm
    });
  }
}