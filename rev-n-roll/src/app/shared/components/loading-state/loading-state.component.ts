/**
 * LoadingState Component
 * 
 * A reusable loading state component that displays skeleton screens
 * instead of generic spinners for better user experience.
 * 
 * Features:
 * - Multiple skeleton types (card, list, table, form)
 * - Shimmer animation effect
 * - Customizable count and layout
 * - Accessible with ARIA labels
 * - Respects prefers-reduced-motion
 */

import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SkeletonType = 'card' | 'list' | 'table' | 'form' | 'profile' | 'event-card';

@Component({
  selector: 'app-loading-state',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-state.component.html',
  styleUrls: ['./loading-state.component.css']
})
export class LoadingStateComponent {
  /**
   * Type of skeleton to display
   * Default: 'card'
   */
  @Input() type: SkeletonType = 'card';

  /**
   * Number of skeleton items to display
   * Default: 3
   */
  @Input() count: number = 3;

  /**
   * Whether to show the shimmer animation
   * Default: true
   */
  @Input() shimmer: boolean = true;

  /**
   * Custom loading message for screen readers
   * Default: 'Loading content...'
   */
  @Input() ariaLabel: string = 'Loading content...';

  /**
   * Layout direction (horizontal or vertical)
   * Default: 'vertical'
   */
  @Input() layout: 'horizontal' | 'vertical' = 'vertical';

  /**
   * Get array for ngFor iteration
   */
  getCountArray(): number[] {
    return Array(this.count).fill(0).map((_, i) => i);
  }
}
