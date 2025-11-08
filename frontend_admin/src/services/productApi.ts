// src/services/productApi.ts

import { getCookie } from '../utils/cookies';
import { notify } from '../utils/notify';
import { 
    CreateCategoryDto, 
    UpdateCategoryDto, 
    CreateSubCategoryDto, 
    UpdateSubCategoryDto,
    CreateVariantDto,
} from '../types/product.types';

const API_BASE = 'http://localhost:5067';

const getAuthHeaders = () => {
  const token = getCookie('auth_token');
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
  };
};

export const authFetch = async (url: string, options: RequestInit = {}) => {
  const response = await fetch(url, {
    ...options,
    headers: {
      ...getAuthHeaders(),
      ...options.headers,
    },
  });

  if (response.status === 401 || response.status === 403) {
    notify.error('Phiên đăng nhập đã hết hạn');
    setTimeout(() => {
      window.location.href = '/admin-login';
    }, 500);
    throw new Error('Unauthorized');
  }

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    throw new Error(errorData.message || 'Có lỗi xảy ra');
  }

  return response.json();
};

export const api = {
  // ==================== CATEGORY APIs ====================
  getCategories: () => 
    authFetch(`${API_BASE}/api/categories/with-subcategories`),
  
  createCategory: (data: CreateCategoryDto) => 
    authFetch(`${API_BASE}/api/categories`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  
  updateCategory: (id: number, data: UpdateCategoryDto) => 
    authFetch(`${API_BASE}/api/categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  
  deleteCategory: (id: number) => 
    authFetch(`${API_BASE}/api/categories/${id}`, {
      method: 'DELETE',
    }),

  // ==================== SUB-CATEGORY APIs ====================
  createSubCategory: (data: CreateSubCategoryDto) => 
    authFetch(`${API_BASE}/api/categories/subcategories`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  
  updateSubCategory: (id: number, data: UpdateSubCategoryDto) => 
    authFetch(`${API_BASE}/api/categories/subcategories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  
  deleteSubCategory: (id: number) => 
    authFetch(`${API_BASE}/api/categories/subcategories/${id}`, {
      method: 'DELETE',
    }),

  // ==================== PRODUCT APIs ====================
  
  // Tạo sản phẩm mới
  createProduct: async (data: {
    id: number;
    name: string;
    slug: string;
    brand: string;
    description: string;
    subCategoryId: number;
    attributeOptions: Record<string, string[]>;
    variants: CreateVariantDto[];
  }): Promise<number> => {
    const response = await authFetch(`${API_BASE}/api/products`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return response.data.productId;
  },

  // Lấy chi tiết sản phẩm theo ID
  getProductDetailById: (id: number) =>
    authFetch(`${API_BASE}/api/products/${id}`),

  // Lấy chi tiết sản phẩm theo slug
  getProductDetailBySlug: (slug: string) =>
    authFetch(`${API_BASE}/api/products/slug/${slug}`),

  // Cập nhật sản phẩm
  updateProduct: (id: number, payload: {
    name: string;
    slug: string;
    brand: string;
    description: string;
    attributeOptions: Record<string, string[]>;
    variants: CreateVariantDto[];
  }) => 
    authFetch(`${API_BASE}/api/products/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }),

  // Xóa sản phẩm
  deleteProduct: (id: number) =>
    authFetch(`${API_BASE}/api/products/${id}`, {
      method: 'DELETE',
    }),

  // Lấy product card theo ID
  getProductCard: (id: number) =>
    authFetch(`${API_BASE}/api/products/card/${id}`),

  // Lấy product card theo slug
  getProductCardBySlug: (slug: string) =>
    authFetch(`${API_BASE}/api/products/card/slug/${slug}`),

  // Lấy sản phẩm theo sub-category (GIỮ NGUYÊN)
  getProductsBySubCategory: (slug: string, params?: {
    brand?: string;
    minPrice?: number;
    maxPrice?: number;
    sortByPriceAscending?: boolean;
  }) => {
    const queryParams = new URLSearchParams();
    if (params?.brand) queryParams.append('brand', params.brand);
    if (params?.minPrice !== undefined) queryParams.append('minPrice', params.minPrice.toString());
    if (params?.maxPrice !== undefined) queryParams.append('maxPrice', params.maxPrice.toString());
    if (params?.sortByPriceAscending !== undefined) queryParams.append('sortByPriceAscending', params.sortByPriceAscending.toString());
    
    const queryString = queryParams.toString();
    return authFetch(`${API_BASE}/api/products/subcategory/${slug}${queryString ? `?${queryString}` : ''}`);
  },

  // Lấy brands theo sub-category (GIỮ NGUYÊN)
  getBrandsBySubCategory: (slug: string) =>
    authFetch(`${API_BASE}/api/products/subcategory/${slug}/brands`),

  // TÌM KIẾM SẢN PHẨM (cũ)
  searchProducts: (params: {
    keyword: string;
    sortByPriceAscending?: boolean;
    minPrice?: number;
    maxPrice?: number;
  }) => {
    const queryParams = new URLSearchParams();
    queryParams.append('keyword', params.keyword);
    if (params.sortByPriceAscending !== undefined) queryParams.append('sortByPriceAscending', params.sortByPriceAscending.toString());
    if (params.minPrice !== undefined) queryParams.append('minPrice', params.minPrice.toString());
    if (params.maxPrice !== undefined) queryParams.append('maxPrice', params.maxPrice.toString());
    
    return authFetch(`${API_BASE}/api/products/search?${queryParams.toString()}`);
  },

  // LỌC SẢN PHẨM NÂNG CAO
  filterProducts: (payload: {
    brands?: string[];
    subCategorySlugs?: string[];
    minPrice?: number;
    maxPrice?: number;
    inStock?: boolean;
    onSale?: boolean;
    page?: number;
    pageSize?: number;
    sortBy?: string;
  }) =>
    authFetch(`${API_BASE}/api/products/filter`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  // === MỚI: TÌM KIẾM TẤT CẢ SẢN PHẨM (KỂ CẢ NGỪNG KINH DOANH) ===
  searchAll: (params: {
    keyword?: string;           // Có thể chứa slug danh mục hoặc tên sản phẩm
    brand?: string;
    minPrice?: number;
    maxPrice?: number;
    sortByPriceAscending?: boolean;
  }) => {
    const queryParams = new URLSearchParams();
    
    if (params.keyword) queryParams.append('Keyword', params.keyword);
    if (params.brand) queryParams.append('Brand', params.brand);
    if (params.minPrice !== undefined) queryParams.append('MinPrice', params.minPrice.toString());
    if (params.maxPrice !== undefined) queryParams.append('MaxPrice', params.maxPrice.toString());
    if (params.sortByPriceAscending !== undefined) {
      queryParams.append('SortByPriceAscending', params.sortByPriceAscending.toString());
    }

    const queryString = queryParams.toString();
    return authFetch(`${API_BASE}/api/products/search-all${queryString ? `?${queryString}` : ''}`);
  },

  // ==================== VARIANT APIs ====================

  getVariantInfoById: (productId: number, variantSlug: string) =>
    authFetch(`${API_BASE}/api/products/${productId}/variants/${variantSlug}`),

  getVariantInfoBySlug: (productSlug: string, variantSlug: string) =>
    authFetch(`${API_BASE}/api/products/slug/${productSlug}/variants/${variantSlug}`),

  updateVariantImages: async (productSlug: string, variantSlug: string, images: File[]) => {
    const formData = new FormData();
    images.forEach(image => formData.append('images', image));

    const token = getCookie('auth_token');
    const response = await fetch(`${API_BASE}/api/products/slug/${productSlug}/variants/${variantSlug}/images`, {
      method: 'PUT',
      headers: {
        ...(token && { 'Authorization': `Bearer ${token}` }),
      },
      body: formData,
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      throw new Error(error.message || 'Cập nhật hình ảnh thất bại');
    }

    return response.json();
  },

  updateVariantPrice: (productSlug: string, variantSlug: string, data: {
    originalPrice: number;
    discountedPrice: number;
  }) =>
    authFetch(`${API_BASE}/api/products/slug/${productSlug}/variants/${variantSlug}/price`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  bulkUpdatePrices: (updates: Array<{
    productSlug: string;
    variantSlug: string;
    originalPrice: number;
    discountedPrice: number;
  }>) =>
    authFetch(`${API_BASE}/api/products/bulk-update-prices`, {
      method: 'POST',
      body: JSON.stringify(updates),
    }),

  updateIsDiscontinued: (productSlug: string, isDiscontinued: boolean) =>
    authFetch(`${API_BASE}/api/products/slug/${productSlug}/discontinued`, {
      method: 'PATCH',
      body: JSON.stringify({ isDiscontinued }),
    }),
};