import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { RaceService } from '../../services/race.service';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { Race, RaceType } from '../../models/race';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { GoogleMapsModule } from '@angular/google-maps';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  // --- CORRECTED SELECTOR FOR CONSISTENCY ---
  selector: 'app-race-details-dialog-component',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    GoogleMapsModule,
    MatProgressSpinnerModule,
  ],
  // --- CORRECTED PATHS START ---
  templateUrl: './race-details-dialog-component.component.html',
  styleUrls: ['./race-details-dialog-component.component.css']
  // --- CORRECTED PATHS END ---
})
export class RaceDetailsDialogComponent implements OnInit {
  race: Race | null = null;
  isLoading = true;
  mapOptions: google.maps.MapOptions = {
    zoom: 15,
    mapTypeControl: false,
    streetViewControl: false,
    fullscreenControl: false,
  };
  markerPosition: google.maps.LatLngLiteral | null = null;
  isSubmitting = false;
  hasJoined = false;
  currentUserId: number | null = null;

  constructor(
    // --- CORRECTED DIALOG REF TYPE ---
    public dialogRef: MatDialogRef<RaceDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { raceId: number },
    private raceService: RaceService,
    private authService: AuthService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.getUserId(); 
    this.loadRaceDetails();
  }

  loadRaceDetails(): void {
    this.isLoading = true;
    this.raceService.getRace(this.data.raceId).subscribe({
      next: (race) => {
        this.race = race;
        this.isLoading = false;
        this.setMapPosition();
        this.hasJoined = race.users.some(user => user.id === this.currentUserId);
      },
      error: (err) => {
        console.error('Error loading race details:', err);
        this.isLoading = false;
      }
    });
  }

  setMapPosition(): void {
    if (this.race?.coordinates) {
      const [lat, lng] = this.race.coordinates.split(',').map(coord => parseFloat(coord.trim()));
      this.markerPosition = { lat, lng };
      this.mapOptions.center = this.markerPosition;
    }
  }

  joinRace(): void {
    if (!this.currentUserId || !this.race?.id) return;

    this.isSubmitting = true;
    this.raceService.joinRace(this.race.id, this.currentUserId).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.hasJoined = true;
        this.loadRaceDetails(); // Refresh race details to update the users list
      },
      error: (err) => {
        console.error('Error joining race:', err);
        this.isSubmitting = false;
      }
    });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  getFormattedDate(date: string | undefined): string {
    if (!date) return 'Date not available';
    const d = new Date(date);
    return d.toLocaleString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getRaceTypeDisplay(raceType: RaceType | undefined): string {
    if (raceType === undefined) return 'No type';
    // Access enum string name via numeric value
    return RaceType[raceType];
  }
}
