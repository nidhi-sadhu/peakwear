import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('@modules/login/feature/login-page/login-page').then((m) => m.LoginPage),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('@modules/login/feature/register-page/register-page').then((m) => m.RegisterPage),
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
];
