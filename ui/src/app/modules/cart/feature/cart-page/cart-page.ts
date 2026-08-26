import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CartStore } from '@modules/cart/data-access/cart.store';

@Component({
  selector: 'app-cart-page',
  imports: [RouterLink, CurrencyPipe, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss',
})
export class CartPage {
  readonly store = inject(CartStore);

  constructor() {
    this.store.load();
  }

  changeQuantity(itemId: string, quantity: number): void {
    this.store.updateQuantity({ itemId, quantity });
  }
}
