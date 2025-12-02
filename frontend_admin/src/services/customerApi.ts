// src/services/customerApi.ts
import { getCookie } from '@/utils/cookies';
import { notify } from '@/utils/notify';

const API_BASE = 'http://localhost:5067';

const authFetch = async (url: string, options: RequestInit = {}) => {
  const token = getCookie('auth_token'); // Token JWT của bạn

  const response = await fetch(`${API_BASE}${url}`, {
    ...options,
    // KHÔNG DÙNG credentials: 'include' → vì bạn dùng Bearer Token, không dùng cookie session
    headers: {
      'Content-Type': 'application/json',
      // Chỉ cần Bearer Token là đủ → backend của bạn đang dùng JWT
      ...(token && { Authorization: `Bearer ${token}` }),
      ...options.headers,
    },
  });

  // Xử lý hết hạn token
  if (response.status === 401 || response.status === 403) {
    notify.error('Phiên đăng nhập đã hết hạn');
    setTimeout(() => {
      window.location.href = '/admin-login';
    }, 1500);
    throw new Error('Unauthorized');
  }

  if (!response.ok) {
    let message = 'Lỗi kết nối server';
    try {
      const err = await response.json();
      message = err.message || message;
    } catch {}
    throw new Error(message);
  }

  return response.json();
};

export const customerApi = {
  getCustomers: (page = 1, search = '', status: boolean | null = null) => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: '20',
      ...(search && { search }),
      ...(status !== null && { status: status.toString() }),
    });
    return authFetch(`/api/admin/customers?${params.toString()}`);
  },

  getCustomerInvoices: (customerId: string) =>
    authFetch(`/api/admin/invoices/customer/${customerId}`),

  getInvoiceDetail: (invoiceId: number) =>
    authFetch(`/api/invoices/${invoiceId}`),

  toggleCustomerStatus: (customerId: string) =>
    authFetch(`/api/admin/customers/${customerId}/toggle-status`, {
      method: 'POST',
    }),
};