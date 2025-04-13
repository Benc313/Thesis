import { Component } from '@angular/core';
import { NavBarComponent } from '../nav-bar/nav-bar.component';

@Component({
  selector: 'app-crews',
  standalone: true,
  imports: [NavBarComponent],
  templateUrl: './crews.component.html',
  styleUrls: ['./crews.component.css']
})
export class CrewsComponent {}