import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { CurrencyPipe, TitleCasePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ProductStore } from '@modules/products/data-access/product.store';

@Component({
  selector: 'app-product-list',
  imports: [RouterLink, CurrencyPipe, MatCardModule, MatProgressSpinnerModule, TitleCasePipe],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
})
export class ProductList {
  private route = inject(ActivatedRoute);
  readonly store = inject(ProductStore);

  // URL param as a signal, so navigating women -> men re-fetches automatically
  readonly category = toSignal(
    this.route.paramMap.pipe(map((params) => params.get('category') ?? 'women')),
    { initialValue: 'women' },
  );

  constructor() {
    this.store.loadByCategory(this.category);
  }
}
