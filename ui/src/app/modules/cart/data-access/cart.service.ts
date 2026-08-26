import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cart } from '../interfaces/cart.interfaces';

@Injectable({ providedIn: 'root' })
export class CartService {
  private http = inject(HttpClient);

  get(): Observable<Cart> {
    return this.http.get<Cart>('/api/cart');
  }

  add(productVariantId: string, quantity = 1): Observable<Cart> {
    return this.http.post<Cart>('/api/cart', { productVariantId, quantity });
  }

  updateQuantity(itemId: string, quantity: number): Observable<Cart> {
    return this.http.put<Cart>(`/api/cart/${itemId}`, { quantity });
  }

  remove(itemId: string): Observable<Cart> {
    return this.http.delete<Cart>(`/api/cart/${itemId}`);
  }
}
