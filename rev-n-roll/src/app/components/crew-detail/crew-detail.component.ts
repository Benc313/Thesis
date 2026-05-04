import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CrewService } from '../../services/crew.service';
import { AuthService } from '../../services/auth.service';
import { MeetService } from '../../services/meet.service';
import { RaceService } from '../../services/race.service';
import { Crew, CrewMember } from '../../models/crew';
import { SmallEvent } from '../../models/event';
import { AddMeetDialogComponent } from '../add-meet-dialog/add-meet-dialog.component';
import { AddRaceDialogComponent } from '../add-race-dialog-component/add-race-dialog-component.component';
import { MeetRequest } from '../../models/meet';
import { RaceRequest } from '../../models/race';

@Component({
  selector: 'app-crew-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatSnackBarModule,
    MatSelectModule,
    MatTooltipModule,
  ],
  templateUrl: './crew-detail.component.html',
  styleUrls: ['./crew-detail.component.css']
})
export class CrewDetailComponent implements OnInit {
  crew: Crew | null = null;
  events: SmallEvent[] = [];
  isLoading = true;
  isLoadingEvents = true;
  errorMessage = '';
  crewId!: number;
  userId!: number;

  rankLabels = ['Member', 'Moderator', 'Leader'];
  rankOptions = [
    { value: 0, label: 'Member' },
    { value: 1, label: 'Moderator' },
    { value: 2, label: 'Leader' },
  ];

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private crewService: CrewService,
    private authService: AuthService,
    private meetService: MeetService,
    private raceService: RaceService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.userId = this.authService.getUserId() || 0;
    this.crewId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCrew();
    this.loadEvents();
  }

  loadCrew(): void {
    this.isLoading = true;
    this.crewService.getCrew(this.crewId).subscribe({
      next: (crew) => {
        this.crew = crew;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Crew not found.';
        this.isLoading = false;
      }
    });
  }

  loadEvents(): void {
    this.isLoadingEvents = true;
    this.crewService.getEventsForCrew(this.crewId).subscribe({
      next: (events) => {
        this.events = events;
        this.isLoadingEvents = false;
      },
      error: () => {
        this.isLoadingEvents = false;
      }
    });
  }

  isMember(): boolean {
    return this.crew?.users?.some(u => u.id === this.userId.toString()) || false;
  }

  isLeader(): boolean {
    const me = this.crew?.users?.find(u => u.id === this.userId.toString());
    return me?.rank === 2;
  }

  isModeratorOrLeader(): boolean {
    const me = this.crew?.users?.find(u => u.id === this.userId.toString());
    return me?.rank === 1 || me?.rank === 2;
  }

  getRankLabel(rank: number | undefined): string {
    if (rank === undefined || rank === null) return 'Member';
    return this.rankLabels[rank] || 'Member';
  }

  getRankClass(rank: number | undefined): string {
    switch (rank) {
      case 2: return 'bg-amber-500/20 text-amber-300';
      case 1: return 'bg-blue-500/20 text-blue-300';
      default: return 'bg-slate-600/50 text-slate-300';
    }
  }

  onChangeRank(member: CrewMember, newRank: number): void {
    const userId = parseInt(member.id, 10);
    this.crewService.updateUserRank(this.crewId, userId, newRank).subscribe({
      next: () => {
        this.snackBar.open(`${member.nickname}'s rank updated`, 'Close', { duration: 3000 });
        this.loadCrew();
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to update rank', 'Close', { duration: 3000 });
      }
    });
  }

  removeMember(member: CrewMember): void {
    if (!confirm(`Remove ${member.nickname} from this crew?`)) return;
    const userId = parseInt(member.id, 10);
    this.crewService.removeUserFromCrew(this.crewId, userId).subscribe({
      next: () => {
        this.snackBar.open(`${member.nickname} removed`, 'Close', { duration: 3000 });
        this.loadCrew();
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to remove member', 'Close', { duration: 3000 });
      }
    });
  }

  joinCrew(): void {
    this.crewService.addUserToCrew(this.crewId, this.userId).subscribe({
      next: () => {
        this.snackBar.open('Joined crew!', 'Close', { duration: 3000 });
        this.loadCrew();
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to join crew', 'Close', { duration: 3000 });
      }
    });
  }

  leaveCrew(): void {
    if (!confirm('Are you sure you want to leave this crew?')) return;
    this.crewService.removeUserFromCrew(this.crewId, this.userId).subscribe({
      next: () => {
        this.snackBar.open('Left crew', 'Close', { duration: 3000 });
        this.loadCrew();
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to leave crew', 'Close', { duration: 3000 });
      }
    });
  }

  onCreateMeet(): void {
    const dialogRef = this.dialog.open(AddMeetDialogComponent, {
      width: '600px',
      data: { meet: { crewId: this.crewId } }
    });

    dialogRef.afterClosed().subscribe((result: MeetRequest | undefined) => {
      if (result) {
        result.crewId = this.crewId;
        this.meetService.addMeet(this.userId, result).subscribe({
          next: () => {
            this.snackBar.open('Meet created!', 'Close', { duration: 3000 });
            this.loadEvents();
          },
          error: (err) => {
            this.snackBar.open(err.error?.message || 'Failed to create meet', 'Close', { duration: 3000 });
          }
        });
      }
    });
  }

  onCreateRace(): void {
    const dialogRef = this.dialog.open(AddRaceDialogComponent, {
      width: '600px',
      data: { race: { crewId: this.crewId } }
    });

    dialogRef.afterClosed().subscribe((result: RaceRequest | undefined) => {
      if (result) {
        result.crewId = this.crewId;
        this.raceService.addRace(result).subscribe({
          next: () => {
            this.snackBar.open('Race created!', 'Close', { duration: 3000 });
            this.loadEvents();
          },
          error: (err) => {
            this.snackBar.open(err.error?.message || 'Failed to create race', 'Close', { duration: 3000 });
          }
        });
      }
    });
  }

  viewEvent(event: SmallEvent): void {
    if (event.isMeet) {
      this.router.navigate(['/meets'], { queryParams: { view: event.id } });
    } else {
      this.router.navigate(['/races'], { queryParams: { view: event.id } });
    }
  }
}
