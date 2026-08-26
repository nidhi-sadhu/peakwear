import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { CurrencyPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ProductStore } from '@modules/products/data-access/product.store';
import { CartStore } from '@modules/cart/data-access/cart.store';
import { TokenService } from '@core/auth/token.service';

@Component({
  selector: 'app-product-detail',
  imports: [CurrencyPipe, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
})
export class ProductDetail {
  private route = inject(ActivatedRoute);
  readonly store = inject(ProductStore);
  private cartStore = inject(CartStore);
  private tokenService = inject(TokenService);
  private router = inject(Router);

  private slug = toSignal(this.route.paramMap.pipe(map((p) => p.get('slug') ?? '')), {
    initialValue: '',
  });

  selectedColour = signal<string | null>(null);
  selectedSize = signal<string | null>(null);

  // Distinct colours, in the order the API returned them
  readonly colours = computed(() => {
    const variants = this.store.selected()?.variants ?? [];
    return [...new Set(variants.map((v) => v.colour))];
  });

  readonly sizes = computed(() => {
    const variants = this.store.selected()?.variants ?? [];
    return [...new Set(variants.map((v) => v.size))];
  });

  // The one variant matching both selections — null until both are chosen
  readonly selectedVariant = computed(() => {
    const colour = this.selectedColour();
    const size = this.selectedSize();
    if (!colour || !size) return null;
    return (
      this.store.selected()?.variants.find((v) => v.colour === colour && v.size === size) ?? null
    );
  });

  // Image follows the chosen colour, falling back to the first variant
  readonly displayImage = computed(() => {
    const colour = this.selectedColour();
    const variants = this.store.selected()?.variants ?? [];
    const match = colour ? variants.find((v) => v.colour === colour) : variants[0];
    return match?.imageUrl ?? null;
  });

  readonly canAddToCart = computed(() => this.selectedVariant()?.inStock === true);

  constructor() {
    this.store.loadBySlug(this.slug);
  }

  sizeAvailable(size: string): boolean {
    const colour = this.selectedColour();
    if (!colour) return true;
    return (
      this.store
        .selected()
        ?.variants.some((v) => v.colour === colour && v.size === size && v.inStock) ?? false
    );
  }

  selectColour(colour: string): void {
    this.selectedColour.set(colour);
    const size = this.selectedSize();
    if (size && !this.sizeAvailable(size)) this.selectedSize.set(null);
  }

  addToCart(): void {
    const variant = this.selectedVariant();
    if (!variant) return;

    if (!this.tokenService.isLoggedIn) {
      sessionStorage.setItem(
        'pendingCartItem',
        JSON.stringify({
          variantId: variant.id,
          quantity: 1,
        }),
      );
      void this.router.navigate(['/login'], {
        queryParams: { returnUrl: this.router.url },
      });
      return;
    }

    this.cartStore.add({ variantId: variant.id, quantity: 1 });
  }
}
