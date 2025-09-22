import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ApiResponse,
  PagedResponse,
  TimeRecordCreate,
  TimeRecordResponse,
  TimeRecordList,
  TimeRecordFilter,
  TimeRecordSummary,
  TimeRecordType,
  CreateTimeRecordRequest,
  TimeRecord
} from '../models';
import { environment } from '../../environments/environment';
import { TimezoneService } from './timezone.service';

@Injectable({
  providedIn: 'root'
})
export class TimeRecordService {
  private readonly API_URL = `${environment.apiUrl}/api/timerecord`;

  constructor(
    private http: HttpClient,
    private timezoneService: TimezoneService
  ) {}

  createTimeRecord(timeRecord: CreateTimeRecordRequest): Observable<ApiResponse<TimeRecord>> {
    // Converte timestamp para UTC se fornecido
    const request = {
      ...timeRecord,
      timestamp: timeRecord.timestamp ? this.timezoneService.toUtcIsoString(timeRecord.timestamp) : undefined
    };

    return this.http.post<ApiResponse<TimeRecord>>(this.API_URL, request);
  }

  getTimeRecordById(id: number): Observable<ApiResponse<TimeRecord>> {
    return this.http.get<ApiResponse<TimeRecord>>(`${this.API_URL}/${id}`);
  }

  getTimeRecords(page: number = 1, pageSize: number = 10): Observable<ApiResponse<TimeRecord[]>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResponse<TimeRecord[]>>(this.API_URL, { params });
  }

  getTodayTimeRecords(): Observable<ApiResponse<TimeRecord[]>> {
    return this.http.get<ApiResponse<TimeRecord[]>>(`${this.API_URL}/today`);
  }

  getTimeRecordsWithFilter(filter: TimeRecordFilter): Observable<ApiResponse<TimeRecordList[]>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.startDate) {
      // Converte data local para UTC antes de enviar para o backend
      params = params.set('startDate', this.timezoneService.dateStringToUtc(filter.startDate));
    }

    if (filter.endDate) {
      // Converte data local para UTC antes de enviar para o backend
      params = params.set('endDate', this.timezoneService.dateStringToUtc(filter.endDate));
    }

    if (filter.type !== undefined) {
      params = params.set('type', filter.type.toString());
    }

    return this.http.get<ApiResponse<TimeRecordList[]>>(this.API_URL, { params });
  }

  getTimeRecordsSummary(filter: TimeRecordFilter): Observable<ApiResponse<PagedResponse<TimeRecordSummary>>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.startDate) {
      // Converte data local para UTC antes de enviar para o backend
      params = params.set('startDate', this.timezoneService.dateStringToUtc(filter.startDate));
    }

    if (filter.endDate) {
      // Converte data local para UTC antes de enviar para o backend
      params = params.set('endDate', this.timezoneService.dateStringToUtc(filter.endDate));
    }

    return this.http.get<ApiResponse<PagedResponse<TimeRecordSummary>>>(`${this.API_URL}/summary`, { params });
  }


  deleteTimeRecord(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.API_URL}/${id}`);
  }

  getTypeDescription(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn:
        return 'Entrada';
      case TimeRecordType.ClockOut:
        return 'Saída';
      case TimeRecordType.BreakStart:
        return 'Início do Intervalo';
      case TimeRecordType.BreakEnd:
        return 'Fim do Intervalo';
      default:
        return 'Desconhecido';
    }
  }

  getTypeColor(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn:
        return 'primary';
      case TimeRecordType.ClockOut:
        return 'accent';
      case TimeRecordType.BreakStart:
        return 'warn';
      case TimeRecordType.BreakEnd:
        return 'primary';
      default:
        return 'basic';
    }
  }

  getTypeIcon(type: TimeRecordType): string {
    switch (type) {
      case TimeRecordType.ClockIn:
        return 'login';
      case TimeRecordType.ClockOut:
        return 'logout';
      case TimeRecordType.BreakStart:
        return 'coffee';
      case TimeRecordType.BreakEnd:
        return 'work';
      default:
        return 'schedule';
    }
  }
}