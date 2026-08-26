export interface CartItem {
  id: string;
  productVariantId: string;
  productName: string;
  productSlug: string;
  colour: string;
  size: string;
  sku: string;
  imageUrl: string | null;
  unitPrice: number;
  quantity: number;
  stockAvailable: number;
  lineTotal: number;
}

export interface Cart {
  items: CartItem[];
  itemCount: number;
  subtotal: number;
}

export interface CartState {
  cart: Cart | null;
  isLoading: boolean;
  error: string | null;
}
