import { useState } from 'react';
import { ArrowLeft, Mail, Lock, User } from 'lucide-react';

interface LoginPageProps {
  onNavigate: (page: 'home' | 'register' | 'profile') => void;
}

export default function LoginPage({ onNavigate }: LoginPageProps) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!username || !password) {
      alert('Vui lòng nhập đầy đủ thông tin');
      return;
    }

    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      alert('Đăng nhập thành công!');
      onNavigate('profile');
    }, 1500);
  };

  const handleGoogleLogin = () => {
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      alert('Đăng nhập thành công!');
      onNavigate('profile');
    }, 1500);
  };

  return (
    <div className="min-h-screen px-4 py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-12 sm:px-6 lg:px-8">
      <div className="container max-w-md mx-auto">
        {/* Back Button */}
        <button
          onClick={() => onNavigate('home')}
          className="flex items-center gap-1.5 sm:gap-2 px-3 sm:px-4 py-2 sm:py-2.5 mb-6 sm:mb-8 text-xs sm:text-sm font-medium text-violet-800 transition-all bg-white hover:bg-violet-50 shadow-sm hover:shadow-md"
          style={{ borderRadius: '12px' }}
        >
          <ArrowLeft className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
          Quay lại
        </button>

        {/* Login Card */}
        <div 
          className="p-6 bg-white border shadow-2xl border-violet-100/50 sm:p-8 md:p-10"
          style={{ borderRadius: '20px' }}
        >
          {/* Header */}
          <div className="mb-6 text-center sm:mb-8">
            <div 
              className="inline-flex items-center justify-center w-12 h-12 mb-3 shadow-lg sm:w-16 sm:h-16 bg-gradient-to-br from-violet-600 to-violet-800 sm:mb-4"
              style={{ borderRadius: '16px' }}
            >
              <Mail className="w-6 h-6 text-white sm:w-8 sm:h-8" />
            </div>
            <h1 className="text-2xl sm:text-3xl font-bold mb-1.5 sm:mb-2 bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text text-transparent">
              Đăng nhập
            </h1>
            <p className="text-sm font-medium text-gray-600 sm:text-base">Chào mừng bạn quay trở lại!</p>
          </div>

          <div className="space-y-4 sm:space-y-5">
            {/* Username Input */}
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
                  className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
              </div>
            </div>

            {/* Password Input */}
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
                  className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
              </div>
            </div>

            {/* Forgot Password */}
            <div className="flex justify-end">
              <button
                type="button"
                className="text-xs font-medium transition-colors sm:text-sm text-violet-700 hover:text-violet-900"
              >
                Quên mật khẩu?
              </button>
            </div>

            {/* Login Button */}
            <button
              onClick={handleLogin}
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
                  Đang đăng nhập...
                </span>
              ) : (
                'Đăng nhập'
              )}
            </button>

            {/* Divider */}
            <div className="relative py-3 sm:py-4">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-violet-100"></div>
              </div>
              <div className="relative flex justify-center">
                <span className="px-3 text-xs font-medium text-gray-500 bg-white sm:px-4 sm:text-sm">Hoặc</span>
              </div>
            </div>

            {/* Google Login */}
            <button
              onClick={handleGoogleLogin}
              disabled={loading}
              className="w-full py-3 sm:py-3.5 text-xs sm:text-sm font-semibold text-gray-700 bg-white border-2 border-violet-200 hover:border-violet-400 hover:bg-violet-50 shadow-sm hover:shadow-md hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-60 disabled:cursor-not-allowed disabled:hover:scale-100"
              style={{ borderRadius: '14px' }}
            >
              <span className="flex items-center justify-center gap-2 sm:gap-2.5">
                <svg className="flex-shrink-0 w-4 h-4 sm:w-5 sm:h-5" viewBox="0 0 24 24">
                  <path
                    fill="#4285F4"
                    d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                  />
                  <path
                    fill="#34A853"
                    d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                  />
                  <path
                    fill="#FBBC05"
                    d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                  />
                  <path
                    fill="#EA4335"
                    d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                  />
                </svg>
                <span>Đăng nhập bằng Google</span>
              </span>
            </button>
          </div>

          {/* Register Link */}
          <div className="pt-5 mt-6 text-center border-t sm:mt-8 sm:pt-6 border-violet-100">
            <p className="text-xs font-medium text-gray-600 sm:text-sm">
              Chưa có tài khoản?{' '}
              <button
                onClick={() => onNavigate('register')}
                className="font-semibold transition-colors text-violet-700 hover:text-violet-900"
              >
                Đăng ký ngay
              </button>
            </p>
          </div>

          {/* Promotion Banner */}
          <div 
            className="p-3 mt-5 text-center text-white sm:mt-6 sm:p-4 bg-gradient-to-r from-violet-600 to-violet-800"
            style={{ borderRadius: '14px' }}
          >
            <p className="text-xs font-semibold mb-0.5 sm:mb-1">🎉 Khuyến mãi đặc biệt</p>
            <p className="text-[10px] sm:text-xs text-violet-100">Giảm 20% cho đơn hàng đầu tiên khi đăng ký mới</p>
          </div>
        </div>
      </div>
    </div>
  );
}