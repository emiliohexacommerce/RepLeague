import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Workout, CreateWorkoutRequest } from './workout.model';

@Injectable({ providedIn: 'root' })
export class WorkoutsService {
  private readonly url = `${environment.apiUrl}/workouts`;

  constructor(private http: HttpClient) {}

  getWorkouts(range: 'last7' | 'last30' | 'last90' = 'last30'): Observable<Workout[]> {
    const params = new HttpParams().set('range', range);
    return this.http.get<Workout[]>(this.url, { params });
  }

  getWorkoutById(id: string): Observable<Workout> {
    return this.http.get<Workout>(`${this.url}/${id}`);
  }

  createWorkout(request: CreateWorkoutRequest): Observable<Workout> {
    return this.http.post<Workout>(this.url, request);
  }
}
