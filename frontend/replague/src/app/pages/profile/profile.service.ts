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
  totalWorkouts: number;
  totalPrs: number;
  totalLeagues: number;
}

export interface UpdateProfileRequest {
  displayName?: string;
  country?: string;
  bio?: string;
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
}
