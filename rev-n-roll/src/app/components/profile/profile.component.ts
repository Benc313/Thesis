import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';
import { CarService } from '../../services/car.service';
import { User } from '../../models/user';
import { Car } from '../../models/car';
import { NavBarComponent } from '../nav-bar/nav-bar.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { AddCarDialogComponent } from '../add-car-dialog-component/add-car-dialog-component.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EditUserDialogComponent } from '../edit-user-dialog/edit-user-dialog.component';
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, NavBarComponent,
    MatCardModule,MatButtonModule, MatDialogModule,
    MatIconModule, MatButtonToggleModule, AddCarDialogComponent],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  user: User | null = null;
  cars: Car[] = [];
  userId: number = parseInt(localStorage.getItem('id') || '0', 10); // Replace with actual user ID from auth
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private userService: UserService, private carService: CarService, private dialog: MatDialog,) {}

  ngOnInit(): void {
    this.userService.getUser(this.userId).subscribe(user => {
      this.user = user;
    });
    this.carService.getCars(this.userId).subscribe(cars => {
      this.cars = cars;
    });
  }

  onEditProfile(): void {
    if (!this.user) return;

    const dialogRef = this.dialog.open(EditUserDialogComponent, {
        width: '500px',
        panelClass: 'custom-dialog-container',
        data: this.user, // Pass the current user data to the dialog
    });

    dialogRef.afterClosed().subscribe((updatedUser: User | undefined) => {
        if (updatedUser) {
            this.user = updatedUser; // Update the user with the new data
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
        console.log('Car deleted successfully!');
        this.loadCars(); // Reload the car list after deletion
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
      data: { car }, // "Edit" mode: pass the car data
    });

    dialogRef.componentInstance.isSubmitting = false;
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Car data to update:', result);
        dialogRef.componentInstance.isSubmitting = true;
        this.carService.updateCar(result.id, result).subscribe({
          next: () => {
            console.log('Car updated successfully!');
            this.loadCars(); // Reload the car list after updating
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
}