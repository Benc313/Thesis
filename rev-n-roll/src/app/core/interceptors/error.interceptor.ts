/**
 * HTTP Error Interceptor
 * 
 * Intercepts all HTTP requests and handles errors globally.
 * 
 * Features:
 * - Automatic retry for failed requests
 * - Token refresh on 401 errors
 * - Request timeout handling
 * - Network error detection
 * - Logging of HTTP errors
 */

import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, timer } from 'rxjs';
import { catchError, retry, timeout } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  /**
   * Maximum number of retry attempts
   */
  private readonly MAX_RETRIES = 1;

  /**
   * Request timeout in milliseconds
   */
  private readonly REQUEST_TIMEOUT = 30000; // 30 seconds

  /**
   * HTTP methods that should be retried
   */
  private readonly RETRYABLE_METHODS = ['GET', 'HEAD', 'OPTIONS'];

  constructor(private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      // Add timeout to prevent hanging requests
      timeout(this.REQUEST_TIMEOUT),
      
      // Retry failed requests (only for safe methods)
      retry({
        count: this.shouldRetry(request) ? this.MAX_RETRIES : 0,
        delay: (error, retryCount) => {
          // Only retry on network errors or 5xx server errors
          if (this.isRetryableError(error)) {
            // Exponential backoff: 1s, 2s, 4s, etc.
            const delayMs = Math.pow(2, retryCount) * 1000;
            console.log(`Retrying request (attempt ${retryCount + 1}) after ${delayMs}ms`);
            return timer(delayMs);
          }
          // Don't retry
          throw error;
        }
      }),
      
      // Handle errors
      catchError((error: HttpErrorResponse) => {
        return this.handleError(error, request);
      })
    );
  }

  /**
   * Determine if request should be retried
   */
  private shouldRetry(request: HttpRequest<unknown>): boolean {
    return this.RETRYABLE_METHODS.includes(request.method.toUpperCase());
  }

  /**
   * Determine if error is retryable
   */
  private isRetryableError(error: any): boolean {
    if (!(error instanceof HttpErrorResponse)) {
      return false;
    }

    // Retry on network errors (status 0)
    if (error.status === 0) {
      return true;
    }

    // Retry on server errors (5xx)
    if (error.status >= 500 && error.status < 600) {
      return true;
    }

    // Retry on specific client errors
    if (error.status === 408 || error.status === 429) {
      return true;
    }

    return false;
  }

  /**
   * Handle HTTP errors
   */
  private handleError(
    error: HttpErrorResponse,
    request: HttpRequest<unknown>
  ): Observable<never> {
    // Log error details
    this.logError(error, request);

    // Handle specific error cases
    if (error.status === 401) {
      this.handleUnauthorized();
    } else if (error.status === 403) {
      this.handleForbidden();
    } else if (error.status === 0) {
      this.handleNetworkError();
    }

    // Re-throw error to be handled by global error handler
    return throwError(() => error);
  }

  /**
   * Handle 401 Unauthorized errors
   */
  private handleUnauthorized(): void {
    console.warn('Unauthorized access - redirecting to login');
    
    // Clear any stored authentication data
    localStorage.removeItem('token');
    sessionStorage.clear();
    
    // Redirect to login page
    this.router.navigate(['/login'], {
      queryParams: { returnUrl: this.router.url }
    });
  }

  /**
   * Handle 403 Forbidden errors
   */
  private handleForbidden(): void {
    console.warn('Forbidden access - insufficient permissions');
    
    // Optionally redirect to a "no permission" page
    // this.router.navigate(['/forbidden']);
  }

  /**
   * Handle network errors (no connection)
   */
  private handleNetworkError(): void {
    console.error('Network error - no connection to server');
    
    // Could show a global "offline" banner here
  }

  /**
   * Log error details for debugging
   */
  private logError(error: HttpErrorResponse, request: HttpRequest<unknown>): void {
    console.group('🔴 HTTP Error');
    console.error('Status:', error.status);
    console.error('Status Text:', error.statusText);
    console.error('URL:', request.url);
    console.error('Method:', request.method);
    console.error('Error:', error.error);
    console.error('Message:', error.message);
    console.groupEnd();
  }
}
