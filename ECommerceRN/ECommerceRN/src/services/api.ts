import axios from 'axios';
import { storage } from '../utils/storage';
import { 
  User, 
  Product, 
  Category, 
  CartItem, 
  CartSummary,
  Order, 
  AuthResponse, 
  LoginRequest, 
  RegisterRequest,
  PagedResult,
  ProductSearch,
  CreateOrderRequest,
  AddToCartRequest 
} from '../types';

// API Base URL - localhost'u gerçek IP adresi ile değiştirin
const API_BASE_URL = 'http://10.0.2.2:7038/api'; // Android emulator için
// const API_BASE_URL = 'http://192.168.1.100:7038/api'; // Gerçek cihaz için

// Axios instance oluştur
const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - her istekte token ekle
api.interceptors.request.use(
  async (config) => {
    console.log('🚀 API Request:', {
      url: config.url,
      method: config.method,
      baseURL: config.baseURL,
      data: config.data
    });
    
    const token = await storage.getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    console.error('❌ Request Error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor - 401 durumunda token temizle
api.interceptors.response.use(
  (response) => {
    console.log('✅ API Response:', {
      status: response.status,
      url: response.config.url,
      data: response.data
    });
    return response;
  },
  async (error) => {
    console.error('❌ API Error:', {
      status: error.response?.status,
      url: error.config?.url,
      message: error.message,
      data: error.response?.data
    });
    
    if (error.response?.status === 401) {
      await storage.clearAll();
    }
    return Promise.reject(error);
  }
);

// Auth Services
export const authService = {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await api.post('/auth/login', credentials);
    return response.data;
  },

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post('/auth/register', userData);
    return response.data;
  },

  async getCurrentUser(): Promise<User> {
    try {
      console.log('👤 Fetching current user from /auth/profile...');
      const response = await api.get('/auth/profile');
      console.log('👤 Profile API Response:', response.data);
      return response.data;
    } catch (error) {
      console.error('👤 Error fetching current user:', error);
      if (error instanceof Error) {
        console.error('👤 Error message:', error.message);
      }
      throw error; // Re-throw to be handled by the calling component
    }
  }
};

// Product Services
export const productService = {
  async getAllProducts(search?: ProductSearch): Promise<PagedResult<Product>> {
    const params = search ? {
      searchTerm: search.searchTerm,
      categoryId: search.categoryId,
      minPrice: search.minPrice,
      maxPrice: search.maxPrice,
      inStockOnly: search.inStockOnly,
      sortBy: search.sortBy,
      sortOrder: search.sortOrder,
      pageNumber: search.pageNumber,
      pageSize: search.pageSize
    } : undefined;

    const response = await api.get('/products', { params });
    console.log('📦 Products API Response:', response.data);
    return response.data;
  },

  async getSimpleProductList(): Promise<Product[]> {
    const response = await api.get('/products');
    console.log('📦 Simple Products API Response:', response.data);
    
    // Backend PagedResult döndürüyor, sadece data kısmını al
    if (response.data && response.data.data) {
      console.log('📦 Extracted Products Array:', response.data.data);
      return response.data.data;
    }
    
    // Fallback: eğer direkt array gelirse
    if (Array.isArray(response.data)) {
      console.log('📦 Direct Array Response:', response.data);
      return response.data;
    }
    
    console.warn('⚠️ Unexpected products response format:', response.data);
    return [];
  },

  async getProductById(id: number): Promise<Product> {
    const response = await api.get(`/products/${id}`);
    return response.data;
  },

  async getProductsByCategory(categoryId: number): Promise<Product[]> {
    const response = await api.get(`/products/category/${categoryId}`);
    console.log('📦 Products by Category API Response:', response.data);
    
    // Backend PagedResult döndürüyor, sadece data kısmını al
    if (response.data && response.data.data) {
      console.log('📦 Extracted Category Products Array:', response.data.data);
      return response.data.data;
    }
    
    // Fallback: eğer direkt array gelirse
    if (Array.isArray(response.data)) {
      console.log('📦 Direct Category Array Response:', response.data);
      return response.data;
    }
    
    console.warn('⚠️ Unexpected category products response format:', response.data);
    return [];
  }
};

// Category Services
export const categoryService = {
  async getAllCategories(): Promise<Category[]> {
    const response = await api.get('/categories');
    console.log('📦 Categories API Response:', response.data);
    
    // Backend PagedResult döndürüyor olabilir, data varsa al
    if (response.data && response.data.data) {
      console.log('📦 Extracted Categories Array:', response.data.data);
      return response.data.data;
    }
    
    // Fallback: eğer direkt array gelirse
    if (Array.isArray(response.data)) {
      console.log('📦 Direct Categories Array Response:', response.data);
      return response.data;
    }
    
    console.warn('⚠️ Unexpected categories response format:', response.data);
    return [];
  },

  async getCategoryById(id: number): Promise<Category> {
    const response = await api.get(`/categories/${id}`);
    console.log('📦 Category by ID API Response:', response.data);
    return response.data;
  }
};

// Cart Services
export const cartService = {
  async getCartSummary(): Promise<CartSummary> {
    try {
      console.log('🛒 Fetching cart summary from API...');
      
      // Check auth token first
      const token = await storage.getAuthToken();
      if (!token) {
        console.error('🛒 No auth token found');
        throw new Error('Giriş yapmış kullanıcı bulunamadı. Lütfen önce giriş yapın.');
      }
      
      const response = await api.get('/cart');
      console.log('🛒 Cart Summary API Response:', response.data);
      
      // Backend CartSummaryDto döndürüyor
      if (response.data) {
        return response.data;
      }
      
      console.warn('⚠️ Unexpected cart response format:', response.data);
      return {
        items: [],
        totalItems: 0,
        totalAmount: 0,
        shippingCost: 0,
        taxAmount: 0,
        grandTotal: 0
      };
    } catch (error: any) {
      console.error('🛒 Error fetching cart summary:', error);
      
      // Specific error handling
      if (error.response) {
        const status = error.response.status;
        const message = error.response.data?.message || error.response.statusText;
        
        console.error('🛒 HTTP Error:', { status, message, data: error.response.data });
        
        if (status === 401) {
          throw new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        } else if (status === 404) {
          throw new Error('Sepet servisi bulunamadı. API bağlantısını kontrol edin.');
        } else if (status === 500) {
          throw new Error('Sunucu hatası. Lütfen daha sonra tekrar deneyin.');
        } else {
          throw new Error(`Sepet yüklenirken hata oluştu: ${message}`);
        }
      } else if (error.request) {
        console.error('🛒 Network Error:', error.request);
        throw new Error('Ağ bağlantısı hatası. İnternet bağlantınızı kontrol edin.');
      } else {
        console.error('🛒 Unknown Error:', error.message);
        throw new Error(error.message || 'Bilinmeyen hata oluştu.');
      }
    }
  },

  async getCartItems(): Promise<CartItem[]> {
    const cartSummary = await this.getCartSummary();
    return cartSummary.items;
  },

  async addToCart(request: AddToCartRequest): Promise<CartSummary> {
    try {
      console.log('🛒 Adding to cart:', request);
      
      // Check if we have a token
      const token = await storage.getAuthToken();
      console.log('🛒 Auth token available for addToCart:', token ? 'Yes' : 'No');
      
      const response = await api.post('/cart/add', request);
      console.log('🛒 Add to Cart API Response:', response.data);
      
      // Backend returns CartSummaryDto
      return response.data;
    } catch (error) {
      console.error('🛒 Error adding to cart:', error);
      if (error instanceof Error) {
        console.error('🛒 Error message:', error.message);
      }
      throw error; // Re-throw to be handled by the calling component
    }
  },

  async updateCartItem(id: number, quantity: number): Promise<CartSummary> {
    const response = await api.put(`/cart/${id}`, { quantity });
    console.log('🛒 Update Cart Item API Response:', response.data);
    return response.data;
  },

  async removeFromCart(id: number): Promise<void> {
    await api.delete(`/cart/${id}`);
    console.log('🛒 Item removed from cart');
  },

  async clearCart(): Promise<void> {
    await api.delete('/cart');
    console.log('🛒 Cart cleared');
  }
};

// Order Services
export const orderService = {
  async getOrders(): Promise<PagedResult<Order>> {
    const response = await api.get('/orders');
    console.log('📋 Orders API Response:', response.data);
    return response.data;
  },

  async getSimpleOrderList(): Promise<Order[]> {
    const response = await api.get('/order');
    console.log('📋 Simple Orders API Response:', response.data);
    
    // Backend PagedResult döndürüyor olabilir, data varsa al
    if (response.data && response.data.data) {
      console.log('📋 Extracted Orders Array:', response.data.data);
      return response.data.data;
    }
    
    // Fallback: eğer direkt array gelirse
    if (Array.isArray(response.data)) {
      console.log('📋 Direct Orders Array Response:', response.data);
      return response.data;
    }
    
    console.warn('⚠️ Unexpected orders response format:', response.data);
    return [];
  },

  async getOrderById(id: number): Promise<Order> {
    const response = await api.get(`/order/${id}`);
    console.log('📋 Order by ID API Response:', response.data);
    return response.data;
  },

  async createOrder(orderData: CreateOrderRequest): Promise<Order> {
    const response = await api.post('/order', orderData);
    console.log('📋 Create Order API Response:', response.data);
    return response.data;
  },

  async createOrderFromCart(): Promise<Order> {
    // Checkout için gerekli minimum bilgileri gönder
    const checkoutData = {
      ShippingAddress: "Varsayılan teslimat adresi", // Bu sonradan dinamik yapılabilir
      ShippingCity: "İstanbul",
      PaymentMethod: "Kapıda Ödeme",
      OrderItems: [] // Backend sepetten alacak
    };
    
    const response = await api.post('/order/create-from-cart', checkoutData);
    console.log('📋 Create Order from Cart API Response:', response.data);
    return response.data;
  }
};

export { api };
