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
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, NavBarComponent,
    MatCardModule,MatButtonModule,
    MatIconModule, MatButtonToggleModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  user: User | null = null;
  cars: Car[] = [];
  userId: number = parseInt(localStorage.getItem('id') || '0', 10); // Replace with actual user ID from auth

  constructor(private userService: UserService, private carService: CarService) {}

  ngOnInit(): void {
    this.userService.getUser(this.userId).subscribe(user => {
      this.user = user;
    });
    this.carService.getCars(this.userId).subscribe(cars => {
      this.cars = cars;
    });
  }

  onEditProfile(): void {
    // Logic to edit the profile
    console.log('Edit Profile button clicked');
  }
  
  onAddCar(): void {
    // Logic to add a car
    console.log('Add a Car button clicked');
  }
}