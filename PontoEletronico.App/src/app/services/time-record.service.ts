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
  UpdateTimeRecordRequest,
  TimeRecord
} from '../models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TimeRecordService {
  private readonly API_URL = `${environment.apiUrl}/api/timerecord`;

  constructor(private http: HttpClient) {}

  createTimeRecord(timeRecord: CreateTimeRecordRequest): Observable<ApiResponse<TimeRecord>> {
    return this.http.post<ApiResponse<TimeRecord>>(this.API_URL, timeRecord);
  }

  updateTimeRecord(id: number, timeRecord: UpdateTimeRecordRequest): Observable<ApiResponse<TimeRecord>> {
    return this.http.put<ApiResponse<TimeRecord>>(`${this.API_URL}/${id}`, timeRecord);
  }

  getTimeRecordById(id: number): Observable<ApiResponse<TimeRecord>> {
    return this.http.get<ApiResponse<TimeRecord>>(`${this.API_URL}/${id}`);
  }

  getTimeRecords(page: number = 1, pageSize: number = 10): Observable<ApiResponse<PagedResponse<TimeRecord>>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResponse<PagedResponse<TimeRecord>>>(this.API_URL, { params });
  }

  getTimeRecordsWithFilter(filter: TimeRecordFilter): Observable<ApiResponse<TimeRecordList[]>> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString());

    if (filter.startDate) {
      params = params.set('startDate', filter.startDate);
    }

    if (filter.endDate) {
      params = params.set('endDate', filter.endDate);
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
      params = params.set('startDate', filter.startDate);
    }

    if (filter.endDate) {
      params = params.set('endDate', filter.endDate);
    }

    return this.http.get<ApiResponse<PagedResponse<TimeRecordSummary>>>(`${this.API_URL}/summary`, { params });
  }

  getTodayTimeRecords(): Observable<ApiResponse<TimeRecordList[]>> {
    return this.http.get<ApiResponse<TimeRecordList[]>>(`${this.API_URL}/today`);
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