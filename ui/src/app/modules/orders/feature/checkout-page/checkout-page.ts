import { Component, computed, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CartStore } from '@modules/cart/data-access/cart.store';
import { AccountStore } from '@modules/account/data-access/account.store';
import { OrderStore } from '@modules/orders/data-access/order.store';

@Component({
  selector: 'app-checkout-page',
  imports: [
    RouterLink,
    CurrencyPipe,
    MatCardModule,
    MatRadioModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './checkout-page.html',
  styleUrl: './checkout-page.scss',
})
export class CheckoutPage {
  readonly cartStore = inject(CartStore);
  readonly accountStore = inject(AccountStore);
  readonly orderStore = inject(OrderStore);

  selectedAddressId = signal<string | null>(null);

  private readonly freeShippingThreshold = 150;

  readonly shipping = computed(() =>
    this.cartStore.subtotal() >= this.freeShippingThreshold ? 0 : 9.95,
  );

  readonly total = computed(() => this.cartStore.subtotal() + this.shipping());

  readonly amountToFreeShipping = computed(() =>
    Math.max(0, this.freeShippingThreshold - this.cartStore.subtotal()),
  );

  readonly canPlaceOrder = computed(
    () =>
      this.selectedAddressId() !== null &&
      !this.cartStore.isEmpty() &&
      !this.orderStore.isPlacing(),
  );

  constructor() {
    this.cartStore.load();
    this.accountStore.load();

    // Pre-select the default address once addresses arrive
    effect(() => {
      const addresses = this.accountStore.addresses();
      if (addresses.length > 0 && this.selectedAddressId() === null) {
        const preferred = addresses.find((a) => a.isDefault) ?? addresses[0];
        this.selectedAddressId.set(preferred.id);
      }
    });
  }

  placeOrder(): void {
    const addressId = this.selectedAddressId();
    if (!addressId) return;
    this.orderStore.placeOrder(addressId);
  }
}
