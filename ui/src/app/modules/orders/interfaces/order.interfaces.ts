export interface OrderItem {
  id: string;
  productName: string;
  colour: string;
  size: string;
  sku: string;
  imageUrl: string | null;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  status: string;
  subtotal: number;
  shippingCost: number;
  total: number;
  shippingAddress: string;
  createdAtUtc: string;
  items: OrderItem[];
}

export interface OrderState {
  orders: Order[];
  lastOrder: Order | null;
  isPlacing: boolean;
  isLoading: boolean;
  error: string | null;
}
