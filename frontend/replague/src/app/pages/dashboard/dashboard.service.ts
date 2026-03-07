import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface WelcomeDto {
  firstName: string;
  displayName: string;
  avatarUrl?: string;
}

export interface StreakDto {
  weeks: number;
}

export interface WeeklySummaryDto {
  sessions: number;
  prs: number;
  volumeKg: number;
  timeSec: number;
}

export interface KpiDto {
  key: string;
  label: string;
  value: string;
}

export interface RecentWodDashDto {
  id: string;
  type: string;
  title?: string;
  date: string;
  elapsedTime?: string;
  rxScaled: boolean;
  rounds?: number;
  extraReps?: number;
}

export interface RecentLiftDashDto {
  sessionId: string;
  exercise: string;
  reps: number;
  weight: number;
  est1Rm?: number;
  isPr: boolean;
  date: string;
}

export interface LeagueDashDto {
  leagueId: string;
  name: string;
  points: number;
  rank: number;
  members: number;
}

export interface ChartDatasetDto {
  labels: string[];
  data: number[];
}

export interface DashboardChartsDto {
  strength1Rm: ChartDatasetDto;
  weeklyVolume: ChartDatasetDto;
  forTimeBest: ChartDatasetDto;
  topExercise?: string;
}

export interface RecommendationDto {
  code: string;
  title: string;
  body: string;
  cta: { label: string; url: string };
}

export interface QuoteDto {
  text: string;
  author: string;
  lang: string;
}

export interface DashboardOverviewDto {
  welcome: WelcomeDto;
  streak: StreakDto;
  weeklySummary: WeeklySummaryDto;
  kpis: KpiDto[];
  recentWods: RecentWodDashDto[];
  recentLifts: RecentLiftDashDto[];
  leagues: LeagueDashDto[];
  charts: DashboardChartsDto;
  recommendations: RecommendationDto[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getOverview(): Observable<DashboardOverviewDto> {
    return this.http.get<DashboardOverviewDto>(`${this.base}/dashboard/overview`);
  }

  getDailyQuote(lang = 'es'): Observable<QuoteDto> {
    return this.http.get<QuoteDto>(`${this.base}/quotes/daily?lang=${lang}`);
  }
}
