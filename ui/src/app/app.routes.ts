import { Routes } from '@angular/router';
import { authGuard } from '@core/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'shop/women', pathMatch: 'full' },
  {
    path: 'shop/:category',
    loadComponent: () =>
      import('@modules/products/feature/product-list/product-list').then((m) => m.ProductList),
  },
  {
    path: 'cart',
    loadComponent: () =>
      import('@modules/cart/feature/cart-page/cart-page').then((m) => m.CartPage),
  },
  {
    path: 'account/profile',
    canActivate: [authGuard],
    loadComponent: () =>
      import('@modules/account/feature/profile-page/profile-page').then((m) => m.ProfilePage),
  },
  {
    path: 'account/orders',
    canActivate: [authGuard],
    loadComponent: () =>
      import('@modules/account/feature/orders-page/orders-page').then((m) => m.OrdersPage),
  },
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
];
