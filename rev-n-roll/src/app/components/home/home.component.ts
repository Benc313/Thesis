import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../services/user.service';
import { SmallEvent } from '../../models/event';
import { NavBarComponent } from '../nav-bar/nav-bar.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, NavBarComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  events: SmallEvent[] = [];
  userId: number = 1; // Replace with actual user ID from auth

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.userService.getUserMeets(this.userId).subscribe(meets => {
      this.events = meets;
    });
    this.userService.getUserRaces(this.userId).subscribe(races => {
      this.events = [...this.events, ...races];
    });
  }
}