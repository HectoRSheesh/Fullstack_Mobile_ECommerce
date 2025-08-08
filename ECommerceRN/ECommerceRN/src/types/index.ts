export interface User {
  id: number;
  fullName: string;
  email: string;
  phone?: string;
  address?: string;
  createdAt: string;
  lastLoginAt?: string;
  isActive: boolean;
}

export interface Product {
  id: number;
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
  categoryName?: string;
  imageUrl?: string;
  sku?: string;
  isActive: boolean;
  createdAt: string;
  inStock?: boolean;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  slug?: string;
  parentCategoryId?: number;
  parentCategoryName?: string;
  sortOrder: number;
  isActive: boolean;
  productCount: number;
  subCategories?: Category[];
}

export interface CartItem {
  id: number;
  productId: number;
  productName: string;
  productPrice: number;
  productImageUrl?: string;
  quantity: number;
  totalPrice: number;
  isAvailable: boolean;
  availableStock: number;
}

export interface CartSummary {
  items: CartItem[];
  totalItems: number;
  totalAmount: number;
  shippingCost: number;
  taxAmount: number;
  grandTotal: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  orderDate: string;
  status: string;
  totalAmount: number;
  shippingCost: number;
  taxAmount: number;
  shippingAddress: string;
  shippingCity: string;
  shippingPostalCode?: string;
  shippingPhone?: string;
  paymentMethod?: string;
  isPaid: boolean;
  paidAt?: string;
  notes?: string;
  totalItems: number;
  items: OrderItem[];
  canBeCancelled: boolean;
}

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productSKU?: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  notes?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  message: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  phone?: string;
  address?: string;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ProductSearch {
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  inStockOnly?: boolean;
  sortBy?: string;
  sortOrder?: string;
  pageNumber: number;
  pageSize: number;
}

export interface CreateOrderRequest {
  shippingAddress: string;
  shippingCity: string;
  shippingPostalCode?: string;
  shippingPhone?: string;
  paymentMethod?: string;
  notes?: string;
}

export interface AddToCartRequest {
  productId: number;
  quantity: number;
}
