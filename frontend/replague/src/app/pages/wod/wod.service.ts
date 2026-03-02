import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { WodEntry, CreateWodEntryRequest } from './wod.models';

@Injectable({ providedIn: 'root' })
export class WodService {
  private readonly base = `${environment.apiUrl}/wod`;

  constructor(private http: HttpClient) {}

  getHistory(page = 1, pageSize = 20, type?: string): Observable<WodEntry[]> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    if (type) params = params.set('type', type);
    return this.http.get<WodEntry[]>(this.base, { params });
  }

  getById(id: string): Observable<WodEntry> {
    return this.http.get<WodEntry>(`${this.base}/${id}`);
  }

  create(request: CreateWodEntryRequest): Observable<WodEntry> {
    return this.http.post<WodEntry>(this.base, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
