import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Meet, MeetRequest } from '../models/meet';
import { SmallEvent } from '../models/event';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MeetService {
  private apiUrl = 'http://localhost:5000/api/v1/Meet'; // Replace with your backend URL

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  addMeet(userId: number, meet: MeetRequest): Observable<Meet> {
    return this.http.post<Meet>(`${this.apiUrl}/addMeet/${userId}`, meet, { headers: this.getHeaders() });
  }

  updateMeet(meetId: number, meet: MeetRequest): Observable<Meet> {
    return this.http.put<Meet>(`${this.apiUrl}/updateMeet/${meetId}`, meet, { headers: this.getHeaders() });
  }

  getMeet(meetId: number): Observable<Meet> {
    return this.http.get<Meet>(`${this.apiUrl}/getMeet/${meetId}`, { headers: this.getHeaders() });
  }

  deleteMeet(meetId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/deleteMeet/${meetId}`, { headers: this.getHeaders() });
  }

  getMeets(latitude?: number, longitude?: number, distanceInKm?: number, tags?: string[]): Observable<SmallEvent[]> {
    let params = new HttpParams();
    if (latitude) params = params.set('Latitude', latitude.toString());
    if (longitude) params = params.set('Longitude', longitude.toString());
    if (distanceInKm) params = params.set('DistanceInKm', distanceInKm.toString());
    if (tags) params = params.set('Tags', tags.join(','));

    return this.http.get<SmallEvent[]>(`${this.apiUrl}/getMeets`, { headers: this.getHeaders(), params });
  }
}