import { Component, computed, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { StripeElements } from '@stripe/stripe-js';
import { CartStore } from '@modules/cart/data-access/cart.store';
import { AccountStore } from '@modules/account/data-access/account.store';
import { OrderStore } from '@modules/orders/data-access/order.store';
import { StripeService } from '@core/stripe/stripe.service';

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
  private stripeService = inject(StripeService);

  private elements: StripeElements | null = null;

  selectedAddressId = signal<string | null>(null);
  readonly isPaying = signal(false);

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

    // Mount the card form once the order exists and we have a client secret.
    // The plain card element rather than the Payment Element: no Link layer,
    // so returning customers get the same blank form as everyone else.
    effect(async () => {
      const secret = this.orderStore.clientSecret();
      if (!secret || this.elements) return;

      const stripe = await this.stripeService.getStripe();
      if (!stripe) return;

      this.elements = stripe.elements();
      this.elements.create('card').mount('#payment-element');
    });
  }

  placeOrder(): void {
    const addressId = this.selectedAddressId();
    if (!addressId) return;
    this.orderStore.placeOrder(addressId);
  }

  async pay(): Promise<void> {
    const stripe = await this.stripeService.getStripe();
    const secret = this.orderStore.clientSecret();
    const card = this.elements?.getElement('card');
    if (!stripe || !secret || !card) return;

    this.isPaying.set(true);

    // Card details go from Stripe's iframe straight to Stripe. They never
    // reach our JS, our server, or the network tab.
    const { error } = await stripe.confirmCardPayment(secret, {
      payment_method: { card },
    });

    this.isPaying.set(false);

    if (error) {
      this.orderStore.paymentFailed(error.message ?? 'Your payment could not be completed.');
      this.elements = null;
      return;
    }

    this.orderStore.paymentSucceeded();
  }
}
