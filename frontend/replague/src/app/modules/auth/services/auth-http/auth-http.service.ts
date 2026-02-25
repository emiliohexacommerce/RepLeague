import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { UserModel } from '../../models/user.model';
import { environment } from '../../../../../environments/environment';
import { AuthModel } from '../../models/auth.model';
import { map } from 'rxjs/operators';

const API_URL = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class AuthHTTPService {
  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<AuthModel> {
    return this.http.post<AuthModel>(`${API_URL}/auth/login`, { email, password });
  }

  register(email: string, password: string, displayName: string): Observable<AuthModel> {
    return this.http.post<AuthModel>(`${API_URL}/auth/register`, { email, password, displayName });
  }

  refreshToken(refreshToken: string): Observable<AuthModel> {
    return this.http.post<AuthModel>(`${API_URL}/auth/refresh`, { refreshToken });
  }

  forgotPassword(email: string): Observable<boolean> {
    return this.http.post<boolean>(`${API_URL}/auth/forgot`, { email });
  }

  getUserByToken(token: string): Observable<UserModel> {
    const httpHeaders = new HttpHeaders({ Authorization: `Bearer ${token}` });
    return this.http.get<any>(`${API_URL}/me`, { headers: httpHeaders }).pipe(
      map(profile => {
        const user = new UserModel();
        user.setUser(profile);
        return user;
      })
    );
  }
}
