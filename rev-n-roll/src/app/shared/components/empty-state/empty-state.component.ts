/**
 * EmptyState Component
 * 
 * A reusable component for displaying empty states with helpful messages and actions.
 * 
 * Features:
 * - Customizable icon, title, and message
 * - Optional primary action button
 * - Accessible with ARIA labels
 * - Responsive design
 */

import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './empty-state.component.html',
  styleUrls: ['./empty-state.component.css']
})
export class EmptyStateComponent {
  /**
   * Icon to display (Material icon name)
   * Default: 'inbox'
   */
  @Input() icon: string = 'inbox';

  /**
   * Title text
   * Default: 'No items found'
   */
  @Input() title: string = 'No items found';

  /**
   * Description message
   * Default: 'There are no items to display at this time.'
   */
  @Input() message: string = 'There are no items to display at this time.';

  /**
   * Primary action button text
   * If not provided, button won't be shown
   */
  @Input() actionText?: string;

  /**
   * Primary action button icon
   */
  @Input() actionIcon?: string;

  /**
   * Whether to show the illustration
   * Default: true
   */
  @Input() showIllustration: boolean = true;

  /**
   * Emitted when the action button is clicked
   */
  @Output() actionClick = new EventEmitter<void>();

  /**
   * Handle action button click
   */
  onActionClick(): void {
    this.actionClick.emit();
  }
}
