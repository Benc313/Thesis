import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MeetService } from '../../services/meet.service';
import { AuthService } from '../../services/auth.service';
import { Meet, MeetTags } from '../../models/meet';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { GoogleMapsModule } from '@angular/google-maps';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-meet-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    GoogleMapsModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './meet-details-dialog.component.html',
  styleUrls: ['./meet-details-dialog.component.css']
})
export class MeetDetailsDialogComponent implements OnInit {
  meet: Meet | null = null;
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
    public dialogRef: MatDialogRef<MeetDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { meetId: number },
    private meetService: MeetService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.getUserId(); // Assuming AuthService has a method to get the current user's ID
    this.loadMeetDetails();
  }

  loadMeetDetails(): void {
    this.isLoading = true;
    this.meetService.getMeet(this.data.meetId).subscribe({
      next: (meet) => {
        console.log('Meet details loaded:', meet);
        this.meet = meet;
        this.isLoading = false;
        this.setMapPosition();
        this.hasJoined = meet.users.some(user => user.id === this.currentUserId);
      },
      error: (err) => {
        console.error('Error loading meet details:', err);
        this.isLoading = false;
      }
    });
  }

  setMapPosition(): void {
    if (this.meet?.coordinates) {
      const [lat, lng] = this.meet.coordinates.split(',').map(coord => parseFloat(coord.trim()));
      this.markerPosition = { lat, lng };
      this.mapOptions.center = this.markerPosition;
    }
  }

  joinMeet(): void {
    if (!this.currentUserId) return;

    this.isSubmitting = true;
    this.meetService.joinMeet(this.data.meetId, this.currentUserId).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.hasJoined = true;
        this.loadMeetDetails(); // Refresh meet details to update the users list
      },
      error: (err) => {
        console.error('Error joining meet:', err);
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

  getTagsDisplay(tags: MeetTags[] | undefined): string {
    if (!tags || tags.length === 0) return 'No tags';
    return tags.map(tag => MeetTags[tag]).join(', ');
  }
}