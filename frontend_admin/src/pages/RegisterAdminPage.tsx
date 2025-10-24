import React, { useState } from 'react';
import { User, Lock } from 'lucide-react';
import { setCookie } from '../utils/cookies';
import { notify } from '../components/NotificationProvider';
import { useNavigate } from 'react-router-dom';

export default function RegisterAdminPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleRegister = async () => {
    const trimmedUsername = username.trim();
    if (!trimmedUsername || !password) {
      notify('warning', 'Vui lòng nhập đầy đủ thông tin');
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:5067/api/administrators', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Username: trimmedUsername,
          Password: password,
        }),
      });

      const data = await response.json();

      if (response.ok && data.data?.username) {
        setCookie('auth_token', data.data.token || 'admin_token', 7);
        notify('success', data.message || 'Tạo tài khoản quản trị viên thành công');
        setTimeout(() => {
          navigate('/admin');
        }, 1000);
      } else {
        notify('error', data.message || 'Tạo tài khoản thất bại');
      }
    } catch (error) {
      notify('error', 'Không thể kết nối đến server');
      console.error('Register error:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleRegister();
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-12 sm:px-6 lg:px-8">
      <div className="container max-w-md mx-auto">
        <div 
          className="p-6 bg-white border shadow-2xl border-violet-100/50 sm:p-8 md:p-10"
          style={{ borderRadius: '20px' }}
        >
          <div className="mb-6 text-center sm:mb-8">
            <div 
              className="inline-flex items-center justify-center w-12 h-12 mb-3 shadow-lg sm:w-16 sm:h-16 bg-gradient-to-br from-violet-600 to-violet-800 sm:mb-4"
              style={{ borderRadius: '16px' }}
            >
              <User className="w-6 h-6 text-white sm:w-8 sm:h-8" />
            </div>
            <h1 className="text-2xl sm:text-3xl font-bold mb-1.5 sm:mb-2 bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text text-transparent">
              Đăng ký quản trị viên
            </h1>
            <p className="text-sm font-medium text-gray-600 sm:text-base">Tạo tài khoản quản trị viên để quản lý hệ thống</p>
          </div>

          <div className="space-y-4 sm:space-y-5">
            <div className="space-y-1.5 sm:space-y-2">
              <label htmlFor="username" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                Tên đăng nhập
              </label>
              <div className="relative">
                <User className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                <input
                  id="username"
                  type="text"
                  placeholder="Nhập tên đăng nhập"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  onKeyPress={handleKeyPress}
                  className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
              </div>
            </div>

            <div className="space-y-1.5 sm:space-y-2">
              <label htmlFor="password" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                Mật khẩu
              </label>
              <div className="relative">
                <Lock className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                <input
                  id="password"
                  type="password"
                  placeholder="Nhập mật khẩu"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  onKeyPress={handleKeyPress}
                  className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
              </div>
            </div>

            <button
              onClick={handleRegister}
              disabled={loading}
              className="w-full py-3 sm:py-3.5 text-xs sm:text-sm font-semibold text-white bg-gradient-to-r from-violet-700 to-violet-800 hover:from-violet-800 hover:to-violet-900 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-60 disabled:cursor-not-allowed disabled:hover:scale-100"
              style={{ borderRadius: '14px' }}
            >
              {loading ? (
                <span className="flex items-center justify-center gap-2">
                  <svg className="w-4 h-4 text-white animate-spin sm:h-5 sm:w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Đang đăng ký...
                </span>
              ) : (
                'Đăng ký'
              )}
            </button>
          </div>

          <div className="pt-5 mt-6 text-center border-t sm:mt-8 sm:pt-6 border-violet-100">
            <p className="text-xs font-medium text-gray-600 sm:text-sm">
              Đã có tài khoản?{' '}
              <button
                onClick={() => navigate('/admin-login')}
                className="font-semibold transition-colors text-violet-700 hover:text-violet-900"
              >
                Đăng nhập ngay
              </button>
            </p>
          </div>

          <div 
            className="p-3 mt-5 text-center text-white sm:mt-6 sm:p-4 bg-gradient-to-r from-violet-600 to-violet-800"
            style={{ borderRadius: '14px' }}
          >
            <p className="text-xs font-semibold mb-0.5 sm:mb-1">🎉 Chào mừng quản trị viên</p>
            <p className="text-[10px] sm:text-xs text-violet-100">Tạo tài khoản để quản lý hệ thống</p>
          </div>
        </div>
      </div>
    </div>
  );
}