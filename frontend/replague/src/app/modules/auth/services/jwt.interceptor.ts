import { Injectable, Injector } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { AuthModel } from '../models/auth.model';
import { environment } from 'src/environments/environment';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  private readonly storageKey = `${environment.appVersion}-${environment.USERDATA_KEY}`;

  constructor(private injector: Injector) {}

  private get authService(): AuthService {
    return this.injector.get(AuthService);
  }

  /** Reads the access token directly from localStorage — no AuthService dependency at construction time */
  private getTokenFromStorage(): string | null {
    try {
      const raw = localStorage.getItem(this.storageKey);
      if (!raw) return null;
      const auth: AuthModel = JSON.parse(raw);
      return auth?.accessToken ?? null;
    } catch {
      return null;
    }
  }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.getTokenFromStorage();
    // Prevent infinite loop: do not attempt refresh-on-401 for auth endpoints
    const isAuthRequest = request.url.includes('/auth/');

    const authReq = token
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && token && !isAuthRequest) {
          return this.authService.refreshAuthToken().pipe(
            switchMap((newAuth: AuthModel) => {
              if (!newAuth?.accessToken) {
                return throwError(() => error);
              }
              const retryReq = request.clone({
                setHeaders: { Authorization: `Bearer ${newAuth.accessToken}` },
              });
              return next.handle(retryReq);
            }),
            catchError(() => throwError(() => error))
          );
        }
        return throwError(() => error);
      })
    );
  }
}
