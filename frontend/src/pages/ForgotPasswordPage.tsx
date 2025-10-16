// ==================== pages/ForgotPasswordPage.tsx ====================
import React, { useState } from 'react';
import { ArrowLeft, User, Lock, Shield, KeyRound } from 'lucide-react';
import { notify } from '../components/NotificationProvider';
import { useNavigate } from 'react-router-dom';

export default function ForgotPasswordPage() {
  const [step, setStep] = useState<1 | 2>(1);
  const [phoneNumber, setPhoneNumber] = useState('');
  const [otp, setOtp] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSendOtp = async () => {
    if (!phoneNumber) {
      notify('warning', 'Vui lòng nhập số điện thoại');
      return;
    }

    if (phoneNumber.length < 10) {
      notify('warning', 'Số điện thoại không hợp lệ');
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:5067/api/customers/password/reset/otp', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Phonenumber: phoneNumber,
        }),
      });

      const data = await response.json();

      if (response.ok) {
        notify('success', 'Mã OTP đã được gửi đến số điện thoại của bạn');
        setStep(2);
      } else {
        notify('error', data.message || 'Không thể gửi OTP');
      }
    } catch (error) {
      notify('error', 'Không thể kết nối đến server');
      console.error('Send OTP error:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleResetPassword = async () => {
    if (!otp || !newPassword || !confirmPassword) {
      notify('warning', 'Vui lòng nhập đầy đủ thông tin');
      return;
    }

    if (newPassword !== confirmPassword) {
      notify('error', 'Mật khẩu xác nhận không khớp');
      return;
    }

    if (newPassword.length < 6) {
      notify('warning', 'Mật khẩu phải có ít nhất 6 ký tự');
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:5067/api/customers/password/reset', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          Phonenumber: phoneNumber,
          otp: otp,
          NewPassword: newPassword,
        }),
      });

      const data = await response.json();

      if (response.ok) {
        notify('success', 'Đặt lại mật khẩu thành công!');
        setTimeout(() => {
          navigate('/login');
        }, 1000);
      } else {
        notify('error', data.message || 'Đặt lại mật khẩu thất bại');
      }
    } catch (error) {
      notify('error', 'Không thể kết nối đến server');
      console.error('Reset password error:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      if (step === 1) {
        handleSendOtp();
      } else {
        handleResetPassword();
      }
    }
  };

  return (
    <div className="min-h-screen px-4 py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-12 sm:px-6 lg:px-8">
      <div className="container max-w-md mx-auto">
        <button
          onClick={() => navigate('/login')}
          className="flex items-center gap-1.5 sm:gap-2 px-3 sm:px-4 py-2 sm:py-2.5 mb-6 sm:mb-8 text-xs sm:text-sm font-medium text-violet-800 transition-all bg-white hover:bg-violet-50 shadow-sm hover:shadow-md"
          style={{ borderRadius: '12px' }}
        >
          <ArrowLeft className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
          Quay lại đăng nhập
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
              <KeyRound className="w-6 h-6 text-white sm:w-8 sm:h-8" />
            </div>
            <h1 className="text-2xl sm:text-3xl font-bold mb-1.5 sm:mb-2 bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text text-transparent">
              Khôi phục mật khẩu
            </h1>
            <p className="text-sm font-medium text-gray-600 sm:text-base">
              {step === 1 ? 'Nhập số điện thoại để nhận mã OTP' : 'Nhập mã OTP và mật khẩu mới'}
            </p>
          </div>

          <div className="flex items-center justify-center mb-6 sm:mb-8">
            <div className="flex items-center gap-2">
              <div className={`w-8 h-8 flex items-center justify-center rounded-full text-xs font-bold ${
                step >= 1 ? 'bg-violet-800 text-white' : 'bg-gray-200 text-gray-500'
              }`}>
                1
              </div>
              <div className={`w-12 h-1 ${step >= 2 ? 'bg-violet-800' : 'bg-gray-200'}`}></div>
              <div className={`w-8 h-8 flex items-center justify-center rounded-full text-xs font-bold ${
                step >= 2 ? 'bg-violet-800 text-white' : 'bg-gray-200 text-gray-500'
              }`}>
                2
              </div>
            </div>
          </div>

          {step === 1 ? (
            <div className="space-y-4 sm:space-y-5">
              <div className="space-y-1.5 sm:space-y-2">
                <label htmlFor="phoneNumber" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                  Số điện thoại
                </label>
                <div className="relative">
                  <User className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                  <input
                    id="phoneNumber"
                    type="text"
                    placeholder="Nhập số điện thoại"
                    value={phoneNumber}
                    onChange={(e) => setPhoneNumber(e.target.value)}
                    onKeyPress={handleKeyPress}
                    className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                    style={{ borderRadius: '14px' }}
                  />
                </div>
              </div>

              <button
                onClick={handleSendOtp}
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
                    Đang gửi OTP...
                  </span>
                ) : (
                  'Nhận mã OTP'
                )}
              </button>
            </div>
          ) : (
            <div className="space-y-4 sm:space-y-5">
              <div className="space-y-1.5 sm:space-y-2">
                <label htmlFor="otp" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                  Mã OTP
                </label>
                <div className="relative">
                  <Shield className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                  <input
                    id="otp"
                    type="text"
                    placeholder="Nhập mã OTP"
                    value={otp}
                    onChange={(e) => setOtp(e.target.value)}
                    onKeyPress={handleKeyPress}
                    className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                    style={{ borderRadius: '14px' }}
                  />
                </div>
              </div>

              <div className="space-y-1.5 sm:space-y-2">
                <label htmlFor="newPassword" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                  Mật khẩu mới
                </label>
                <div className="relative">
                  <Lock className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                  <input
                    id="newPassword"
                    type="password"
                    placeholder="Nhập mật khẩu mới"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    onKeyPress={handleKeyPress}
                    className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                    style={{ borderRadius: '14px' }}
                  />
                </div>
              </div>

              <div className="space-y-1.5 sm:space-y-2">
                <label htmlFor="confirmPassword" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                  Xác nhận mật khẩu
                </label>
                <div className="relative">
                  <Lock className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                  <input
                    id="confirmPassword"
                    type="password"
                    placeholder="Nhập lại mật khẩu mới"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    onKeyPress={handleKeyPress}
                    className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                    style={{ borderRadius: '14px' }}
                  />
                </div>
              </div>

              <div className="flex gap-2">
                <button
                  onClick={() => setStep(1)}
                  className="flex-1 py-3 sm:py-3.5 text-xs sm:text-sm font-semibold text-violet-800 bg-white border-2 border-violet-200 hover:bg-violet-50 shadow-sm hover:shadow-md transition-all"
                  style={{ borderRadius: '14px' }}
                >
                  Quay lại
                </button>
                <button
                  onClick={handleResetPassword}
                  disabled={loading}
                  className="flex-1 py-3 sm:py-3.5 text-xs sm:text-sm font-semibold text-white bg-gradient-to-r from-violet-700 to-violet-800 hover:from-violet-800 hover:to-violet-900 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-60 disabled:cursor-not-allowed disabled:hover:scale-100"
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
                    'Đặt lại mật khẩu'
                  )}
                </button>
              </div>
            </div>
          )}

          <div className="pt-5 mt-6 text-center border-t sm:mt-8 sm:pt-6 border-violet-100">
            <p className="text-xs font-medium text-gray-600 sm:text-sm">
              Nhớ mật khẩu?{' '}
              <button
                onClick={() => navigate('/login')}
                className="font-semibold transition-colors text-violet-700 hover:text-violet-900"
              >
                Đăng nhập ngay
              </button>
            </p>
          </div>

          <div 
            className="p-3 mt-5 text-center border sm:mt-6 sm:p-4 bg-violet-50 border-violet-100"
            style={{ borderRadius: '14px' }}
          >
            <p className="text-xs font-semibold text-violet-900 mb-0.5 sm:mb-1">Bảo mật</p>
            <p className="text-[10px] sm:text-xs text-violet-700">Mã OTP có hiệu lực trong 5 phút</p>
          </div>
        </div>
      </div>
    </div>
  );
}