import { Injectable, signal } from '@angular/core';
import { UserResponse } from '../../modules/login/interfaces/login.interfaces';

const TOKEN_KEY = 'peakwear_token';
const USER_KEY = 'peakwear_user';

@Injectable({ providedIn: 'root' })
export class TokenService {
  readonly currentUser = signal<UserResponse | null>(this.readUser());

  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  get isLoggedIn(): boolean {
    return !!this.token;
  }

  save(token: string, user: UserResponse): void {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.currentUser.set(user);
  }

  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.currentUser.set(null);
  }

  private readUser(): UserResponse | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }
}
