import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5123/api/v1/Authentication'; // Replace with your backend URL
  private isAuthenticated = false; // Track authentication state

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { email, password }, {withCredentials: true}).pipe(
      tap((response: any) => {
        console.log('Login response:', response); // Debug the response
        this.isAuthenticated = true; // Set authentication state
        localStorage.setItem('id', response.id); // Store user ID in localStorage
      })
    );
  }

  register(email: string, nickname: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, { email, nickname, password });
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated; // Use the in-memory state instead of localStorage
  }

  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}).subscribe(() => {
      this.isAuthenticated = false; // Reset authentication state
      localStorage.removeItem('id'); // Remove user ID from localStorage
    });
  }

  getUserId(): number | null {
    return parseInt(localStorage.getItem('id') || '0', 10); // Get user ID from localStorage
  }


  getToken(): string | null {
    return localStorage.getItem('token');
  }
}