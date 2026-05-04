/**
 * Global Error Handler
 * 
 * Centralized error handling for the entire application.
 * Catches all unhandled errors and provides user-friendly feedback.
 * 
 * Features:
 * - User-friendly error messages
 * - Error logging to console and external services
 * - Toast notifications for errors
 * - HTTP error handling
 * - Network error detection
 */

import { ErrorHandler, Injectable, Injector } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatSnackBar } from '@angular/material/snack-bar';

/**
 * Error severity levels
 */
export enum ErrorSeverity {
  INFO = 'info',
  WARNING = 'warning',
  ERROR = 'error',
  CRITICAL = 'critical'
}

/**
 * Structured error information
 */
export interface ErrorInfo {
  message: string;
  severity: ErrorSeverity;
  timestamp: Date;
  stack?: string;
  context?: any;
}

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private snackBar!: MatSnackBar;

  constructor(private injector: Injector) {
    // Lazy inject MatSnackBar to avoid circular dependency
    setTimeout(() => {
      this.snackBar = this.injector.get(MatSnackBar);
    });
  }

  /**
   * Handle all unhandled errors
   */
  handleError(error: Error | HttpErrorResponse): void {
    const errorInfo = this.processError(error);
    
    // Log error to console
    this.logError(errorInfo);
    
    // Send error to external logging service (e.g., Sentry)
    this.sendToLoggingService(errorInfo);
    
    // Show user-friendly notification
    this.showErrorNotification(errorInfo);
  }

  /**
   * Process error and extract relevant information
   */
  private processError(error: Error | HttpErrorResponse): ErrorInfo {
    if (error instanceof HttpErrorResponse) {
      return this.processHttpError(error);
    } else {
      return this.processClientError(error);
    }
  }

  /**
   * Process HTTP errors
   */
  private processHttpError(error: HttpErrorResponse): ErrorInfo {
    let message: string;
    let severity: ErrorSeverity;

    // Network error (no response from server)
    if (error.status === 0) {
      message = 'Unable to connect to the server. Please check your internet connection.';
      severity = ErrorSeverity.ERROR;
    }
    // Client errors (4xx)
    else if (error.status >= 400 && error.status < 500) {
      message = this.getClientErrorMessage(error.status, error);
      severity = ErrorSeverity.WARNING;
    }
    // Server errors (5xx)
    else if (error.status >= 500) {
      message = 'A server error occurred. Please try again later.';
      severity = ErrorSeverity.ERROR;
    }
    // Other errors
    else {
      message = 'An unexpected error occurred. Please try again.';
      severity = ErrorSeverity.ERROR;
    }

    return {
      message,
      severity,
      timestamp: new Date(),
      stack: error.error?.stack,
      context: {
        url: error.url,
        status: error.status,
        statusText: error.statusText,
        error: error.error
      }
    };
  }

  /**
   * Get user-friendly message for client errors
   */
  private getClientErrorMessage(status: number, error: HttpErrorResponse): string {
    switch (status) {
      case 400:
        return error.error?.message || 'Invalid request. Please check your input.';
      case 401:
        return 'Your session has expired. Please log in again.';
      case 403:
        return 'You don\'t have permission to perform this action.';
      case 404:
        return 'The requested resource was not found.';
      case 409:
        return error.error?.message || 'A conflict occurred. The resource may already exist.';
      case 422:
        return error.error?.message || 'Validation failed. Please check your input.';
      case 429:
        return 'Too many requests. Please slow down and try again later.';
      default:
        return error.error?.message || 'An error occurred. Please try again.';
    }
  }

  /**
   * Process client-side errors
   */
  private processClientError(error: Error): ErrorInfo {
    let message: string;
    let severity: ErrorSeverity;

    // Check for specific error types
    if (error.name === 'ChunkLoadError') {
      message = 'Failed to load application resources. Please refresh the page.';
      severity = ErrorSeverity.WARNING;
    } else if (error.message?.includes('timeout')) {
      message = 'The request timed out. Please try again.';
      severity = ErrorSeverity.WARNING;
    } else {
      message = 'An unexpected error occurred. Please refresh the page.';
      severity = ErrorSeverity.ERROR;
    }

    return {
      message,
      severity,
      timestamp: new Date(),
      stack: error.stack,
      context: {
        name: error.name,
        originalMessage: error.message
      }
    };
  }

  /**
   * Log error to console
   */
  private logError(errorInfo: ErrorInfo): void {
    const logMethod = this.getLogMethod(errorInfo.severity);
    
    console.group(`🚨 ${errorInfo.severity.toUpperCase()} - ${errorInfo.timestamp.toISOString()}`);
    console[logMethod]('Message:', errorInfo.message);
    if (errorInfo.stack) {
      console[logMethod]('Stack:', errorInfo.stack);
    }
    if (errorInfo.context) {
      console[logMethod]('Context:', errorInfo.context);
    }
    console.groupEnd();
  }

  /**
   * Get appropriate console log method based on severity
   */
  private getLogMethod(severity: ErrorSeverity): 'log' | 'warn' | 'error' {
    switch (severity) {
      case ErrorSeverity.INFO:
        return 'log';
      case ErrorSeverity.WARNING:
        return 'warn';
      case ErrorSeverity.ERROR:
      case ErrorSeverity.CRITICAL:
        return 'error';
      default:
        return 'error';
    }
  }

  /**
   * Send error to external logging service
   * TODO: Integrate with Sentry, LogRocket, or similar service
   */
  private sendToLoggingService(errorInfo: ErrorInfo): void {
    // Only send errors and critical issues to external service
    if (errorInfo.severity === ErrorSeverity.ERROR || 
        errorInfo.severity === ErrorSeverity.CRITICAL) {
      
      // Example: Sentry integration
      // if (window.Sentry) {
      //   window.Sentry.captureException(errorInfo, {
      //     level: errorInfo.severity,
      //     extra: errorInfo.context
      //   });
      // }

      // For now, just log that we would send it
      console.log('📤 Would send to logging service:', errorInfo);
    }
  }

  /**
   * Show error notification to user
   */
  private showErrorNotification(errorInfo: ErrorInfo): void {
    if (!this.snackBar) {
      console.warn('MatSnackBar not available yet');
      return;
    }

    const config = {
      duration: this.getNotificationDuration(errorInfo.severity),
      horizontalPosition: 'center' as const,
      verticalPosition: 'top' as const,
      panelClass: this.getNotificationClass(errorInfo.severity)
    };

    this.snackBar.open(errorInfo.message, 'Dismiss', config);
  }

  /**
   * Get notification duration based on severity
   */
  private getNotificationDuration(severity: ErrorSeverity): number {
    switch (severity) {
      case ErrorSeverity.INFO:
        return 3000;
      case ErrorSeverity.WARNING:
        return 5000;
      case ErrorSeverity.ERROR:
        return 7000;
      case ErrorSeverity.CRITICAL:
        return 10000;
      default:
        return 5000;
    }
  }

  /**
   * Get CSS class for notification based on severity
   */
  private getNotificationClass(severity: ErrorSeverity): string[] {
    const baseClass = 'error-snackbar';
    const severityClass = `error-snackbar-${severity}`;
    return [baseClass, severityClass];
  }
}
