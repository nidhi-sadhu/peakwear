import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Address,
  AddressRequest,
  ChangePasswordRequest,
  Preference,
  Profile,
  UpdateProfileRequest,
} from '../interfaces/account.interfaces';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private http = inject(HttpClient);

  getProfile(): Observable<Profile> {
    return this.http.get<Profile>('/api/account/profile');
  }

  updateProfile(request: UpdateProfileRequest): Observable<Profile> {
    return this.http.put<Profile>('/api/account/profile', request);
  }

  updatePreferences(request: Preference): Observable<Preference> {
    return this.http.put<Preference>('/api/account/preferences', request);
  }

  addAddress(request: AddressRequest): Observable<Address[]> {
    return this.http.post<Address[]>('/api/account/addresses', request);
  }

  updateAddress(id: string, request: AddressRequest): Observable<Address[]> {
    return this.http.put<Address[]>(`/api/account/addresses/${id}`, request);
  }

  deleteAddress(id: string): Observable<Address[]> {
    return this.http.delete<Address[]>(`/api/account/addresses/${id}`);
  }

  changePassword(request: ChangePasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/account/change-password', request);
  }
}
