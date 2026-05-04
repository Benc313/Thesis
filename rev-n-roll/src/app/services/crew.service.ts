// rev-n-roll/src/app/services/crew.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Crew, CrewRequest, Rank } from '../models/crew';
import { SmallEvent } from '../models/event';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class CrewService {
  private apiUrl = 'http://localhost:5000/api/v1/crews';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getAllCrews(): Observable<Crew[]> {
    return this.http.get<Crew[]>(`${this.apiUrl}`, { headers: this.getHeaders() });
  }

  getCrew(crewId: number): Observable<Crew> {
    return this.http.get<Crew>(`${this.apiUrl}/${crewId}`, { headers: this.getHeaders() });
  }

  createCrew(crewRequest: CrewRequest, userId: number): Observable<Crew> {
    // NOTE: The backend CrewController.cs requires the 'User-Id' header for POST/CreateCrew.
    let headers = this.getHeaders();
    headers = headers.set('User-Id', userId.toString());
    return this.http.post<Crew>(`${this.apiUrl}`, crewRequest, { headers: headers });
  }

  updateCrew(crewId: number, crewRequest: CrewRequest): Observable<Crew> {
    return this.http.put<Crew>(`${this.apiUrl}/${crewId}`, crewRequest, { headers: this.getHeaders() });
  }

  deleteCrew(crewId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${crewId}`, { headers: this.getHeaders() });
  }

  addUserToCrew(crewId: number, userId: number): Observable<void> {
    const userCrewRequest = { userId: userId, rank: Rank.Member };
    return this.http.post<void>(`${this.apiUrl}/${crewId}/users`, userCrewRequest, { headers: this.getHeaders() });
  }

  removeUserFromCrew(crewId: number, userId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${crewId}/users/${userId}`, { headers: this.getHeaders() });
  }

  getEventsForCrew(crewId: number): Observable<SmallEvent[]> {
    return this.http.get<SmallEvent[]>(`${this.apiUrl}/${crewId}/events`, { headers: this.getHeaders() });
  }

  getUserCrews(userId: number): Observable<Crew[]> {
    return this.http.get<Crew[]>(`${this.apiUrl}/user/${userId}`, { headers: this.getHeaders() });
  }

  updateUserRank(crewId: number, userId: number, rank: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${crewId}/users/${userId}?rank=${rank}`, {}, { headers: this.getHeaders() });
  }
}
