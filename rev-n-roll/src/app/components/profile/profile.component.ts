// profile.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { CarService } from '../../services/car.service';
import { CrewService } from '../../services/crew.service';
import { User } from '../../models/user';
import { Car } from '../../models/car';
import { Crew } from '../../models/crew';
import { NavBarComponent } from '../nav-bar/nav-bar.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { AddCarDialogComponent } from '../add-car-dialog-component/add-car-dialog-component.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EditUserDialogComponent } from '../edit-user-dialog/edit-user-dialog.component';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    NavBarComponent,
    MatCardModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatButtonToggleModule,
    AddCarDialogComponent,
    MatSnackBarModule,
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  user: User | null = null;
  cars: Car[] = [];
  crews: Crew[] = [];
  userId: number = parseInt(localStorage.getItem('id') || '0', 10);
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private userService: UserService,
    private carService: CarService,
    private crewService: CrewService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    public router: Router,
  ) {}

  ngOnInit(): void {
    this.userService.getUser(this.userId).subscribe(user => {
      this.user = user;
    });
    this.carService.getCars(this.userId).subscribe(cars => {
      this.cars = cars;
    });
    this.crewService.getUserCrews(this.userId).subscribe(crews => {
      this.crews = crews;
    });
  }

  onEditProfile(): void {
    if (!this.user) return;

    const dialogRef = this.dialog.open(EditUserDialogComponent, {
        width: '500px',
        panelClass: 'custom-dialog-container',
        data: this.user,
    });

    dialogRef.afterClosed().subscribe((updatedUser: User | undefined) => {
        if (updatedUser) {
            this.snackBar.open('User updated successfully!', 'Close', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              panelClass: ['success-snackbar'],
            });
            this.user = updatedUser;
        }
    });
  }
  
  loadCars() {
    if (!this.userId) return;
    this.isLoading = true;
    this.errorMessage = null;
    this.carService.getCars(this.userId).subscribe({
      next: (cars) => {
        this.cars = cars;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load cars. Please try again later.';
        console.error('Error loading cars:', error);
        this.isLoading = false;
      }
    });
  }

  onAddCar() {
    if (!this.userId) {
      this.errorMessage = 'User not authenticated. Please log in.';
      return;
    }

    const dialogRef = this.dialog.open(AddCarDialogComponent, {
      width: '500px',
      panelClass: 'custom-dialog-container',
    });

    dialogRef.componentInstance.isSubmitting = false;
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        dialogRef.componentInstance.isSubmitting = true;
        this.carService.addCar(this.userId!, result).subscribe({
          next: (newCar) => {
            this.loadCars();
            // Add snackbar message
            this.snackBar.open('Car added successfully!', 'Close', {
              duration: 3000, // Duration in milliseconds (3 seconds)
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              panelClass: ['success-snackbar'], // Optional: Custom styling
            });
            dialogRef.componentInstance.isSubmitting = false;
          },
          error: (error) => {
            this.errorMessage = 'Failed to add car. Please try again.';
            console.error('Error adding car:', error);
            dialogRef.componentInstance.isSubmitting = false;
          }
        });
      }
    });
  }

  deleteCar(carId: number, event: Event) {
    event.stopPropagation();
    if (!this.userId) {
      this.errorMessage = 'User not authenticated. Please log in.';
      return;
    }

    this.carService.deleteCar(carId).subscribe({
      next: () => {
        this.snackBar.open('Car deleted successfully!', 'Close', {
          duration: 3000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          panelClass: ['success-snackbar'],
      });
        this.loadCars();
      },
      error: (error) => {
        this.errorMessage = 'Failed to delete car. Please try again.';
        console.error('Error deleting car:', error);
      },
    });
  }

  openEditCarDialog(car: Car) {
    if (!this.userId) {
      this.errorMessage = 'User not authenticated. Please log in.';
      return;
    }

    const dialogRef = this.dialog.open(AddCarDialogComponent, {
      width: '500px',
      panelClass: 'custom-dialog-container',
      data: { car },
    });

    dialogRef.componentInstance.isSubmitting = false;
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Car data to update:', result);
        dialogRef.componentInstance.isSubmitting = true;
        this.carService.updateCar(result.id, result).subscribe({
          next: () => {
            this.snackBar.open('Car updated successfully!', 'Close', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            panelClass: ['success-snackbar'],
          });
            this.loadCars();
            dialogRef.componentInstance.isSubmitting = false;
          },
          error: (error) => {
            this.errorMessage = 'Failed to update car. Please try again.';
            console.error('Error updating car:', error);
            dialogRef.componentInstance.isSubmitting = false;
          },
        });
      }
    });
  }

  getUserRank(crew: Crew): string {
    const me = crew.users.find(u => u.id === this.userId.toString());
    if (!me || me.rank === undefined) return 'Member';
    const labels = ['Member', 'Moderator', 'Leader'];
    return labels[me.rank] || 'Member';
  }

  getRankClass(crew: Crew): string {
    const me = crew.users.find(u => u.id === this.userId.toString());
    switch (me?.rank) {
      case 2: return 'bg-amber-500/20 text-amber-300';
      case 1: return 'bg-blue-500/20 text-blue-300';
      default: return 'bg-slate-600/50 text-slate-300';
    }
  }

  viewCrew(crewId: number): void {
    this.router.navigate(['/crew', crewId]);
  }
}