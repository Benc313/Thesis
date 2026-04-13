/**
 * EventCard Component
 * 
 * A reusable card component for displaying event information (meets and races).
 * Supports customization through inputs and provides action slots via content projection.
 * 
 * Features:
 * - Displays event name, date, location, and description
 * - Shows event tags as chips
 * - Supports elevation levels for visual hierarchy
 * - Provides action slots for buttons (edit, delete, view details)
 * - Accessible with ARIA labels
 * - Responsive design
 */

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

/**
 * Base interface for events (meets and races)
 */
export interface EventData {
  id: number;
  name: string;
  description: string;
  location: string;
  date: Date | string;
  tags?: string[];
  creatorName?: string;
  attendeeCount?: number;
  private?: boolean;
}

@Component({
  selector: 'app-event-card',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './event-card.component.html',
  styleUrls: ['./event-card.component.css']
})
export class EventCardComponent {
  /**
   * Event data to display
   */
  @Input({ required: true }) event!: EventData;

  /**
   * Elevation level (0-5) for card shadow
   * Default: 1
   */
  @Input() elevation: number = 1;

  /**
   * Whether to show the card in compact mode (less padding, smaller text)
   * Default: false
   */
  @Input() compact: boolean = false;

  /**
   * Whether to show action buttons
   * Default: true
   */
  @Input() showActions: boolean = true;

  /**
   * Whether the current user can edit this event
   * Default: false
   */
  @Input() canEdit: boolean = false;

  /**
   * Whether the current user can delete this event
   * Default: false
   */
  @Input() canDelete: boolean = false;

  /**
   * Event type for styling (meet or race)
   * Default: 'meet'
   */
  @Input() eventType: 'meet' | 'race' = 'meet';

  /**
   * Emitted when the card is clicked
   */
  @Output() cardClick = new EventEmitter<EventData>();

  /**
   * Emitted when the edit button is clicked
   */
  @Output() editClick = new EventEmitter<EventData>();

  /**
   * Emitted when the delete button is clicked
   */
  @Output() deleteClick = new EventEmitter<EventData>();

  /**
   * Emitted when the view details button is clicked
   */
  @Output() detailsClick = new EventEmitter<EventData>();

  /**
   * Handle card click
   */
  onCardClick(): void {
    this.cardClick.emit(this.event);
  }

  /**
   * Handle edit button click
   * Stops event propagation to prevent card click
   */
  onEditClick(event: Event): void {
    event.stopPropagation();
    this.editClick.emit(this.event);
  }

  /**
   * Handle delete button click
   * Stops event propagation to prevent card click
   */
  onDeleteClick(event: Event): void {
    event.stopPropagation();
    this.deleteClick.emit(this.event);
  }

  /**
   * Handle details button click
   * Stops event propagation to prevent card click
   */
  onDetailsClick(event: Event): void {
    event.stopPropagation();
    this.detailsClick.emit(this.event);
  }

  /**
   * Get formatted date string
   */
  getFormattedDate(): string {
    const date = typeof this.event.date === 'string' 
      ? new Date(this.event.date) 
      : this.event.date;
    
    return date.toLocaleDateString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  /**
   * Get elevation class based on elevation input
   */
  getElevationClass(): string {
    return `elevation-${Math.min(Math.max(this.elevation, 0), 5)}`;
  }

  /**
   * Get event type icon
   */
  getEventTypeIcon(): string {
    return this.eventType === 'race' ? 'sports_score' : 'event';
  }

  /**
   * Get event type color
   */
  getEventTypeColor(): string {
    return this.eventType === 'race' ? 'accent' : 'primary';
  }
}
