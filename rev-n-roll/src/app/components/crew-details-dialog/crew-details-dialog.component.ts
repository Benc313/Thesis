// rev-n-roll/src/app/components/crew-details-dialog/crew-details-dialog.component.ts

import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Crew, Rank, CrewMember } from '../../models/crew'; // Added CrewMember
import { SmallEvent } from '../../models/event';
import { CrewService } from '../../services/crew.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-crew-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule
  ],
  templateUrl: './crew-details-dialog.component.html',
  styleUrls: ['./crew-details-dialog.component.css'] 
})
export class CrewDetailsDialogComponent implements OnInit {
  crew: Crew | null = null;
  events: SmallEvent[] = [];
  isLoading = true;
  isMember = false;
  isLeader = false; // Assuming only Leader has full management control for UI purposes
  currentUserId: number = 0;
  
  ranks = Object.keys(Rank).filter(key => isNaN(Number(key))); // ['Member', 'Moderator', 'Leader']
  RankEnum = Rank; 
  
  newMemberId: string = ''; // Used for manual ID entry to invite

  constructor(
    public dialogRef: MatDialogRef<CrewDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { crewId: number },
    private crewService: CrewService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {
    this.currentUserId = this.authService.getUserId() || 0;
  }

  ngOnInit(): void {
    this.loadCrewDetails();
    this.loadCrewEvents();
  }
  
  loadCrewDetails(): void {
    this.isLoading = true;
    this.crewService.getCrew(this.data.crewId).subscribe({
      next: (crew) => {
        this.crew = crew;
        
        const currentUser = crew.users.find(member => parseInt(member.id, 10) === this.currentUserId);
        
        this.isMember = !!currentUser;
        // Check if the current user is a Leader (assuming rank is available on CrewMember)
        // NOTE: If BE doesn't return rank on member, this will be incorrect. Assuming Leader = Rank.Leader=2.
        this.isLeader = currentUser ? currentUser.rank === Rank.Leader : false;
        
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading crew details:', err);
        this.snackBar.open('Failed to load crew details.', 'Close', { duration: 3000, panelClass: ['error-snackbar'] });
        this.isLoading = false;
      }
    });
  }

  loadCrewEvents(): void {
    this.crewService.getCrewEvents(this.data.crewId).subscribe({
        next: (events) => {
            this.events = events;
        },
        error: (err) => {
            if (err.status !== 404) {
                 this.snackBar.open('Failed to load crew events.', 'Close', { duration: 3000, panelClass: ['error-snackbar'] });
                 console.error('Error loading crew events:', err);
            }
            this.events = [];
        }
    });
  }

  getRankName(rank: number): string {
    return Rank[rank] || 'Unknown';
  }

  // --- MANAGEMENT ACTIONS ---

  onToggleMembership(): void {
    if (this.currentUserId === 0 || !this.crew) return;

    if (this.isMember) {
      if (!confirm('Are you sure you want to leave this crew?')) return;
      this.crewService.removeUserFromCrew(this.crew.id, this.currentUserId).subscribe({
        next: () => {
          this.snackBar.open('Successfully left the crew.', 'Close', { duration: 3000, panelClass: ['success-snackbar'] });
          this.loadCrewDetails(); 
        },
        error: (err) => this.snackBar.open(err.error?.message || 'Failed to leave crew.', 'Close', { duration: 5000, panelClass: ['error-snackbar'] })
      });
    } else {
      this.crewService.addUserToCrew(this.crew.id, this.currentUserId).subscribe({
        next: () => {
          this.snackBar.open('Successfully joined the crew!', 'Close', { duration: 3000, panelClass: ['success-snackbar'] });
          this.loadCrewDetails(); 
        },
        error: (err) => this.snackBar.open(err.error?.message || 'Failed to join crew.', 'Close', { duration: 5000, panelClass: ['error-snackbar'] })
      });
    }
  }

  onUpdateRank(memberId: string, event: MatSelectChange): void {
    if (!this.crew || !this.isLeader) return; // Only leader can promote/demote
    
    const newRankValue: Rank = event.value;
    const memberNumericId = parseInt(memberId, 10);
    
    this.crewService.updateUserRank(this.crew.id, memberNumericId, newRankValue).subscribe({
      next: () => {
        this.snackBar.open(`Rank updated to ${this.getRankName(newRankValue)}!`, 'Close', { duration: 3000, panelClass: ['success-snackbar'] });
        this.loadCrewDetails(); 
      },
      error: (err) => {
        const message = err.error?.message || 'Failed to update rank. Check your permissions (must be Leader).';
        this.snackBar.open(message, 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
      }
    });
  }
  
  onRemoveMember(memberId: string): void {
    if (!this.crew || !this.isLeader || !confirm('Are you sure you want to remove this member?')) return;
    
    const memberNumericId = parseInt(memberId, 10);

    this.crewService.removeUserFromCrew(this.crew.id, memberNumericId).subscribe({
      next: () => {
        this.snackBar.open('Member removed successfully!', 'Close', { duration: 3000, panelClass: ['success-snackbar'] });
        this.loadCrewDetails(); 
      },
      error: (err) => {
        const message = err.error?.message || 'Failed to remove member. Check your permissions (must be Leader).';
        this.snackBar.open(message, 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
      }
    });
  }

  onAddMember(): void {
    if (!this.crew || !this.newMemberId || !this.isLeader) return;
    
    const memberNumericId = parseInt(this.newMemberId, 10);
    if (isNaN(memberNumericId)) {
        this.snackBar.open('Please enter a valid numeric User ID to invite.', 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
        return;
    }

    this.crewService.addUserToCrew(this.crew.id, memberNumericId).subscribe({
      next: () => {
        this.snackBar.open(`User ${memberNumericId} invited (added) successfully!`, 'Close', { duration: 3000, panelClass: ['success-snackbar'] });
        this.newMemberId = '';
        this.loadCrewDetails();
      },
      error: (err) => {
        const message = err.error?.message || 'Failed to add user. They might already be a member.';
        this.snackBar.open(message, 'Close', { duration: 5000, panelClass: ['error-snackbar'] });
      }
    });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}