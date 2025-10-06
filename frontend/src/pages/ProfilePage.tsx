import { useState } from 'react';
import { ArrowLeft, User, Phone, MapPin, Trash2, Save } from 'lucide-react';

interface ProfilePageProps {
  onNavigate: (page: 'home' | 'login' | 'register') => void;
}

export default function ProfilePage({ onNavigate }: ProfilePageProps) {
  const [formData, setFormData] = useState({
    displayName: 'Nguyễn Văn A',
    phone: '0123456789',
    address: '123 Đường ABC, Quận 1, TP.HCM'
  });
  const [loading, setLoading] = useState(false);

  const handleChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSave = () => {
    if (!formData.displayName || !formData.phone) {
      alert('Vui lòng nhập đầy đủ thông tin bắt buộc');
      return;
    }

    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      alert('Cập nhật thông tin thành công!');
    }, 1500);
  };

  const handleDeleteAccount = () => {
    const confirmed = confirm('Bạn có chắc chắn muốn xóa tài khoản? Hành động này không thể hoàn tác.');
    if (confirmed) {
      setLoading(true);
      setTimeout(() => {
        setLoading(false);
        alert('Tài khoản đã được xóa');
        onNavigate('home');
      }, 1500);
    }
  };

  return (
    <div className="min-h-screen px-4 py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-12 sm:px-6 lg:px-8">
      <div className="container max-w-2xl mx-auto">
        {/* Back Button */}
        <button
          onClick={() => onNavigate('home')}
          className="flex items-center gap-1.5 sm:gap-2 px-3 sm:px-4 py-2 sm:py-2.5 mb-6 sm:mb-8 text-xs sm:text-sm font-medium text-violet-800 transition-all bg-white hover:bg-violet-50 shadow-sm hover:shadow-md"
          style={{ borderRadius: '12px' }}
        >
          <ArrowLeft className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
          Quay lại
        </button>

        {/* Profile Card */}
        <div 
          className="p-6 bg-white border shadow-2xl border-violet-100/50 sm:p-8 md:p-10"
          style={{ borderRadius: '20px' }}
        >
          {/* Header */}
          <div className="mb-6 text-center sm:mb-8">
            <div 
              className="inline-flex items-center justify-center w-16 h-16 mb-3 shadow-lg sm:w-20 sm:h-20 bg-gradient-to-br from-violet-600 to-violet-800 sm:mb-4"
              style={{ borderRadius: '16px' }}
            >
              <User className="w-8 h-8 text-white sm:w-10 sm:h-10" />
            </div>
            <h1 className="text-2xl sm:text-3xl font-bold mb-1.5 sm:mb-2 bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text text-transparent">
              Thông tin cá nhân
            </h1>
            <p className="text-sm font-medium text-gray-600 sm:text-base">Quản lý thông tin tài khoản của bạn</p>
          </div>

          <div className="space-y-5 sm:space-y-6">
            {/* Display Name Input */}
            <div className="space-y-1.5 sm:space-y-2">
              <label htmlFor="displayName" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                Tên hiển thị <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <User className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                <input
                  id="displayName"
                  type="text"
                  placeholder="Nhập tên hiển thị"
                  value={formData.displayName}
                  onChange={(e) => handleChange('displayName', e.target.value)}
                  className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
              </div>
            </div>

            {/* Phone Input */}
            <div className="space-y-1.5 sm:space-y-2">
              <label htmlFor="phone" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                Số điện thoại <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <Phone className="absolute w-4 h-4 text-gray-400 -translate-y-1/2 pointer-events-none left-3 sm:left-4 top-1/2 sm:w-5 sm:h-5" />
                <input
                  id="phone"
                  type="tel"
                  placeholder="Nhập số điện thoại"
                  value={formData.phone}
                  onChange={(e) => handleChange('phone', e.target.value)}
                  className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all"
                  style={{ borderRadius: '14px' }}
                />
              </div>
            </div>

            {/* Address Input */}
            <div className="space-y-1.5 sm:space-y-2">
              <label htmlFor="address" className="block text-xs font-semibold text-gray-700 sm:text-sm">
                Địa chỉ giao hàng
              </label>
              <div className="relative">
                <MapPin className="absolute w-4 h-4 text-gray-400 pointer-events-none left-3 sm:left-4 top-4 sm:top-4 sm:w-5 sm:h-5" />
                <textarea
                  id="address"
                  placeholder="Nhập địa chỉ giao hàng"
                  value={formData.address}
                  onChange={(e) => handleChange('address', e.target.value)}
                  rows={3}
                  className="w-full pl-10 sm:pl-12 pr-3 sm:pr-4 py-3 sm:py-3.5 text-xs sm:text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all resize-none"
                  style={{ borderRadius: '14px' }}
                />
              </div>
            </div>

            {/* Save Button */}
            <button
              onClick={handleSave}
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
                  Đang lưu...
                </span>
              ) : (
                <span className="flex items-center justify-center gap-2">
                  <Save className="w-4 h-4 sm:w-5 sm:h-5" />
                  Lưu thay đổi
                </span>
              )}
            </button>

            {/* Delete Account Button */}
            <button
              onClick={handleDeleteAccount}
              disabled={loading}
              className="w-full py-2.5 sm:py-3 text-xs sm:text-sm font-semibold text-red-600 bg-white border-2 border-red-200 hover:border-red-400 hover:bg-red-50 shadow-sm hover:shadow-md hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-60 disabled:cursor-not-allowed disabled:hover:scale-100"
              style={{ borderRadius: '14px' }}
            >
              <span className="flex items-center justify-center gap-2">
                <Trash2 className="w-4 h-4 sm:w-4.5 sm:h-4.5" />
                Xóa tài khoản
              </span>
            </button>
          </div>

          {/* Account Info */}
          <div 
            className="pt-5 mt-6 border-t sm:mt-8 sm:pt-6 border-violet-100"
          >
            <div className="flex items-center justify-between text-xs sm:text-sm">
              <span className="font-medium text-gray-600">Email:</span>
              <span className="font-semibold text-gray-900">user@example.com</span>
            </div>
            <div className="flex items-center justify-between mt-2 text-xs sm:text-sm sm:mt-3">
              <span className="font-medium text-gray-600">Tên đăng nhập:</span>
              <span className="font-semibold text-gray-900">username123</span>
            </div>
            <div className="mt-3 sm:mt-4">
              <button
                onClick={() => onNavigate('login')}
                className="text-xs font-semibold transition-colors sm:text-sm text-violet-700 hover:text-violet-900"
              >
                Đăng xuất
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}