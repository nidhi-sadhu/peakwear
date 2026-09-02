import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CheckoutResponse, Order } from '../interfaces/order.interfaces';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);

  placeOrder(addressId: string): Observable<CheckoutResponse> {
    return this.http.post<CheckoutResponse>('/api/orders', { addressId });
  }

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>('/api/orders');
  }

  getOrder(orderId: string): Observable<Order> {
    return this.http.get<Order>(`/api/orders/${orderId}`);
  }
}
