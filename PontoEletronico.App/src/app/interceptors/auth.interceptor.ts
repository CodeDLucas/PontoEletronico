import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService, NotificationService } from '../services';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();

    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // For 401 errors, get more details from the response
          let message = 'Sessão expirada. Faça login novamente.';
          if (error.error?.message) {
            message = `${message} ${error.error.message}`;
          }
          this.authService.logout();
          this.router.navigate(['/login']);
          this.notificationService.showError(message);
        } else if (error.status === 403) {
          let message = 'Acesso negado.';
          if (error.error?.message) {
            message = `${message} ${error.error.message}`;
          }
          this.notificationService.showError(message);
        } else if (error.status >= 500) {
          let message = 'Erro interno do servidor. Tente novamente mais tarde.';
          if (error.error?.message) {
            message = `${message} Detalhes: ${error.error.message}`;
          }
          this.notificationService.showError(message);
        } else if (error.status === 0) {
          // Network error or CORS issue
          this.notificationService.showError('Erro de conexão. Verifique sua internet e tente novamente.');
        } else if (error.status >= 400 && error.status < 500) {
          // Other client errors
          let message = `Erro na requisição (${error.status}).`;
          if (error.error?.message) {
            message = `${message} ${error.error.message}`;
          }
          this.notificationService.showError(message);
        }

        return throwError(() => error);
      })
    );
  }
}
