import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { loadStripe, Stripe } from '@stripe/stripe-js';

@Injectable({ providedIn: 'root' })
export class StripeService {
  private http = inject(HttpClient);
  private stripePromise: Promise<Stripe | null> | null = null;

  // Stripe.js is loaded from Stripe's own CDN, never bundled — that's a hard
  // requirement of staying out of PCI scope. Cached so the script loads once.
  getStripe(): Promise<Stripe | null> {
    if (!this.stripePromise) {
      this.stripePromise = this.loadWithKey();
    }
    return this.stripePromise;
  }

  private async loadWithKey(): Promise<Stripe | null> {
    const config = await firstValueFrom(
      this.http.get<{ publishableKey: string }>('/api/payments/config'),
    );
    return loadStripe(config.publishableKey);
  }
}
