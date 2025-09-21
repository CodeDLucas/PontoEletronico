import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ApiResponse,
  PagedResponse,
  UserProfile,
  UserUpdate,
  ChangePassword,
  UserList
} from '../models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly API_URL = `${environment.apiUrl}/api/user`;

  constructor(private http: HttpClient) {}

  getUserProfile(): Observable<ApiResponse<UserProfile>> {
    return this.http.get<ApiResponse<UserProfile>>(`${this.API_URL}/profile`);
  }

  updateUserProfile(userData: UserUpdate): Observable<ApiResponse<UserProfile>> {
    return this.http.put<ApiResponse<UserProfile>>(`${this.API_URL}/profile`, userData);
  }

  changePassword(passwordData: ChangePassword): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.API_URL}/change-password`, passwordData);
  }

  deactivateAccount(): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.API_URL}/deactivate`, {});
  }

  getUsers(page: number = 1, pageSize: number = 10, search?: string): Observable<ApiResponse<PagedResponse<UserList>>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<ApiResponse<PagedResponse<UserList>>>(this.API_URL, { params });
  }

  getCurrentUserInfo(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.API_URL}/me`);
  }
}