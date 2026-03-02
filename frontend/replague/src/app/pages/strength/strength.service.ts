import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LiftSessionDto, LiftPrDto, CreateLiftSessionRequest } from './strength.models';

@Injectable({ providedIn: 'root' })
export class StrengthService {
  private readonly base = `${environment.apiUrl}/strength`;

  constructor(private http: HttpClient) {}

  getHistory(page = 1, pageSize = 20): Observable<LiftSessionDto[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<LiftSessionDto[]>(this.base, { params });
  }

  getPrs(): Observable<LiftPrDto[]> {
    return this.http.get<LiftPrDto[]>(`${this.base}/prs`);
  }

  create(request: CreateLiftSessionRequest): Observable<LiftSessionDto> {
    return this.http.post<LiftSessionDto>(this.base, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
