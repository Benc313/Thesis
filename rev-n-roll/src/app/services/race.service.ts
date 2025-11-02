import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Race, RaceRequest } from '../models/race';
import { SmallEvent } from '../models/event';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class RaceService {
  private apiUrl = 'http://localhost:5123/api/v1/races'; // Base URL for RaceController

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      // The backend expects 'User-Id' for CreateRace if Auth is commented out, 
      // but in a fully authenticated app, only JWT (implied by withCredentials or Bearer token) is needed.
      // For consistency with MeetService pattern, we'll ensure a header is provided if getToken() returns a token.
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  addRace(race: RaceRequest): Observable<Race> {
    // The backend RaceController expects the userId from the 'User-Id' header if not authorized.
    // For this mock-authenticated setup, we'll explicitly add the User-Id header as well.
    const userId = this.authService.getUserId();
    const headers = this.getHeaders().set('User-Id', userId ? userId.toString() : '0');

    // POST /api/v1/races
    return this.http.post<Race>(this.apiUrl, race, { headers });
  }

  updateRace(raceId: number, race: RaceRequest): Observable<Race> {
    // PUT /api/v1/races/{id}
    return this.http.put<Race>(`${this.apiUrl}/${raceId}`, race, { headers: this.getHeaders() });
  }

  getRace(raceId: number): Observable<Race> {
    // GET /api/v1/races/{id}
    return this.http.get<Race>(`${this.apiUrl}/${raceId}`, { headers: this.getHeaders() });
  }

  deleteRace(raceId: number): Observable<void> {
    // DELETE /api/v1/races/{id}
    return this.http.delete<void>(`${this.apiUrl}/${raceId}`, { headers: this.getHeaders() });
  }
  
  // POST /api/v1/races/{raceId}/join/{userId}
  joinRace(raceId: number, userId: number): Observable<void> {
    // Note: The backend endpoint is POST /races/{raceId}/join/{userId} which is a bit non-standard for a POST with path params
    // The JoinRace controller method expects raceId and userId as path segments: POST "{raceId}/join/{userId}"
    return this.http.post<void>(`${this.apiUrl}/${raceId}/join/${userId}`, {}, { headers: this.getHeaders() });
  }

  // GET /api/v1/races/getMeetsF (Used for filtered list, which returns SmallEventResponse with isMeet: false)
  getFilteredRaces(latitude?: number, longitude?: number, distanceInKm?: number): Observable<SmallEvent[]> {
    let params = new HttpParams();
    if (latitude) params = params.set('Latitude', latitude.toFixed(6));
    if (longitude) params = params.set('Longitude', longitude.toFixed(6));
    if (distanceInKm) params = params.set('DistanceInKm', distanceInKm.toString());
    params = params.set('Tags', ''); 

    // The backend uses a generic "getMeetsF" endpoint name but it's configured for races in RaceController.cs
    return this.http.get<SmallEvent[]>(`${this.apiUrl}/getMeetsF`, { headers: this.getHeaders(), params });
  }
}