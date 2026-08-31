import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { ProductStore } from '@modules/products/data-access/product.store';
import { AccountStore } from '@modules/account/data-access/account.store';
import { TokenService } from '@core/auth/token.service';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, CurrencyPipe, MatButtonModule],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage {
  readonly store = inject(ProductStore);
  private accountStore = inject(AccountStore);
  private tokenService = inject(TokenService);
  private router = inject(Router);

  // Signed-in shoppers see their preferred section; everyone else gets women's
  readonly shopLink = computed(() => {
    const preference = this.accountStore.profile()?.preference?.shoppingFor;
    return preference === 'Men' ? '/shop/men' : '/shop/women';
  });

  constructor() {
    this.store.loadByCategory('new');
    if (this.tokenService.isLoggedIn) this.accountStore.load();
  }
}
