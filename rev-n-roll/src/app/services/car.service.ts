import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Car, CarRequest } from '../models/car';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class CarService {
  private apiUrl = 'http://localhost:5123/api/v1/cars'; // Replace with your backend URL

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  addCar(userId: number, car: CarRequest): Observable<Car> {
    return this.http.post<Car>(`${this.apiUrl}/addCar/${userId}`, car, { headers: this.getHeaders() });
  }

  updateCar(carId: number, car: CarRequest): Observable<Car> {
    return this.http.post<Car>(`${this.apiUrl}/updateCar/${carId}`, car, { headers: this.getHeaders() });
  }

  getCars(userId: number): Observable<Car[]> {
    return this.http.get<Car[]>(`${this.apiUrl}/getCars/${userId}`, { headers: this.getHeaders() });
  }

  deleteCar(carId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/deleteCar/${carId}`, { headers: this.getHeaders() });
  }
}