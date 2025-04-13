import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MeetService } from '../../services/meet.service';
import { SmallEvent } from '../../models/event';
import { NavBarComponent } from '../nav-bar/nav-bar.component';

@Component({
  selector: 'app-meets',
  standalone: true,
  imports: [CommonModule, NavBarComponent],
  templateUrl: './meets.component.html',
  styleUrls: ['./meets.component.css']
})
export class MeetsComponent implements OnInit {
  meets: SmallEvent[] = [];

  constructor(private meetService: MeetService) {}

  ngOnInit(): void {
    this.meetService.getMeets().subscribe(meets => {
      this.meets = meets.filter(event => event.isMeet);
    });
  }
}