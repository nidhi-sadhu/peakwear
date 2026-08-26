import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProductDetail, ProductListItem } from '../interfaces/product.interfaces';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);

  getByCategory(category: string): Observable<ProductListItem[]> {
    return this.http.get<ProductListItem[]>('/api/products', { params: { category } });
  }

  getBySlug(slug: string): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`/api/products/${slug}`);
  }
}
