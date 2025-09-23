import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor(private snackBar: MatSnackBar) {}

  /**
   * Exibe mensagem de sucesso
   */
  showSuccess(message: string, duration: number = 3000): void {
    this.snackBar.open(message, 'Fechar', {
      duration,
      panelClass: ['success-snackbar']
    });
  }

  /**
   * Exibe mensagem de sucesso com informação de redirecionamento
   */
  showSuccessWithRedirect(message: string, redirectMessage: string = 'Redirecionando...', duration: number = 1500): void {
    const fullMessage = `${message} ${redirectMessage}`;
    this.snackBar.open(fullMessage, 'Fechar', {
      duration,
      panelClass: ['success-snackbar']
    });
  }

  /**
   * Exibe mensagem de erro básica
   */
  showError(message: string, duration: number = 5000): void {
    this.snackBar.open(message, 'Fechar', {
      duration,
      panelClass: ['error-snackbar']
    });
  }

  /**
   * Exibe mensagem de informação
   */
  showInfo(message: string, duration: number = 4000): void {
    this.snackBar.open(message, 'Fechar', {
      duration,
      panelClass: ['info-snackbar']
    });
  }

  /**
   * Exibe mensagem de aviso
   */
  showWarning(message: string, duration: number = 4000): void {
    this.snackBar.open(message, 'Fechar', {
      duration,
      panelClass: ['warning-snackbar']
    });
  }

  /**
   * Processa resposta de sucesso da API e exibe mensagem apropriada
   */
  handleSuccessResponse<T>(response: ApiResponse<T>, defaultMessage?: string): void {
    const message = response.message || defaultMessage || 'Operação realizada com sucesso!';
    this.showSuccess(message);
  }

  /**
   * Processa resposta de erro da API e exibe mensagem detalhada
   */
  handleErrorResponse<T>(response: ApiResponse<T>, fallbackMessage?: string): void {
    let errorMessage = fallbackMessage || 'Ocorreu um erro inesperado.';

    if (response.message) {
      errorMessage = response.message;
    }

    // Erros específicos são mantidos apenas no console para debugging
    if (response.errors && response.errors.length > 0) {
      console.error('API Error Details:', response.errors);
    }

    this.showError(errorMessage);
  }

  /**
   * Processa erro HTTP e exibe mensagem apropriada
   */
  handleHttpError(error: any, fallbackMessage?: string): void {
    let errorMessage = fallbackMessage || 'Erro de conexão. Tente novamente.';

    if (error?.error?.message) {
      // Usa apenas a mensagem principal da API
      errorMessage = error.error.message;

      // Detalhes técnicos ficam apenas no console para debugging
      if (error.error.errors && error.error.errors.length > 0) {
        console.error('HTTP Error Details:', error.error.errors);
      }
    } else if (error?.message) {
      // Para erros HTTP genéricos, usa apenas o fallback sem detalhes técnicos
      errorMessage = fallbackMessage || 'Erro de conexão. Tente novamente.';
      console.error('HTTP Error:', error.message);
    }

    this.showError(errorMessage);
  }

  /**
   * Exibe mensagem baseada no resultado da operação
   */
  handleApiResult<T>(
    response: ApiResponse<T>,
    successMessage?: string,
    errorMessage?: string
  ): boolean {
    if (response.success) {
      this.handleSuccessResponse(response, successMessage);
      return true;
    } else {
      this.handleErrorResponse(response, errorMessage);
      return false;
    }
  }
}