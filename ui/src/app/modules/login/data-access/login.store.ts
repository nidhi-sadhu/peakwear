import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap, tap } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AuthResponse,
  LoginRequest,
  LoginState,
  RegisterRequest,
} from '../interfaces/login.interfaces';
import { AuthService } from '../../../core/auth/auth.service';
import { TokenService } from '../../../core/auth/token.service';

const initialState: LoginState & { isLoading: boolean } = {
  currentUser: null,
  error: null,
  isLoading: false,
};

export const LoginStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    isLoggedIn: computed(() => store.currentUser() !== null),
    fullName: computed(() => {
      const user = store.currentUser();
      return user ? `${user.firstName} ${user.lastName}` : '';
    }),
  })),

  withMethods(
    (
      store,
      authService = inject(AuthService),
      tokenService = inject(TokenService),
      router = inject(Router),
      route = inject(ActivatedRoute),
    ) => ({
      login: rxMethod<LoginRequest>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap((request) =>
            authService.login(request).pipe(
              tapResponse({
                next: (response: AuthResponse) => {
                  tokenService.save(response.token, response.user);
                  patchState(store, { currentUser: response.user, isLoading: false });

                  const returnUrl = route.snapshot.queryParams['returnUrl'] ?? '/';
                  void router.navigateByUrl(returnUrl);
                },
                error: (error: HttpErrorResponse) =>
                  patchState(store, {
                    error: error.error?.message ?? 'Login failed. Please try again.',
                    isLoading: false,
                  }),
              }),
            ),
          ),
        ),
      ),

      register: rxMethod<RegisterRequest>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap((request) =>
            authService.register(request).pipe(
              tapResponse({
                next: (response: AuthResponse) => {
                  tokenService.save(response.token, response.user);
                  patchState(store, { currentUser: response.user, isLoading: false });
                  void router.navigate(['/']);
                },
                error: (error: HttpErrorResponse) =>
                  patchState(store, {
                    error: error.error?.message ?? 'Login failed. Please try again.',
                    isLoading: false,
                  }),
              }),
            ),
          ),
        ),
      ),

      logout: () => {
        tokenService.clear();
        patchState(store, { currentUser: null });
        void router.navigate(['/login']);
      },
      hydrate: () => patchState(store, { currentUser: tokenService.currentUser() }),
    }),
  ),
);
