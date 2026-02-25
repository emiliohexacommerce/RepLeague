import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { League, LeagueMember, RankingEntry, CreateLeagueRequest } from './league.model';

@Injectable({ providedIn: 'root' })
export class LeaguesService {
  private readonly url = `${environment.apiUrl}/leagues`;

  constructor(private http: HttpClient) {}

  getMyLeagues(): Observable<League[]> {
    return this.http.get<League[]>(`${this.url}/mine`);
  }

  getLeagueById(id: string): Observable<League> {
    return this.http.get<League>(`${this.url}/${id}`);
  }

  createLeague(request: CreateLeagueRequest): Observable<League> {
    return this.http.post<League>(this.url, request);
  }

  deleteLeague(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  getMembers(leagueId: string): Observable<LeagueMember[]> {
    return this.http.get<LeagueMember[]>(`${this.url}/${leagueId}/members`);
  }

  getRanking(leagueId: string): Observable<RankingEntry[]> {
    return this.http.get<RankingEntry[]>(`${this.url}/${leagueId}/ranking`);
  }

  inviteMember(leagueId: string, email?: string): Observable<{ token: string }> {
    return this.http.post<{ token: string }>(`${this.url}/${leagueId}/invite`, { email });
  }

  joinLeague(token: string): Observable<void> {
    return this.http.post<void>(`${this.url}/join/${token}`, {});
  }

  removeMember(leagueId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${leagueId}/members/${userId}`);
  }
}
