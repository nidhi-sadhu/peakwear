import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { ProductStore } from '@modules/products/data-access/product.store';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, CurrencyPipe, MatButtonModule],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePage {
  readonly store = inject(ProductStore);

  constructor() {
    this.store.loadByCategory('new');
  }
}
