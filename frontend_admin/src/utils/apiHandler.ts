import { getCookie, deleteCookie } from './cookies';
import { notify } from './notify';

interface ApiResponse<T = unknown> {
  message?: string;
  data?: T;
}

export const handleApiError = (response: Response, data: ApiResponse): boolean => {
  if (response.status === 401 || response.status === 403) {
    deleteCookie('auth_token');
    notify.error(data.message || 'Phiên đăng nhập đã hết hạn');
    
    setTimeout(() => {
      window.location.href = '/admin-login';
    }, 500);
    
    return true;
  }
  
  return false;
};

export const apiRequest = async <T = unknown>(
  url: string,
  options: RequestInit = {}
): Promise<{ ok: boolean; data: ApiResponse<T>; response: Response }> => {
  const token = getCookie('auth_token');
  
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
    ...(options.headers || {}),
  };

  try {
    const response = await fetch(url, {
      ...options,
      headers,
    });

    const data: ApiResponse<T> = await response.json();

    if (!response.ok) {
      handleApiError(response, data);
    }

    return { ok: response.ok, data, response };
  } catch (error) {
    console.error('API request error:', error);
    throw error;
  }
};

export const api = {
  get: async <T = unknown>(url: string, options?: RequestInit) => {
    return apiRequest<T>(url, { ...options, method: 'GET' });
  },
  
  post: async <T = unknown, B = unknown>(url: string, body?: B, options?: RequestInit) => {
    return apiRequest<T>(url, {
      ...options,
      method: 'POST',
      body: body ? JSON.stringify(body) : undefined,
    });
  },
  
  put: async <T = unknown, B = unknown>(url: string, body?: B, options?: RequestInit) => {
    return apiRequest<T>(url, {
      ...options,
      method: 'PUT',
      body: body ? JSON.stringify(body) : undefined,
    });
  },
  
  delete: async <T = unknown>(url: string, options?: RequestInit) => {
    return apiRequest<T>(url, { ...options, method: 'DELETE' });
  },
};