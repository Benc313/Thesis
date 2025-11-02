import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SmallEvent } from '../../models/event';
import { NavBarComponent } from '../nav-bar/nav-bar.component';
import { RaceService } from '../../services/race.service'; // New Service
import { UserService } from '../../services/user.service';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MapDialogComponent } from '../meets/meets.component'; // Reusing MapDialogComponent
// --- CORRECTED IMPORTS START ---
import { AddRaceDialogComponent } from '../add-race-dialog-component/add-race-dialog-component.component';
import { RaceDetailsDialogComponent } from '../race-details-dialog-component/race-details-dialog-component.component';
// --- CORRECTED IMPORTS END ---
import { RaceRequest } from '../../models/race';


@Component({
  selector: 'app-races',
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
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    // The dialogs are correctly referenced here (now pointing to the right files)
    AddRaceDialogComponent,
    RaceDetailsDialogComponent
  ],
  templateUrl: './races.component.html',
  styleUrls: ['./races.component.css']
})
export class RacesComponent implements OnInit {
  allRaces: SmallEvent[] = [];
  userRaces: SmallEvent[] = [];
  userId: number = parseInt(localStorage.getItem('id') || '0', 10);
  isLoadingAllRaces = false;
  isLoadingUserRaces = false;
  errorMessageAllRaces: string | null = null;
  errorMessageUserRaces: string | null = null;

  // Filter properties (no tags for races, focusing on location)
  latitude: number | undefined;
  longitude: number | undefined;
  distanceInKm: number = 50;

  constructor(
    private raceService: RaceService, // Inject new RaceService
    private userService: UserService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadAllRaces();
    this.loadUserRaces();
  }

  loadAllRaces() {
    this.isLoadingAllRaces = true;
    this.errorMessageAllRaces = null;

    this.raceService.getFilteredRaces(
      this.latitude,
      this.longitude,
      this.distanceInKm,
    ).subscribe({
      next: (races) => {
        // Ensure only races are kept (backend should return only races, but this is a safeguard)
        this.allRaces = races.filter(e => !e.isMeet); 
        this.isLoadingAllRaces = false;
      },
      error: (error) => {
        this.errorMessageAllRaces = 'Failed to load all races. Please try again later.';
        console.error('Error loading all races:', error);
        this.isLoadingAllRaces = false;
      },
    });
  }

  loadUserRaces() {
    this.isLoadingUserRaces = true;
    this.errorMessageUserRaces = null;

    this.userService.getUserRaces(this.userId).subscribe({
      next: (races) => {
        this.userRaces = races.filter(e => !e.isMeet);
        this.isLoadingUserRaces = false;
      },
      error: (error) => {
        this.errorMessageUserRaces = 'Failed to load your races. Please try again later.';
        console.error('Error loading user races:', error);
        this.isLoadingUserRaces = false;
      },
    });
  }

  applyFilters() {
    this.loadAllRaces();
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

  viewRace(raceId: number) {
    this.dialog.open(RaceDetailsDialogComponent, {
      width: '100%',
      height: '100%',
      maxWidth: '100%',
      maxHeight: '100%',
      panelClass: 'full-screen-dialog',
      data: { raceId }
    });
  }

  onCreateRace() {
    const dialogRef = this.dialog.open(AddRaceDialogComponent, {
      width: '500px',
      panelClass: 'custom-dialog-container',
    });

    dialogRef.componentInstance.isSubmitting = false;
    dialogRef.afterClosed().subscribe((result: RaceRequest) => {
      if (result) {
        dialogRef.componentInstance.isSubmitting = true;
        this.raceService.addRace(result).subscribe({
          next: () => {
            this.loadUserRaces();
            this.snackBar.open('Race created successfully!', 'Close', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              panelClass: ['success-snackbar'],
            });
            dialogRef.componentInstance.isSubmitting = false;
          },
          error: (error) => {
            this.errorMessageUserRaces = 'Failed to create race. Please try again.';
            console.error('Error creating race:', error);
            this.snackBar.open('Failed to create race. Check console.', 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
            dialogRef.componentInstance.isSubmitting = false;
          },
        });
      }
    });
  }

  onEditRace(race: SmallEvent) {
    // Fetch full race details before opening the edit dialog
    this.raceService.getRace(race.id).subscribe({
      next: (fullRace) => {
        const dialogRef = this.dialog.open(AddRaceDialogComponent, {
          width: '500px',
          panelClass: 'custom-dialog-container',
          data: { race: fullRace },
        });
    
        dialogRef.componentInstance.isSubmitting = false;
        dialogRef.afterClosed().subscribe((result: RaceRequest) => {
          if (result) {
            dialogRef.componentInstance.isSubmitting = true;
            this.raceService.updateRace(race.id, result).subscribe({
              next: () => {
                this.loadUserRaces();
                this.snackBar.open('Race updated successfully!', 'Close', {
                  duration: 3000,
                  horizontalPosition: 'center',
                  verticalPosition: 'bottom',
                  panelClass: ['success-snackbar'],
                });
                dialogRef.componentInstance.isSubmitting = false;
              },
              error: (error) => {
                this.errorMessageUserRaces = 'Failed to update race. Please try again.';
                console.error('Error updating race:', error);
                this.snackBar.open('Failed to update race. Check console.', 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
                dialogRef.componentInstance.isSubmitting = false;
              },
            });
          }
        });
      },
      error: (err) => {
        this.snackBar.open('Failed to load race details for editing.', 'Close', { duration: 3000 });
      }
    });
  }

  onDeleteRace(raceId: number, event: Event) {
    event.stopPropagation();
    this.raceService.deleteRace(raceId).subscribe({
      next: () => {
        this.loadUserRaces();
        this.loadAllRaces();
        this.snackBar.open('Race deleted successfully!', 'Close', {
          duration: 3000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar'],
        });
      },
      error: (error) => {
        this.errorMessageUserRaces = 'Failed to delete race. Please try again.';
        console.error('Error deleting race:', error);
        this.snackBar.open('Failed to delete race. Check console.', 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
      },
    });
  }
}
