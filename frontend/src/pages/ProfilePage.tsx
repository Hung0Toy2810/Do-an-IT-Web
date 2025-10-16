// ==================== pages/ProfilePage.tsx ====================
import React, { useState, useEffect } from 'react';
import { ArrowLeft, User, Mail, MapPin, Camera, LogOut, Shield,Lock } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { getCookie, deleteCookie } from '../utils/cookies';
import { notify } from '../components/NotificationProvider';
import { isTokenValid } from '../utils/auth';

interface Province {
  PROVINCE_ID: number;
  PROVINCE_CODE: string;
  PROVINCE_NAME: string;
}

interface District {
  DISTRICT_ID: number;
  DISTRICT_VALUE: string;
  DISTRICT_NAME: string;
  PROVINCE_ID: number;
}

interface Ward {
  WARDS_ID: number;
  WARDS_NAME: string;
  DISTRICT_ID: number;
}

interface NewDistrict {
  DISTRICT_ID: number;
  DISTRICT_VALUE: string;
  DISTRICT_NAME: string;
}

interface CustomerData {
  customerName: string;
  email: string;
  phoneNumber: string;
  avtURL: string;
  standardShippingAddress: {
    provinceId: number;
    provinceCode: string;
    provinceName: string;
    districtId: number;
    districtValue: string;
    districtName: string;
    wardsId: number;
    wardsName: string;
    detailAddress: string;
  };
}

