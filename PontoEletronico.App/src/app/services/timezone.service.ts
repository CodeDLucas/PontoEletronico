import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TimezoneService {

  /**
   * Converte uma data local para UTC (para envio ao backend)
   */
  toUtc(date: Date): Date {
    return new Date(date.getTime() + (date.getTimezoneOffset() * 60000));
  }

  /**
   * Converte uma data UTC (recebida do backend) para timezone local
   */
  toLocal(utcDate: Date | string): Date {
    // JavaScript automaticamente converte strings ISO para timezone local
    return new Date(utcDate);
  }

  /**
   * Formata uma data UTC para exibição local
   */
  formatToLocal(utcDate: Date | string, options?: Intl.DateTimeFormatOptions): string {
    const date = new Date(utcDate);

    const defaultOptions: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false,
      timeZone: undefined // Use timezone local do usuário
    };

    return date.toLocaleDateString('pt-BR', { ...defaultOptions, ...options });
  }

  /**
   * Formata apenas a data (sem horário) para exibição local
   */
  formatDateOnly(utcDate: Date | string): string {
    const date = new Date(utcDate);
    return date.toLocaleDateString('pt-BR', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      timeZone: undefined // Use timezone local do usuário
    });
  }

  /**
   * Formata apenas o horário para exibição local
   */
  formatTimeOnly(utcDate: Date | string): string {
    const date = new Date(utcDate);
    return date.toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false,
      timeZone: undefined // Use timezone local do usuário
    });
  }

  /**
   * Formata timestamp completo (data e hora) para exibição local
   */
  formatDateTime(utcDate: Date | string): string {
    const date = new Date(utcDate);
    return date.toLocaleString('pt-BR', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
      timeZone: undefined // Use timezone local do usuário
    });
  }

  /**
   * Converte uma data local para string ISO UTC (para envio ao backend)
   */
  toUtcIsoString(date: Date): string {
    return date.toISOString();
  }

  /**
   * Obtém a data atual em UTC
   */
  nowUtc(): Date {
    return new Date();
  }

  /**
   * Converte uma string de data (formato input date) para UTC
   */
  dateStringToUtc(dateString: string): string {
    if (!dateString) return '';

    const localDate = new Date(dateString + 'T00:00:00');
    return localDate.toISOString();
  }

  /**
   * Converte uma data UTC para string de data local (formato input date)
   */
  utcToDateString(utcDate: Date | string): string {
    const date = new Date(utcDate);
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  /**
   * Calcula a diferença entre duas datas em milissegundos
   */
  timeDifference(startDate: Date | string, endDate: Date | string): number {
    const start = new Date(startDate);
    const end = new Date(endDate);
    return end.getTime() - start.getTime();
  }

  /**
   * Formata duração em milissegundos para string HH:mm:ss
   */
  formatDuration(durationMs: number): string {
    const hours = Math.floor(durationMs / (1000 * 60 * 60));
    const minutes = Math.floor((durationMs % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((durationMs % (1000 * 60)) / 1000);

    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }
}