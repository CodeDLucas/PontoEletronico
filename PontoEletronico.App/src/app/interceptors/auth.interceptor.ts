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
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../services';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
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
          this.authService.logout();
          this.router.navigate(['/login']);
          this.snackBar.open('Sessão expirada. Faça login novamente.', 'Fechar', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        } else if (error.status === 403) {
          this.snackBar.open('Acesso negado.', 'Fechar', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        } else if (error.status >= 500) {
          this.snackBar.open('Erro interno do servidor. Tente novamente mais tarde.', 'Fechar', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }

        return throwError(() => error);
      })
    );
  }
}
