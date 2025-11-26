// src/pages/OrderSuccess.tsx
import React, { useEffect, useState } from 'react';
import { CheckCircle, Home, Mail, Package, Truck } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { getCookie } from '../utils/cookies';

interface CustomerData {
  customerName: string;
  email: string;
}

export default function OrderSuccess() {
  const navigate = useNavigate();
  const [customer, setCustomer] = useState<CustomerData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchCustomer = async () => {
      const token = getCookie('auth_token');
      if (!token) {
        setLoading(false);
        return;
      }

      try {
        const response = await fetch('http://localhost:5067/api/customers/me', {
          headers: { 'Authorization': `Bearer ${token}` },
        });

        if (response.ok) {
          const data = await response.json();
          setCustomer({
            customerName: data.data?.customerName || data.customerName || 'Khách hàng',
            email: data.data?.email || data.email || '',
          });
        }
      } catch (error) {
        console.error('Lỗi tải thông tin:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchCustomer();
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-purple-50 via-violet-50 to-indigo-50">
        <div className="w-12 h-12 border-4 border-purple-600 rounded-full border-t-transparent animate-spin"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen px-4 py-16 bg-gradient-to-br from-purple-50 via-violet-50 to-indigo-50 sm:px-6 lg:px-8">
      <div className="max-w-4xl mx-auto">
        {/* Icon thành công – tĩnh, rõ ràng */}
        <div className="flex justify-center mb-12">
          <div className="p-8 bg-white border-4 border-purple-200 rounded-full shadow-2xl">
            <CheckCircle className="w-24 h-24 text-purple-700" />
          </div>
        </div>

        {/* Tiêu đề – tím chủ đạo */}
        <h1 className="px-4 py-4 text-5xl font-extrabold leading-tight text-center text-transparent sm:text-6xl bg-gradient-to-r from-purple-700 via-violet-700 to-indigo-700 bg-clip-text">
          Đặt hàng thành công!
        </h1>

        {/* Lời cảm ơn – dễ đọc */}
        <p className="max-w-3xl px-6 mx-auto mt-8 text-xl font-medium leading-relaxed text-center text-gray-700 sm:text-2xl">
          Chào <span className="font-bold text-purple-700">{customer?.customerName || 'bạn'}</span>,<br />
          <span className="font-bold text-purple-600">Phonecare</span> xin chân thành cảm ơn sự tin tưởng của bạn!
        </p>

        {/* Card thông tin – màu tím nhẹ */}
        <div className="p-8 mt-12 bg-white border border-purple-100 shadow-2xl rounded-3xl sm:p-10">
          <div className="grid grid-cols-1 gap-8 sm:grid-cols-2">
            <div className="flex items-center gap-4 p-5 bg-gradient-to-r from-purple-50 to-violet-50 rounded-2xl">
              <div className="p-3 bg-white rounded-full shadow-md">
                <Package className="w-6 h-6 text-purple-700" />
              </div>
              <div>
                <p className="text-sm text-gray-600">Đơn hàng</p>
                <p className="font-bold text-purple-800">Đã được ghi nhận</p>
              </div>
            </div>

            <div className="flex items-center gap-4 p-5 bg-gradient-to-r from-indigo-50 to-purple-50 rounded-2xl">
              <div className="p-3 bg-white rounded-full shadow-md">
                <Truck className="w-6 h-6 text-indigo-700" />
              </div>
              <div>
                <p className="text-sm text-gray-600">Giao hàng</p>
                <p className="font-bold text-indigo-800">Sắp được xử lý</p>
              </div>
            </div>
          </div>

          {/* Email xác nhận */}
          {customer?.email && (
            <div className="p-6 mt-8 border border-purple-200 bg-gradient-to-r from-purple-50 to-violet-50 rounded-2xl">
              <div className="flex items-center gap-3">
                <Mail className="flex-shrink-0 w-5 h-5 text-purple-700" />
                <p className="text-sm sm:text-base">
                  Xác nhận đơn hàng đã được gửi đến: 
                  <span className="ml-1 font-bold text-purple-800">{customer.email}</span>
                </p>
              </div>
            </div>
          )}
        </div>

        {/* NÚT Ở GIỮA – TÍM CHỦ ĐẠO */}
        <div className="flex justify-center mt-16">
          <button
            onClick={() => navigate('/')}
            className="inline-flex items-center justify-center gap-3 px-12 py-5 text-xl font-bold text-white transition-all duration-200 rounded-full shadow-xl bg-gradient-to-r from-purple-700 via-violet-700 to-indigo-700 hover:shadow-2xl"
          >
            <Home className="w-6 h-6" />
            <span>Quay về trang chủ</span>
          </button>
        </div>

        {/* Footer – tím nhẹ */}
        <div className="px-4 mt-20 text-center">
          <p className="text-sm text-gray-500">
            Cần hỗ trợ? Liên hệ{' '}
            <a href="mailto:hotro@phonecare.vn" className="font-bold text-purple-700 hover:underline">
              hotro@phonecare.vn
            </a>
          </p>
          <p className="mt-2 text-xs text-gray-400">
            © 2025 <span className="font-bold text-purple-700">Phonecare</span> – Chăm sóc điện thoại, chăm sóc bạn.
          </p>
        </div>
      </div>
    </div>
  );
}