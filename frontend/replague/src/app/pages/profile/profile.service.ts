import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface ProfileDto {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  country?: string;
  bio?: string;
  phone?: string;
  birthDate?: string;       // YYYY-MM-DD
  city?: string;
  gymName?: string;
  units: string;            // "kg" | "lb"
  oneRmMethod: string;      // "Epley" | "Brzycki"
  visibility: string;       // "private" | "leagues" | "public"
  marketingConsent: boolean;
  totalWorkouts: number;
  totalPrs: number;
  totalLeagues: number;
}

export interface UpdateProfileRequest {
  displayName?: string;
  country?: string;
  bio?: string;
  phone?: string;
  birthDate?: string;
  city?: string;
  gymName?: string;
  units?: string;
  oneRmMethod?: string;
  visibility?: string;
  marketingConsent?: boolean;
}

export interface LeagueSummaryDto {
  leagueId: string;
  leagueName: string;
  points: number;
  rank: number;
  membersCount: number;
}

export interface PrSummaryDto {
  exerciseName: string;
  bestWeightKg: number;
  achievedAt: string;
}

export interface RecentWodDto {
  id: string;
  type: string;
  title?: string;
  date: string;
  elapsedTime?: string;
  rxScaled: boolean;
}

export interface ProfileSummaryDto {
  streakWeeks: number;
  totalWods: number;
  totalLiftSessions: number;
  leagues: LeagueSummaryDto[];
  topPrs: PrSummaryDto[];
  recentWods: RecentWodDto[];
}

export interface StrengthChartPointDto {
  date: string;
  weightKg: number;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly url = `${environment.apiUrl}/me`;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<ProfileDto> {
    return this.http.get<ProfileDto>(this.url);
  }

  updateProfile(request: UpdateProfileRequest): Observable<ProfileDto> {
    return this.http.patch<ProfileDto>(this.url, request);
  }

  uploadAvatar(file: File): Observable<{ avatarUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ avatarUrl: string }>(`${this.url}/avatar`, formData);
  }

  getProfileSummary(): Observable<ProfileSummaryDto> {
    return this.http.get<ProfileSummaryDto>(`${this.url}/summary`);
  }

  getStrengthChart(exercise: string): Observable<StrengthChartPointDto[]> {
    return this.http.get<StrengthChartPointDto[]>(`${this.url}/strength-chart`, {
      params: { exercise },
    });
  }
}
