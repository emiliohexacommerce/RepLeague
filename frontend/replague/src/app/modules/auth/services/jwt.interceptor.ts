import { Injectable } from '@angular/core';
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

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getAccessToken();

    const authReq = token
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && token) {
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
