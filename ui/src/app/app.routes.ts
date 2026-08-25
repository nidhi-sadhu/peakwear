import { Routes } from '@angular/router';
import { Test1 } from './pages/test1/test1';
import { Test2 } from './pages/test2/test2';

export const routes: Routes = [
  { path: '', redirectTo: 'test1', pathMatch: 'full' },
  { path: 'test1', component: Test1 },
  { path: 'test2', component: Test2 },
];
