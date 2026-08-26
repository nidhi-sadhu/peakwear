import { computed, inject } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { CartService } from './cart.service';
import { Cart, CartState } from '../interfaces/cart.interfaces';
import { TokenService } from '@core/auth/token.service';

const initialState: CartState = { cart: null, isLoading: false, error: null };

export const CartStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),

  withComputed((store) => ({
    itemCount: computed(() => store.cart()?.itemCount ?? 0),
    items: computed(() => store.cart()?.items ?? []),
    subtotal: computed(() => store.cart()?.subtotal ?? 0),
    isEmpty: computed(() => (store.cart()?.items.length ?? 0) === 0),
  })),

  withMethods((store, cartService = inject(CartService), tokenService = inject(TokenService)) => {
    const handle = () =>
      tapResponse({
        next: (cart: Cart) => patchState(store, { cart, isLoading: false, error: null }),
        error: (error: HttpErrorResponse) =>
          patchState(store, {
            error: error.error?.message ?? 'Something went wrong with your bag.',
            isLoading: false,
          }),
      });
    return {
      load: rxMethod<void>(
        pipe(
          tap(() => patchState(store, { isLoading: true })),
          switchMap(() => cartService.get().pipe(handle())),
        ),
      ),

      add: rxMethod<{ variantId: string; quantity: number }>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(({ variantId, quantity }) =>
            cartService.add(variantId, quantity).pipe(handle()),
          ),
        ),
      ),

      updateQuantity: rxMethod<{ itemId: string; quantity: number }>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(({ itemId, quantity }) =>
            cartService.updateQuantity(itemId, quantity).pipe(handle()),
          ),
        ),
      ),

      remove: rxMethod<string>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap((itemId) => cartService.remove(itemId).pipe(handle())),
        ),
      ),

      // Called on logout — clear local state so the next user doesn't see it
      clear: () => patchState(store, initialState),

      // Load the cart only if signed in
      hydrate: () => {
        if (tokenService.isLoggedIn) {
          patchState(store, { isLoading: true });
        }
      },
    };
  }),
);
