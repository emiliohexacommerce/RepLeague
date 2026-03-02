import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { PersonalRecord } from './pr.model';

@Injectable({ providedIn: 'root' })
export class PrsService {
  private readonly url = `${environment.apiUrl}/prs`;

  constructor(private http: HttpClient) {}

  getMyPrs(): Observable<PersonalRecord[]> {
    return this.http.get<PersonalRecord[]>(this.url);
  }
}
