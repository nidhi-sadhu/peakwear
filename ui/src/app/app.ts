import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private http = inject(HttpClient);
  users = signal<any[]>([]);
  error = signal<string>('');

  constructor() {
    this.http.get<any[]>('/api/users').subscribe({
      next: (data) => this.users.set(data),
      error: (err) => this.error.set(err.message),
    });
  }
}
