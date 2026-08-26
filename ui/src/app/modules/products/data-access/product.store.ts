import { inject } from '@angular/core';
import { patchState, signalStore, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { tapResponse } from '@ngrx/operators';
import { pipe, switchMap, tap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ProductService } from './product.service';
import { ProductDetail, ProductListItem, ProductState } from '../interfaces/product.interfaces';

const initialState: ProductState = {
  products: [],
  selected: null,
  isLoading: false,
  error: null,
};

export const ProductStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),

  withMethods((store, productService = inject(ProductService)) => ({
    loadByCategory: rxMethod<string>(
      pipe(
        tap(() => patchState(store, { isLoading: true, error: null })),
        switchMap((category) =>
          productService.getByCategory(category).pipe(
            tapResponse({
              next: (products: ProductListItem[]) =>
                patchState(store, { products, isLoading: false }),
              error: (error: HttpErrorResponse) =>
                patchState(store, {
                  error: error.error?.message ?? 'Could not load products.',
                  isLoading: false,
                }),
            }),
          ),
        ),
      ),
    ),

    loadBySlug: rxMethod<string>(
      pipe(
        tap(() => patchState(store, { isLoading: true, error: null, selected: null })),
        switchMap((slug) =>
          productService.getBySlug(slug).pipe(
            tapResponse({
              next: (selected: ProductDetail) => patchState(store, { selected, isLoading: false }),
              error: (error: HttpErrorResponse) =>
                patchState(store, {
                  error: error.status === 404 ? 'Product not found.' : 'Could not load product.',
                  isLoading: false,
                }),
            }),
          ),
        ),
      ),
    ),
  })),
);
