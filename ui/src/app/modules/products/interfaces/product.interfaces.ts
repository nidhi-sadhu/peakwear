export interface ProductListItem {
  id: string;
  name: string;
  slug: string;
  category: string;
  basePrice: number;
  imageUrl: string | null;
  colours: string[];
}

export interface VariantResponse {
  id: string;
  colour: string;
  size: string;
  sku: string;
  imageUrl: string | null;
  stock: number;
  inStock: boolean;
}

export interface ProductDetail {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  category: string;
  basePrice: number;
  variants: VariantResponse[];
}

export interface ProductState {
  products: ProductListItem[];
  selected: ProductDetail | null;
  isLoading: boolean;
  error: string | null;
}

export interface SizeRecommendationRequest {
  productSlug: string;
  heightCm: number;
  weightKg: number;
  build: string;
  fitPreference: string;
}

export interface SizeRecommendation {
  recommendedSize: string;
  reasoning: string;
  alternative: string | null;
}
