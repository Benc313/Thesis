import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MeetService } from '../../services/meet.service';
import { SmallEvent } from '../../models/event';
import { NavBarComponent } from '../nav-bar/nav-bar.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AddMeetDialogComponent } from '../add-meet-dialog/add-meet-dialog.component';
import { MeetRequest } from '../../models/meet';
import { MatSpinner } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { MatOption, MatSelect, MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { UserService } from '../../services/user.service';

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
    MatSpinner,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
  ],
  templateUrl: './meets.component.html',
  styleUrls: ['./meets.component.css']
})
export class MeetsComponent implements OnInit {
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
  cities: { name: string, lat: number, lng: number }[] = [
    { name: 'Budapest', lat: 47.4979, lng: 19.0402 },
    { name: 'Debrecen', lat: 47.5316, lng: 21.6273 },
    { name: 'Szeged', lat: 46.2530, lng: 20.1414 },
    { name: 'Pécs', lat: 46.0727, lng: 18.2323 },
    { name: 'Győr', lat: 47.6875, lng: 17.6504 },
  ];
  selectedCity: string = '';
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

  loadAllMeets() {
    this.isLoadingAllMeets = true;
    this.errorMessageAllMeets = null;

    this.meetService.getMeets(
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
        this.userMeets = meets.filter(meet => meet.id === this.userId);
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

  updateCoordinates() {
    const city = this.cities.find(c => c.name === this.selectedCity);
    if (city) {
      this.latitude = city.lat;
      this.longitude = city.lng;
      this.applyFilters();
    } else {
      this.latitude = undefined;
      this.longitude = undefined;
    }
  }

  viewMeet(meetId: number) {
    this.router.navigate(['/meet', meetId]);
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