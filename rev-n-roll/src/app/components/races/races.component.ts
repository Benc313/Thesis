import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MeetService } from '../../services/meet.service';
import { SmallEvent } from '../../models/event';
import { NavBarComponent } from '../nav-bar/nav-bar.component';

@Component({
  selector: 'app-races',
  standalone: true,
  imports: [CommonModule, NavBarComponent],
  templateUrl: './races.component.html',
  styleUrls: ['./races.component.css']
})
export class RacesComponent implements OnInit {
  races: SmallEvent[] = [];

  constructor(private meetService: MeetService) {}

  ngOnInit(): void {
    this.meetService.getMeets().subscribe(events => {
      this.races = events.filter(event => !event.isMeet);
    });
  }
}