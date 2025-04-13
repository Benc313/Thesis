import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User, UserRequest } from '../models/user';
import { SmallEvent } from '../models/event';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5000/api/v1/User'; // Replace with your backend URL

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getUser(userId: number): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/getUser/${userId}`, { headers: this.getHeaders() });
  }

  updateUser(userId: number, user: UserRequest): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/updateUser/${userId}`, user, { headers: this.getHeaders() });
  }

  getUserMeets(userId: number): Observable<SmallEvent[]> {
    return this.http.get<SmallEvent[]>(`${this.apiUrl}/GetUserMeets/${userId}`, { headers: this.getHeaders() });
  }

  getUserRaces(userId: number): Observable<SmallEvent[]> {
    return this.http.get<SmallEvent[]>(`${this.apiUrl}/GetUserRaces/${userId}`, { headers: this.getHeaders() });
  }
}