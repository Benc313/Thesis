// rev-n-roll/src/app/services/crew.service.ts

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Crew, CrewRequest, UserCrewRequest, Rank } from '../models/crew';
import { AuthService } from './auth.service';
import { SmallEvent } from '../models/event'; // For crew events

@Injectable({
  providedIn: 'root'
})
export class CrewService {
  private apiUrl = 'http://localhost:5123/api/v1/crews';

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
    // GET /api/v1/crews/{id}
    return this.http.get<Crew>(`${this.apiUrl}/${crewId}`, { headers: this.getHeaders() });
  }

  createCrew(crewRequest: CrewRequest, userId: number): Observable<Crew> {
    let headers = this.getHeaders();
    headers = headers.set('User-Id', userId.toString());
    return this.http.post<Crew>(`${this.apiUrl}`, crewRequest, { headers: headers });
  }

  updateCrew(crewId: number, crewRequest: CrewRequest): Observable<Crew> {
    // PUT /api/v1/crews/{id}
    return this.http.put<Crew>(`${this.apiUrl}/${crewId}`, crewRequest, { headers: this.getHeaders() });
  }

  deleteCrew(crewId: number): Observable<void> {
    // DELETE /api/v1/crews/{id}
    return this.http.delete<void>(`${this.apiUrl}/${crewId}`, { headers: this.getHeaders() });
  }
  

  getCrewEvents(crewId: number): Observable<SmallEvent[]> {
    return this.http.get<SmallEvent[]>(`${this.apiUrl}/${crewId}/events`, { headers: this.getHeaders() });
  }


  addUserToCrew(crewId: number, userId: number): Observable<void> {
    const userCrewRequest: UserCrewRequest = { userId: userId, rank: Rank.Member }; 
    return this.http.post<void>(`${this.apiUrl}/${crewId}/users`, userCrewRequest, { headers: this.getHeaders() });
  }


  updateUserRank(crewId: number, userId: number, newRank: Rank): Observable<void> {
    const userCrewRequest: UserCrewRequest = { userId: userId, rank: newRank }; 
    return this.http.put<void>(`${this.apiUrl}/${crewId}/users/${userId}`, userCrewRequest, { headers: this.getHeaders() });
  }


  removeUserFromCrew(crewId: number, userId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${crewId}/users/${userId}`, { headers: this.getHeaders() });
  }
}