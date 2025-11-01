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

  getBrandsBySubCategory: (slug: string) =>
    authFetch(`${API_BASE}/api/products/subcategory/${slug}/brands`),

  getProductCard: (id: number) =>
    authFetch(`${API_BASE}/api/products/card/${id}`),

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
    const response = await fetch(`${API_BASE}/api/products`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });

    if (!response.ok) {
        const error = await response.json().catch(() => ({}));
        throw new Error(error.message || 'Tạo sản phẩm thất bại');
    }

    const result = await response.json();
    return result.data.productId;
    },
};