import React, { useState } from 'react';
import { ArrowLeft, Lock, Eye, EyeOff } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { getCookie, deleteCookie } from '../utils/cookies';
import { notify } from '../components/NotificationProvider';
import { isTokenValid } from '../utils/auth';

export default function ChangePasswordPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showOldPassword, setShowOldPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  React.useEffect(() => {
    if (!isTokenValid()) {
      notify('warning', 'Phiên đăng nhập hết hạn');
      navigate('/login');
    }
  }, []);

  const handleChangePassword = async () => {
    // Validate input
    if (!oldPassword || !newPassword || !confirmPassword) {
      notify('warning', 'Vui lòng nhập đầy đủ thông tin');
      return;
    }

    if (newPassword !== confirmPassword) {
      notify('warning', 'Mật khẩu mới không khớp');
      return;
    }

    if (newPassword.length < 6) {
      notify('warning', 'Mật khẩu mới phải có ít nhất 6 ký tự');
      return;
    }

    if (newPassword === oldPassword) {
      notify('warning', 'Mật khẩu mới phải khác mật khẩu cũ');
      return;
    }

    const token = getCookie('auth_token');
    if (!token) {
      notify('warning', 'Vui lòng đăng nhập lại');
      navigate('/login');
      return;
    }

    setLoading(true);

    try {
      // Step 1: Change password
      const response = await fetch('http://localhost:5067/api/customers/password', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          OldPassword: oldPassword,
          NewPassword: newPassword,
        }),
      });

      const data = await response.json();

      if (response.ok) {
        notify('success', data.message);

        // Step 2: Logout from other devices
        try {
          await fetch('http://localhost:5067/api/customers/logout/other-devices', {
            method: 'POST',
            headers: {
              'Authorization': `Bearer ${token}`,
            },
          });
        } catch (error) {
          console.error('Logout other devices error:', error);
        }

        // Step 3: Logout from current device
        try {
          await fetch('http://localhost:5067/api/customers/logout', {
            method: 'POST',
            headers: {
              'Authorization': `Bearer ${token}`,
            },
          });
        } catch (error) {
          console.error('Logout error:', error);
        }

        // Clear cookie and redirect
        deleteCookie('auth_token');
        notify('success', 'Vui lòng đăng nhập lại với mật khẩu mới');
        
        setTimeout(() => {
          navigate('/login');
        }, 1500);
      } else {
        notify('error', data.message);
      }
    } catch (error) {
      notify('error', 'Không thể kết nối đến server');
      console.error('Change password error:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleChangePassword();
    }
  };

  return (
    <div className="min-h-screen px-4 py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-12 sm:px-6 lg:px-8">
      <div className="container max-w-md mx-auto">
        <button
          onClick={() => navigate('/profile')}
          className="flex items-center gap-1.5 sm:gap-2 px-3 sm:px-4 py-2 sm:py-2.5 mb-6 sm:mb-8 text-xs sm:text-sm font-medium text-violet-800 transition-all bg-white hover:bg-violet-50 shadow-sm hover:shadow-md"
          style={{ borderRadius: '12px' }}
        >
          <ArrowLeft className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
          Quay lại
        </button>

        <div 
          className="p-6 bg-white border shadow-2xl border-violet-100/50 sm:p-8 md:p-10"
          style={{ borderRadius: '20px' }}
        >
          <div className="mb-6 text-center sm:mb-8">
            <div 
              className="inline-flex items-center justify-center w-12 h-12 mb-3 shadow-lg sm:w-16 sm:h-16 bg-gradient-to-br from-violet-600 to-violet-800 sm:mb-4"
              style={{ borderRadius: '16px' }}
            >
              <Lock className="w-6 h-6 text-white sm:w-8 sm:h-8" />
            </div>
            <h1 className="text-2xl sm:text-3xl font-bold mb-1.5 sm:mb-2 bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text text-transparent">
              Thay đổi mật khẩu
            </h1>
            <p className="text-sm font-medium text-gray-600 sm:text-base">
              Bảo vệ tài khoản của bạn
            </p>
          </div>

          <div className="space-y-4 sm:space-y-5">
            {/* Old Password */}
            <div className="space-y-1.5 sm:space-y-2">
              <label htmlFor="oldPassword" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                Mật khẩu cũ
              </label>
              <div className="relative">
                <Lock className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                <input
                  id="oldPassword"
                  type={showOldPassword ? 'text' : 'password'}
                  placeholder="Nhập mật khẩu cũ"
                  value={oldPassword}
                  onChange={(e) => setOldPassword(e.target.value)}
                  onKeyPress={handleKeyPress}
                  className="w-full pl-10 sm:pl-12 pr-10 sm:pr-12 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
                <button
                  type="button"
                  onClick={() => setShowOldPassword(!showOldPassword)}
                  className="absolute -translate-y-1/2 right-3 sm:right-4 top-1/2"
                >
                  {showOldPassword ? (
                    <EyeOff className="w-4 h-4 text-gray-400 sm:w-5 sm:h-5" />
                  ) : (
                    <Eye className="w-4 h-4 text-gray-400 sm:w-5 sm:h-5" />
                  )}
                </button>
              </div>
            </div>

            {/* New Password */}
            <div className="space-y-1.5 sm:space-y-2">
              <label htmlFor="newPassword" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                Mật khẩu mới
              </label>
              <div className="relative">
                <Lock className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                <input
                  id="newPassword"
                  type={showNewPassword ? 'text' : 'password'}
                  placeholder="Nhập mật khẩu mới"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  onKeyPress={handleKeyPress}
                  className="w-full pl-10 sm:pl-12 pr-10 sm:pr-12 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
                <button
                  type="button"
                  onClick={() => setShowNewPassword(!showNewPassword)}
                  className="absolute -translate-y-1/2 right-3 sm:right-4 top-1/2"
                >
                  {showNewPassword ? (
                    <EyeOff className="w-4 h-4 text-gray-400 sm:w-5 sm:h-5" />
                  ) : (
                    <Eye className="w-4 h-4 text-gray-400 sm:w-5 sm:h-5" />
                  )}
                </button>
              </div>
            </div>

            {/* Confirm Password */}
            <div className="space-y-1.5 sm:space-y-2">
              <label htmlFor="confirmPassword" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                Xác nhận mật khẩu mới
              </label>
              <div className="relative">
                <Lock className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                <input
                  id="confirmPassword"
                  type={showConfirmPassword ? 'text' : 'password'}
                  placeholder="Nhập lại mật khẩu mới"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  onKeyPress={handleKeyPress}
                  className="w-full pl-10 sm:pl-12 pr-10 sm:pr-12 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute -translate-y-1/2 right-3 sm:right-4 top-1/2"
                >
                  {showConfirmPassword ? (
                    <EyeOff className="w-4 h-4 text-gray-400 sm:w-5 sm:h-5" />
                  ) : (
                    <Eye className="w-4 h-4 text-gray-400 sm:w-5 sm:h-5" />
                  )}
                </button>
              </div>
            </div>

            {/* Password Requirements */}
            <div 
              className="p-3 text-xs border-2 sm:p-4 sm:text-sm border-violet-100 bg-violet-50/30"
              style={{ borderRadius: '12px' }}
            >
              <p className="mb-2 font-semibold text-violet-900">Yêu cầu mật khẩu:</p>
              <ul className="space-y-1 text-gray-700 list-disc list-inside">
                <li>Ít nhất 6 ký tự</li>
                <li>Khác với mật khẩu cũ</li>
                <li>Nên kết hợp chữ hoa, chữ thường và số</li>
              </ul>
            </div>

            <button
              onClick={handleChangePassword}
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
                  Đang xử lý...
                </span>
              ) : (
                'Thay đổi mật khẩu'
              )}
            </button>
          </div>

          <div 
            className="p-3 mt-5 text-center border-2 sm:mt-6 sm:p-4 border-amber-200 bg-amber-50"
            style={{ borderRadius: '14px' }}
          >
            <p className="text-xs font-semibold text-amber-900 sm:text-sm">
            Lưu ý
            </p>
            <p className="mt-1 text-xs text-amber-800">
              Sau khi thay đổi mật khẩu, bạn sẽ bị đăng xuất khỏi tất cả thiết bị và cần đăng nhập lại.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}