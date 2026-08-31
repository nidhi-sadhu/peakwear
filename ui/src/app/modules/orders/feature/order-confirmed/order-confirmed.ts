import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { OrderStore } from '@modules/orders/data-access/order.store';

@Component({
  selector: 'app-order-confirmed',
  imports: [RouterLink, CurrencyPipe, DatePipe, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './order-confirmed.html',
  styleUrl: './order-confirmed.scss',
})
export class OrderConfirmed {
  private route = inject(ActivatedRoute);
  readonly store = inject(OrderStore);

  private orderId = toSignal(this.route.paramMap.pipe(map((p) => p.get('orderId') ?? '')), {
    initialValue: '',
  });

  constructor() {
    this.store.loadOrder(this.orderId);
  }
}
