import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { OrderService } from './order.service';
import { Order, OrderState } from '../interfaces/order.interfaces';
import { CartStore } from '@modules/cart/data-access/cart.store';

const initialState: OrderState = {
  orders: [],
  lastOrder: null,
  isPlacing: false,
  isLoading: false,
  error: null,
};

export const OrderStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),

  withMethods(
    (
      store,
      orderService = inject(OrderService),
      cartStore = inject(CartStore),
      router = inject(Router),
    ) => ({
      placeOrder: rxMethod<string>(
        pipe(
          tap(() => patchState(store, { isPlacing: true, error: null })),
          switchMap((addressId) =>
            orderService.placeOrder(addressId).pipe(
              tapResponse({
                next: (order: Order) => {
                  patchState(store, { lastOrder: order, isPlacing: false });
                  cartStore.clear(); // the API already emptied it server-side
                  void router.navigate(['/order-confirmed', order.id]);
                },
                error: (error: HttpErrorResponse) =>
                  patchState(store, {
                    error: error.error?.message ?? 'We could not place your order.',
                    isPlacing: false,
                  }),
              }),
            ),
          ),
        ),
      ),

      loadOrders: rxMethod<void>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(() =>
            orderService.getOrders().pipe(
              tapResponse({
                next: (orders: Order[]) => patchState(store, { orders, isLoading: false }),
                error: () =>
                  patchState(store, { error: 'Could not load your orders.', isLoading: false }),
              }),
            ),
          ),
        ),
      ),

      loadOrder: rxMethod<string>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap((id) =>
            orderService.getOrder(id).pipe(
              tapResponse({
                next: (lastOrder: Order) => patchState(store, { lastOrder, isLoading: false }),
                error: () => patchState(store, { error: 'Order not found.', isLoading: false }),
              }),
            ),
          ),
        ),
      ),

      clearError: () => patchState(store, { error: null }),
    }),
  ),
);