export default function ProfilePage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [avatarLoading, setAvatarLoading] = useState(false);
  const [customerData, setCustomerData] = useState<CustomerData | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  
  const [customerName, setCustomerName] = useState('');
  const [email, setEmail] = useState('');
  const [avatarUrl, setAvatarUrl] = useState('');
  
  const [provinces, setProvinces] = useState<Province[]>([]);
  const [validProvinces, setValidProvinces] = useState<Province[]>([]);
  const [wards, setWards] = useState<Ward[]>([]);
  const [currentNewDistrict, setCurrentNewDistrict] = useState<NewDistrict | null>(null);
  
  const [selectedProvinceId, setSelectedProvinceId] = useState<number>(0);
  const [selectedWardId, setSelectedWardId] = useState<number>(0);
  const [detailAddress, setDetailAddress] = useState('');

  useEffect(() => {
    if (!isTokenValid()) {
      notify('warning', 'Phiên đăng nhập hết hạn');
      navigate('/login');
      return;
    }
    fetchCustomerData();
    fetchProvinces();
  }, []);

  const fetchCustomerData = async () => {
    const token = getCookie('auth_token');
    if (!token) {
      navigate('/login');
      return;
    }

    try {
      const response = await fetch('http://localhost:5067/api/customers/me', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      if (response.ok) {
        const data = await response.json();
        setCustomerData(data);
        setCustomerName(data.customerName || '');
        setEmail(data.email || '');
        setAvatarUrl(data.avtURL || '');
        
        if (data.standardShippingAddress) {
          setSelectedProvinceId(data.standardShippingAddress.provinceId || 0);
          setSelectedWardId(data.standardShippingAddress.wardsId || 0);
          setDetailAddress(data.standardShippingAddress.detailAddress || '');
          
          if (data.standardShippingAddress.provinceId) {
            fetchWards(data.standardShippingAddress.provinceId);
          }
        }
      } else {
        notify('error', 'Không thể tải thông tin');
      }
    } catch (error) {
      notify('error', 'Lỗi kết nối server');
      console.error('Fetch customer error:', error);
    }
  };

  const fetchProvinces = async () => {
    try {
      const response = await fetch('https://partner.viettelpost.vn/v2/categories/listProvinceById?provinceId=-1');
      const result = await response.json();
      
      if (result.status === 200 && result.data) {
        setProvinces(result.data);
        await filterValidProvinces(result.data);
      }
    } catch (error) {
      console.error('Fetch provinces error:', error);
    }
  };

  const filterValidProvinces = async (allProvinces: Province[]) => {
    const validPromises = allProvinces.map(async (province) => {
      try {
        const districtResponse = await fetch(
          `https://partner.viettelpost.vn/v2/categories/listDistrict?provinceId=${province.PROVINCE_ID}`
        );
        const districtResult = await districtResponse.json();

        if (districtResult.status === 200 && districtResult.data) {
          const hasNewDistrict = districtResult.data.some(
            (d: District) => d.DISTRICT_NAME === 'Bỏ qua - Sử dụng địa chỉ 2 cấp'
          );
          return hasNewDistrict ? province : null;
        }
        return null;
      } catch (error) {
        console.error(`Error checking province ${province.PROVINCE_ID}:`, error);
        return null;
      }
    });

    const results = await Promise.all(validPromises);
    const valid = results.filter((province): province is Province => province !== null);
    setValidProvinces(valid);
  };

  const fetchWards = async (provinceId: number) => {
    try {
      const districtResponse = await fetch(`https://partner.viettelpost.vn/v2/categories/listDistrict?provinceId=${provinceId}`);
      const districtResult = await districtResponse.json();
      
      if (districtResult.status === 200 && districtResult.data) {
        const newDistrictItem = districtResult.data.find((d: District) => 
          d.DISTRICT_NAME === 'Bỏ qua - Sử dụng địa chỉ 2 cấp'
        );
        
        if (newDistrictItem) {
          setCurrentNewDistrict({
            DISTRICT_ID: newDistrictItem.DISTRICT_ID,
            DISTRICT_VALUE: newDistrictItem.DISTRICT_VALUE,
            DISTRICT_NAME: newDistrictItem.DISTRICT_NAME,
          });
          
          const wardsResponse = await fetch(`https://partner.viettelpost.vn/v2/categories/listWards?districtId=${newDistrictItem.DISTRICT_ID}`);
          const wardsResult = await wardsResponse.json();
          
          if (wardsResult.status === 200 && wardsResult.data) {
            setWards(wardsResult.data);
          } else {
            setWards([]);
          }
        } else {
          setCurrentNewDistrict(null);
          setWards([]);
        }
      }
    } catch (error) {
      console.error('Fetch wards error:', error);
      setWards([]);
    }
  };

  const handleProvinceChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const provinceId = parseInt(e.target.value);
    setSelectedProvinceId(provinceId);
    setSelectedWardId(0);
    setWards([]);
    setCurrentNewDistrict(null);
    
    if (provinceId) {
      fetchWards(provinceId);
    }
  };

  const handleAvatarUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      notify('warning', 'Vui lòng chọn file ảnh');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      notify('warning', 'Kích thước ảnh không được vượt quá 5MB');
      return;
    }

    const token = getCookie('auth_token');
    if (!token) {
      notify('warning', 'Vui lòng đăng nhập lại');
      navigate('/login');
      return;
    }

    setAvatarLoading(true);

    try {
      const formData = new FormData();
      formData.append('file', file);

      const response = await fetch('http://localhost:5067/api/customers/avatar', {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
        body: formData,
      });

      const data = await response.json();

      if (response.ok && data.avatarUrl) {
        setAvatarUrl(data.avatarUrl);
        notify('success', 'Cập nhật ảnh đại diện thành công');
      } else {
        notify('error', data.message || 'Cập nhật ảnh thất bại');
      }
    } catch (error) {
      notify('error', 'Không thể kết nối đến server');
      console.error('Upload avatar error:', error);
    } finally {
      setAvatarLoading(false);
    }
  };

  const handleUpdateProfile = async () => {
    if (!customerName) {
      notify('warning', 'Vui lòng nhập tên');
      return;
    }

    if (!email) {
      notify('warning', 'Vui lòng nhập email');
      return;
    }

    if (!selectedProvinceId || !selectedWardId) {
      notify('warning', 'Vui lòng chọn đầy đủ địa chỉ');
      return;
    }

    if (!detailAddress) {
      notify('warning', 'Vui lòng nhập địa chỉ chi tiết');
      return;
    }

    if (!currentNewDistrict) {
      notify('error', 'Không tìm thấy thông tin district');
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
      const selectedProvince = provinces.find(p => p.PROVINCE_ID === selectedProvinceId);
      const selectedWard = wards.find(w => w.WARDS_ID === selectedWardId);

      const response = await fetch('http://localhost:5067/api/customers', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({
          customerName,
          email,
          standardShippingAddress: {
            provinceId: selectedProvinceId,
            provinceCode: selectedProvince?.PROVINCE_CODE || '',
            provinceName: selectedProvince?.PROVINCE_NAME || '',
            districtId: currentNewDistrict.DISTRICT_ID,
            districtValue: currentNewDistrict.DISTRICT_VALUE,
            districtName: currentNewDistrict.DISTRICT_NAME,
            wardsId: selectedWardId,
            wardsName: selectedWard?.WARDS_NAME || '',
            detailAddress,
          },
        }),
      });

      const data = await response.json();

      if (response.ok) {
        notify('success', 'Cập nhật thông tin thành công');
        setIsEditing(false);
        await fetchCustomerData();
      } else {
        notify('error', data.message || 'Cập nhật thất bại');
      }
    } catch (error) {
      notify('error', 'Không thể kết nối đến server');
      console.error('Update profile error:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleStartEdit = () => {
    setIsEditing(true);
  };

  const handleCancelEdit = () => {
    if (customerData) {
      setCustomerName(customerData.customerName || '');
      setEmail(customerData.email || '');
      
      if (customerData.standardShippingAddress) {
        setSelectedProvinceId(customerData.standardShippingAddress.provinceId || 0);
        setSelectedWardId(customerData.standardShippingAddress.wardsId || 0);
        setDetailAddress(customerData.standardShippingAddress.detailAddress || '');
        
        if (customerData.standardShippingAddress.provinceId) {
          fetchWards(customerData.standardShippingAddress.provinceId);
        }
      }
    }
    setIsEditing(false);
  };

  const handleLogout = async () => {
    const token = getCookie('auth_token');
    if (!token) {
      navigate('/login');
      return;
    }

    try {
      await fetch('http://localhost:5067/api/customers/logout', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      deleteCookie('auth_token');
      notify('success', 'Đăng xuất thành công');
      navigate('/login');
    } catch (error) {
      deleteCookie('auth_token');
      navigate('/login');
      console.error('Logout error:', error);
    }
  };

  const handleLogoutOtherDevices = async () => {
    const token = getCookie('auth_token');
    if (!token) {
      notify('warning', 'Vui lòng đăng nhập lại');
      navigate('/login');
      return;
    }

    try {
      const response = await fetch('http://localhost:5067/api/customers/logout/other-devices', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });

      if (response.ok) {
        notify('success', 'Đăng xuất tất cả thiết bị khác thành công');
      } else {
        notify('error', 'Không thể đăng xuất thiết bị khác');
      }
    } catch (error) {
      notify('error', 'Lỗi kết nối server');
      console.error('Logout other devices error:', error);
    }
  };

  if (!customerData) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="w-8 h-8 border-4 rounded-full border-violet-800 border-t-transparent animate-spin"></div>
      </div>
    );
  }

  const provinceOptions = isEditing ? validProvinces : provinces;

  return (
    <div className="min-h-screen px-4 py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-12">
      <style>{`
        select {
          appearance: none;
          -webkit-appearance: none;
          -moz-appearance: none;
        }
        select::-ms-expand {
          display: none; /* For IE/Edge */
        }
      `}</style>
      <div className="container max-w-4xl mx-auto">
        <button
          onClick={() => navigate('/')}
          className="flex items-center gap-2 px-4 py-2.5 mb-8 text-sm font-medium text-violet-800 transition-all bg-white hover:bg-violet-50 shadow-sm hover:shadow-md"
          style={{ borderRadius: '12px' }}
        >
          <ArrowLeft className="w-4 h-4" />
          Quay lại
        </button>

        <div className="grid gap-6 md:gap-8">
          <div className="p-6 bg-white shadow-xl sm:p-8" style={{ borderRadius: '20px' }}>
            <div className="flex flex-col items-center gap-6 mb-8 sm:flex-row">
              <div className="relative">
                <div 
                  className="relative w-24 h-24 overflow-hidden bg-gray-200 sm:w-32 sm:h-32"
                  style={{ borderRadius: '16px' }}
                >
                  {avatarUrl ? (
                    <img src={avatarUrl} alt="Avatar" className="object-cover w-full h-full" />
                  ) : (
                    <div className="flex items-center justify-center w-full h-full bg-gradient-to-br from-violet-600 to-violet-800">
                      <User className="w-12 h-12 text-white sm:w-16 sm:h-16" />
                    </div>
                  )}
                  {avatarLoading && (
                    <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-50">
                      <div className="w-6 h-6 border-2 border-white rounded-full border-t-transparent animate-spin"></div>
                    </div>
                  )}
                </div>
                <label 
                  htmlFor="avatar-upload"
                  className="absolute bottom-0 right-0 flex items-center justify-center w-8 h-8 text-white transition-all shadow-lg cursor-pointer bg-violet-800 hover:bg-violet-900 sm:w-10 sm:h-10"
                  style={{ borderRadius: '10px' }}
                >
                  <Camera className="w-4 h-4 sm:w-5 sm:h-5" />
                  <input
                    id="avatar-upload"
                    type="file"
                    accept="image/*"
                    className="hidden"
                    onChange={handleAvatarUpload}
                    disabled={avatarLoading}
                  />
                </label>
              </div>
              <div className="flex-1 text-center sm:text-left">
                <h1 className="mb-2 text-2xl font-bold sm:text-3xl text-violet-900">
                  {customerData.customerName || 'Chưa cập nhật'}
                </h1>
                <p className="text-sm font-medium text-gray-600 sm:text-base">
                  {customerData.phoneNumber}
                </p>
              </div>
            </div>

            <div className="space-y-4 sm:space-y-5">
              <div className="space-y-2">
                <label className="block text-sm font-semibold text-gray-700">
                  Tên khách hàng
                </label>
                <div className="relative">
                  <User className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 pointer-events-none left-4 top-1/2" />
                  <input
                    type="text"
                    placeholder="Nhập tên của bạn"
                    value={customerName}
                    onChange={(e) => setCustomerName(e.target.value)}
                    disabled={!isEditing}
                    className="w-full pl-12 pr-4 py-3.5 text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                    style={{ borderRadius: '14px' }}
                  />
                </div>
              </div>

              <div className="space-y-2">
                <label className="block text-sm font-semibold text-gray-700">
                  Email
                </label>
                <div className="relative">
                  <Mail className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 pointer-events-none left-4 top-1/2" />
                  <input
                    type="email"
                    placeholder="Nhập email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    disabled={!isEditing}
                    className="w-full pl-12 pr-4 py-3.5 text-sm font-medium bg-gray-50 border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                    style={{ borderRadius: '14px' }}
                  />
                </div>
              </div>

              <div className="p-4 border-2 border-violet-100 bg-violet-50/30" style={{ borderRadius: '14px' }}>
                <h3 className="mb-4 text-sm font-bold text-violet-900">Địa chỉ giao hàng mặc định</h3>
                
                <div className="space-y-4">
                  <div className="space-y-2">
                    <label className="block text-sm font-semibold text-gray-700">
                      Tỉnh/Thành phố
                    </label>
                    <div className="relative">
                      <MapPin className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 pointer-events-none left-4 top-1/2" />
                      <select
                        value={selectedProvinceId}
                        onChange={handleProvinceChange}
                        disabled={!isEditing || provinceOptions.length === 0}
                        className="w-full pl-12 pr-4 py-3.5 text-sm font-medium bg-white border border-gray-200 text-gray-900 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                        style={{ borderRadius: '14px' }}
                      >
                        <option value="0">Chọn Tỉnh/Thành phố</option>
                        {provinceOptions.map((province) => (
                          <option key={province.PROVINCE_ID} value={province.PROVINCE_ID}>
                            {province.PROVINCE_NAME}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <label className="block text-sm font-semibold text-gray-700">
                      Xã/Phường
                    </label>
                    <div className="relative">
                      <MapPin className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 pointer-events-none left-4 top-1/2" />
                      <select
                        value={selectedWardId}
                        onChange={(e) => setSelectedWardId(parseInt(e.target.value))}
                        disabled={!isEditing || !selectedProvinceId || wards.length === 0}
                        className="w-full pl-12 pr-4 py-3.5 text-sm font-medium bg-white border border-gray-200 text-gray-900 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                        style={{ borderRadius: '14px' }}
                      >
                        <option value="0">Chọn Xã/Phường</option>
                        {wards.map((ward) => (
                          <option key={ward.WARDS_ID} value={ward.WARDS_ID}>
                            {ward.WARDS_NAME}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <label className="block text-sm font-semibold text-gray-700">
                      Địa chỉ chi tiết
                    </label>
                    <textarea
                      placeholder="Nhập số nhà, tên đường..."
                      value={detailAddress}
                      onChange={(e) => setDetailAddress(e.target.value)}
                      rows={3}
                      disabled={!isEditing}
                      className="w-full px-4 py-3.5 text-sm font-medium bg-white border border-gray-200 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 transition-all resize-none disabled:opacity-50 disabled:cursor-not-allowed"
                      style={{ borderRadius: '14px' }}
                    />
                  </div>
                </div>
              </div>

              {!isEditing ? (
                <button
                  onClick={handleStartEdit}
                  className="w-full py-3.5 text-sm font-semibold text-white bg-gradient-to-r from-violet-700 to-violet-800 hover:from-violet-800 hover:to-violet-900 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all"
                  style={{ borderRadius: '14px' }}
                >
                  Cập nhật thông tin
                </button>
              ) : (
                <div className="flex gap-4">
                  <button
                    onClick={handleUpdateProfile}
                    disabled={loading}
                    className="flex-1 py-3.5 text-sm font-semibold text-white bg-gradient-to-r from-violet-700 to-violet-800 hover:from-violet-800 hover:to-violet-900 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-60 disabled:cursor-not-allowed disabled:hover:scale-100"
                    style={{ borderRadius: '14px' }}
                  >
                    {loading ? (
                      <span className="flex items-center justify-center gap-2">
                        <svg className="w-5 h-5 text-white animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        Đang cập nhật...
                      </span>
                    ) : (
                      'Cập nhật'
                    )}
                  </button>
                  <button
                    onClick={handleCancelEdit}
                    disabled={loading}
                    className="flex-1 py-3.5 text-sm font-semibold text-violet-800 bg-white border-2 border-violet-200 hover:bg-violet-50 shadow-sm hover:shadow-md transition-all disabled:opacity-60 disabled:cursor-not-allowed"
                    style={{ borderRadius: '14px' }}
                  >
                    Hủy
                  </button>
                </div>
              )}
            </div>
          </div>

          <div className="p-6 bg-white shadow-xl sm:p-8" style={{ borderRadius: '20px' }}>
            <h2 className="mb-6 text-xl font-bold text-violet-900">Bảo mật</h2>
            <div className="space-y-3">
              <button
                onClick={() => navigate('/change-password')}
                className="flex items-center justify-center w-full gap-2 py-3.5 text-sm font-semibold text-violet-800 bg-white border-2 border-violet-200 hover:bg-violet-50 shadow-sm hover:shadow-md transition-all"
                style={{ borderRadius: '14px' }}
              >
                <Lock className="w-5 h-5" />
                Thay đổi mật khẩu
              </button>
              <button
                onClick={handleLogoutOtherDevices}
                className="flex items-center justify-center w-full gap-2 py-3.5 text-sm font-semibold text-violet-800 bg-white border-2 border-violet-200 hover:bg-violet-50 shadow-sm hover:shadow-md transition-all"
                style={{ borderRadius: '14px' }}
              >
                <Shield className="w-5 h-5" />
                Đăng xuất thiết bị khác
              </button>
              <button
                onClick={handleLogout}
                className="flex items-center justify-center w-full gap-2 py-3.5 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 shadow-lg hover:shadow-xl transition-all"
                style={{ borderRadius: '14px' }}
              >
                <LogOut className="w-5 h-5" />
                Đăng xuất
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
