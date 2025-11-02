// rev-n-roll/src/app/components/crews/crews.component.ts - MODIFIED

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavBarComponent } from '../nav-bar/nav-bar.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CrewService } from '../../services/crew.service';
import { Crew, CrewRequest } from '../../models/crew';
import { AddCrewDialogComponent } from '../add-crew-dialog/add-crew-dialog.component'; 
import { AuthService } from '../../services/auth.service';
import { MatTooltipModule } from '@angular/material/tooltip'; // Added for tooltips

@Component({
  selector: 'app-crews',
  standalone: true,
  imports: [
    CommonModule, 
    NavBarComponent,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './crews.component.html',
  styleUrls: ['./crews.component.css']
})
export class CrewsComponent implements OnInit {
  crews: Crew[] = [];
  isLoading = false;
  errorMessage: string | null = null;
  userId: number = parseInt(localStorage.getItem('id') || '0', 10);

  constructor(
    private crewService: CrewService,
    private authService: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadCrews();
  }

  loadCrews(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.crewService.getAllCrews().subscribe({
      next: (data) => {
        this.crews = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load crews. Please try again later.';
        console.error('Error loading crews:', err);
        this.isLoading = false;
      }
    });
  }

  onCreateCrew(): void {
    if (!this.userId) {
      this.snackBar.open('You must be logged in to create a crew.', 'Close', { panelClass: ['error-snackbar'] });
      return;
    }

    const dialogRef = this.dialog.open(AddCrewDialogComponent, {
      width: '500px',
      panelClass: 'custom-dialog-container',
    });

    dialogRef.afterClosed().subscribe((result: CrewRequest) => {
      if (result) {
        // Optimistically set submitting flag for visual feedback
        const dialogInstance = dialogRef.componentInstance;
        dialogInstance.isSubmitting = true;

        this.crewService.createCrew(result, this.userId).subscribe({
          next: () => {
            this.snackBar.open('Crew created successfully!', 'Close', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              panelClass: ['success-snackbar'],
            });
            this.loadCrews();
          },
          error: (error) => {
            console.error('Error creating crew:', error);
            const message = error.error?.message || 'Failed to create crew.';
            this.snackBar.open(message, 'Close', { panelClass: ['error-snackbar'], duration: 5000 });
          }
        }).add(() => {
          // Ensure submitting is reset regardless of success/failure
          if (dialogRef.componentInstance) dialogRef.componentInstance.isSubmitting = false;
        });
      }
    });
  }
  
  onEditCrew(crew: Crew): void {
    // Only allow editing if the current user is part of the crew
    if (!this.isMember(crew)) {
      this.snackBar.open('You are not a member of this crew.', 'Close', { panelClass: ['error-snackbar'] });
      return;
    }

    const dialogRef = this.dialog.open(AddCrewDialogComponent, {
      width: '500px',
      panelClass: 'custom-dialog-container',
      data: { crew },
    });

    dialogRef.afterClosed().subscribe((result: CrewRequest) => {
      if (result) {
        const dialogInstance = dialogRef.componentInstance;
        dialogInstance.isSubmitting = true;

        this.crewService.updateCrew(crew.id, result).subscribe({
          next: () => {
            this.snackBar.open('Crew updated successfully!', 'Close', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              panelClass: ['success-snackbar'],
            });
            this.loadCrews();
          },
          error: (error) => {
            console.error('Error updating crew:', error);
            const message = error.error?.message || 'Failed to update crew.';
            this.snackBar.open(message, 'Close', { panelClass: ['error-snackbar'], duration: 5000 });
          }
        }).add(() => {
          if (dialogRef.componentInstance) dialogRef.componentInstance.isSubmitting = false;
        });
      }
    });
  }

  onDeleteCrew(crewId: number, event: Event): void {
    event.stopPropagation();
    if (!confirm('Are you sure you want to delete this crew? This action cannot be undone.')) return;

    this.crewService.deleteCrew(crewId).subscribe({
      next: () => {
        this.snackBar.open('Crew deleted successfully!', 'Close', {
          duration: 3000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar'],
        });
        this.loadCrews();
      },
      error: (error) => {
        this.errorMessage = 'Failed to delete crew. Please try again.';
        console.error('Error deleting crew:', error);
        const message = error.error?.message || 'Failed to delete crew.';
        this.snackBar.open(message, 'Close', { panelClass: ['error-snackbar'], duration: 5000 });
      }
    });
  }

  // Helper to check if the current user is a member of the crew (for UI controls)
  isMember(crew: Crew): boolean {
    return crew.users.some(user => parseInt(user.id, 10) === this.userId);
  }
}