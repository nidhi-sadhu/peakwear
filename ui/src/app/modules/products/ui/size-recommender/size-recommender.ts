import { Component, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ProductService } from '@modules/products/data-access/product.service';
import { SizeRecommendation } from '@modules/products/interfaces/product.interfaces';

@Component({
  selector: 'app-size-recommender',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './size-recommender.html',
  styleUrl: './size-recommender.scss',
})
export class SizeRecommender {
  private fb = inject(FormBuilder);
  private productService = inject(ProductService);

  productSlug = input.required<string>();

  sizeChosen = output<string>();

  isOpen = signal(false);
  isLoading = signal(false);
  result = signal<SizeRecommendation | null>(null);
  error = signal<string | null>(null);

  readonly builds = ['Slim', 'Average', 'Athletic', 'Curvy'];
  readonly fits = ['Snug', 'Regular', 'Relaxed'];

  form = this.fb.nonNullable.group({
    heightCm: [170, [Validators.required, Validators.min(120), Validators.max(220)]],
    weightKg: [70, [Validators.required, Validators.min(35), Validators.max(200)]],
    build: ['Average', Validators.required],
    fitPreference: ['Regular', Validators.required],
  });

  toggle(): void {
    this.isOpen.update((open) => !open);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    this.productService
      .recommendSize({
        productSlug: this.productSlug(),
        ...this.form.getRawValue(),
      })
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.isLoading.set(false);
        },
        error: () => {
          this.error.set("Couldn't get a recommendation right now. Please pick a size manually.");
          this.isLoading.set(false);
        },
      });
  }

  useSize(size: string): void {
    this.sizeChosen.emit(size);
    this.isOpen.set(false);
  }
}
