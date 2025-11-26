// src/pages/ShoppingCart.tsx
import { useState, useEffect } from 'react';
import { Trash2, Plus, Minus, ShoppingBag, CreditCard, MapPin, Check, AlertCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { getCookie } from '../utils/cookies';
import { notify } from '../components/NotificationProvider';
import { isTokenValid } from '../utils/auth';

// ============= TYPES =============
interface CartItem {
  cartId: number;
  productId: number;
  productName: string;
  productSlug: string;
  firstImage: string | null;
  attributes: { [key: string]: string };
  originalPrice: number;
  discountedPrice: number;
  quantity: number;
  availableStock: number;
  lineTotal: number;
}

interface Province {
  PROVINCE_ID: number;
  PROVINCE_CODE: string;
  PROVINCE_NAME: string;
}

interface District {
  DISTRICT_ID: number;
  DISTRICT_VALUE: string;
  DISTRICT_NAME: string;
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
  phoneNumber: string;
  standardShippingAddress?: {
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

// ============= REAL API =============
const API_BASE = 'http://localhost:5067';
const VIETTEL_POST_BASE = 'https://partner.viettelpost.vn/v2/categories';

const fetchCartItems = async (): Promise<CartItem[]> => {
  const token = getCookie('auth_token');
  if (!token || !isTokenValid()) return [];

  try {
    const response = await fetch(`${API_BASE}/api/cart`, {
      headers: { 'Authorization': `Bearer ${token}` },
    });

    if (!response.ok) throw new Error();
    const result = await response.json();
    return result.data?.items || [];
  } catch {
    notify('error', 'Lỗi tải giỏ hàng');
    return [];
  }
};

const fetchCustomerData = async (): Promise<CustomerData | null> => {
  const token = getCookie('auth_token');
  if (!token || !isTokenValid()) return null;

  try {
    const response = await fetch(`${API_BASE}/api/customers/me`, {
      headers: { 'Authorization': `Bearer ${token}` },
    });

    if (!response.ok) return null;
    const data = await response.json();
    return data.data || data;
  } catch {
    return null;
  }
};

const fetchProvinces = async (): Promise<Province[]> => {
  try {
    const response = await fetch(`${VIETTEL_POST_BASE}/listProvinceById?provinceId=-1`);
    const result = await response.json();
    return result.status === 200 ? result.data : [];
  } catch {
    notify('error', 'Không thể tải tỉnh/thành');
    return [];
  }
};

const fetchDistricts = async (provinceId: number): Promise<District[]> => {
  try {
    const response = await fetch(`${VIETTEL_POST_BASE}/listDistrict?provinceId=${provinceId}`);
    const result = await response.json();
    return result.status === 200 ? result.data : [];
  } catch {
    notify('error', 'Không thể tải quận/huyện');
    return [];
  }
};

const fetchWards = async (districtId: number): Promise<Ward[]> => {
  try {
    const response = await fetch(`${VIETTEL_POST_BASE}/listWards?districtId=${districtId}`);
    const result = await response.json();
    return result.status === 200 ? result.data : [];
  } catch {
    notify('error', 'Không thể tải xã/phường');
    return [];
  }
};

const checkout = async (payload: any): Promise<{
  success: boolean;
  invoiceId?: number;
  paymentUrl?: string;
  requiresPayment?: boolean;
}> => {
  const token = getCookie('auth_token');
  if (!token || !isTokenValid()) {
    notify('warning', 'Phiên đăng nhập hết hạn');
    return { success: false };
  }

  try {
    const response = await fetch(`${API_BASE}/api/checkout`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify(payload),
    });

    const result = await response.json();

    if (response.ok) {
      const data = result.data;
      return {
        success: true,
        invoiceId: data.invoiceId,
        paymentUrl: data.paymentUrl,
        requiresPayment: data.requiresPayment,
      };
    } else {
      notify('error', result.message || 'Đặt hàng thất bại');
      return { success: false };
    }
  } catch (error) {
    notify('error', 'Không thể kết nối đến server');
    return { success: false };
  }
};

const clearCart = async (): Promise<boolean> => {
  const token = getCookie('auth_token');
  if (!token || !isTokenValid()) return false;

  try {
    const response = await fetch(`${API_BASE}/api/cart`, {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` },
    });
    return response.ok;
  } catch {
    return false;
  }
};

function removeDiacritics(str: string) {
  return str.normalize('NFD').replace(/[\u0300-\u036f]/g, '');
}

// ============= SHOPPING CART COMPONENT =============
export default function ShoppingCart() {
  const navigate = useNavigate();
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [customer, setCustomer] = useState<CustomerData | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  // Address states
  const [useDefaultAddress, setUseDefaultAddress] = useState(true);
  const [provinces, setProvinces] = useState<Province[]>([]);
  const [wards, setWards] = useState<Ward[]>([]);
  const [currentNewDistrict, setCurrentNewDistrict] = useState<NewDistrict | null>(null);

  const [selectedProvinceId, setSelectedProvinceId] = useState<number>(0);
  const [selectedWardId, setSelectedWardId] = useState<number>(0);
  const [detailAddress, setDetailAddress] = useState('');
  const [receiverName, setReceiverName] = useState('');
  const [receiverPhone, setReceiverPhone] = useState('');

  const [provinceSearch, setProvinceSearch] = useState('');
  const [wardSearch, setWardSearch] = useState('');
  const [showProvinceDropdown, setShowProvinceDropdown] = useState(false);
  const [showWardDropdown, setShowWardDropdown] = useState(false);

  const [selectedPayment, setSelectedPayment] = useState<'COD' | 'VNPAY'>('COD');

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      const [items, cust, provs] = await Promise.all([
        fetchCartItems(),
        fetchCustomerData(),
        fetchProvinces(),
      ]);
      setCartItems(items);
      setCustomer(cust);
      setProvinces(provs);

      if (cust) {
        setReceiverName(cust.customerName);
        setReceiverPhone(cust.phoneNumber);
        if (cust.standardShippingAddress) {
          const addr = cust.standardShippingAddress;
          setSelectedProvinceId(addr.provinceId);
          setSelectedWardId(addr.wardsId);
          setDetailAddress(addr.detailAddress);
          setProvinceSearch(addr.provinceName);
          setWardSearch(addr.wardsName);

          const districts = await fetchDistricts(addr.provinceId);
          const newDist = districts.find(d => d.DISTRICT_NAME === 'Bỏ qua - Sử dụng địa chỉ 2 cấp');
          if (newDist) {
            setCurrentNewDistrict({
              DISTRICT_ID: newDist.DISTRICT_ID,
              DISTRICT_VALUE: newDist.DISTRICT_VALUE,
              DISTRICT_NAME: newDist.DISTRICT_NAME,
            });
            const w = await fetchWards(newDist.DISTRICT_ID);
            setWards(w);
          }
        }
      }

      setLoading(false);
    };
    loadData();
  }, []);

  const handleProvinceSelect = async (province: Province) => {
    setSelectedProvinceId(province.PROVINCE_ID);
    setProvinceSearch(province.PROVINCE_NAME);
    setShowProvinceDropdown(false);
    setSelectedWardId(0);
    setWardSearch('');
    setWards([]);
    setCurrentNewDistrict(null);

    const districts = await fetchDistricts(province.PROVINCE_ID);
    const newDist = districts.find(d => d.DISTRICT_NAME === 'Bỏ qua - Sử dụng địa chỉ 2 cấp');
    if (newDist) {
      setCurrentNewDistrict({
        DISTRICT_ID: newDist.DISTRICT_ID,
        DISTRICT_VALUE: newDist.DISTRICT_VALUE,
        DISTRICT_NAME: newDist.DISTRICT_NAME,
      });
      const w = await fetchWards(newDist.DISTRICT_ID);
      setWards(w);
    }
  };

  const handleWardSelect = (ward: Ward) => {
    setSelectedWardId(ward.WARDS_ID);
    setWardSearch(ward.WARDS_NAME);
    setShowWardDropdown(false);
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
  };

  const calculateTotal = () => {
    return cartItems.reduce((sum, item) => sum + item.discountedPrice * item.quantity, 0);
  };

  const handleCheckout = async () => {
    if (cartItems.length === 0) {
      notify('warning', 'Giỏ hàng trống');
      return;
    }

    if (!receiverName || !receiverPhone) {
      notify('warning', 'Vui lòng nhập tên và số điện thoại');
      return;
    }

    if (!useDefaultAddress && (!selectedProvinceId || !selectedWardId || !detailAddress || !currentNewDistrict)) {
      notify('warning', 'Vui lòng chọn đầy đủ địa chỉ');
      return;
    }

    setSubmitting(true);

    const province = provinces.find(p => p.PROVINCE_ID === selectedProvinceId);
    const ward = wards.find(w => w.WARDS_ID === selectedWardId);

    const payload = {
      paymentMethod: selectedPayment,
      receiverName,
      receiverPhone,
      address: {
        provinceId: selectedProvinceId,
        provinceCode: province?.PROVINCE_CODE || '',
        provinceName: province?.PROVINCE_NAME || '',
        districtId: currentNewDistrict?.DISTRICT_ID || 0,
        districtValue: currentNewDistrict?.DISTRICT_VALUE || '',
        districtName: currentNewDistrict?.DISTRICT_NAME || '',
        wardsId: selectedWardId,
        wardsName: ward?.WARDS_NAME || '',
        detailAddress,
      },
    };

    const result = await checkout(payload);
    setSubmitting(false);

    if (result.success && result.invoiceId) {
      await clearCart();
      setCartItems([]);

      if (result.requiresPayment && result.paymentUrl) {
        // VNPAY: Chuyển hướng đến cổng thanh toán
        window.location.href = result.paymentUrl;
      } else {
        // COD: Chuyển hướng đến trang cảm ơn
        navigate('/order-success', { replace: true });
      }
    }
  };

  const filteredProvinces = provinces.filter(p =>
    removeDiacritics(p.PROVINCE_NAME).toLowerCase().includes(removeDiacritics(provinceSearch).toLowerCase())
  );

  const filteredWards = wards.filter(w =>
    removeDiacritics(w.WARDS_NAME).toLowerCase().includes(removeDiacritics(wardSearch).toLowerCase())
  );

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
        <div className="text-center">
          <div className="w-12 h-12 mx-auto mb-4 border-4 rounded-full animate-spin border-violet-800 border-t-transparent"></div>
          <p className="text-gray-600">Đang tải...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen px-4 py-4 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-6">
      <style>{`
        .dropdown { position: absolute; z-index: 10; background: white; border: 1px solid #e5e7eb; max-height: 200px; overflow-y: auto; width: 100%; border-radius: 14px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }
        .dropdown-item { padding: 10px 16px; cursor: pointer; font-size: 0.875rem; color: #111827; transition: background-color 0.2s; }
        .dropdown-item:hover { background-color: #f3e8ff; color: #6b21a8; }
        .dropdown-item:first-child { border-top-left-radius: 14px; border-top-right-radius: 14px; }
        .dropdown-item:last-child { border-bottom-left-radius: 14px; border-bottom-right-radius: 14px; }
      `}</style>

      <div className="container mx-auto max-w-7xl">
        <div className="flex items-center gap-3 mb-6">
          <ShoppingBag className="w-8 h-8 text-violet-800" />
          <h1 className="text-2xl font-bold text-gray-900">Giỏ hàng</h1>
        </div>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          {/* Cart Items */}
          <div className="space-y-4 lg:col-span-2">
            {cartItems.length === 0 ? (
              <div className="p-8 text-center bg-white border shadow-lg border-violet-100/50" style={{ borderRadius: '20px' }}>
                <ShoppingBag className="w-16 h-16 mx-auto mb-4 text-gray-300" />
                <p className="text-lg text-gray-500">Giỏ hàng trống</p>
              </div>
            ) : (
              cartItems.map(item => (
                <div key={item.cartId} className="flex p-6 bg-white border shadow-lg border-violet-100/50" style={{ borderRadius: '20px' }}>
                  <img
                    src={item.firstImage || 'https://via.placeholder.com/100'}
                    alt={item.productName}
                    className="object-cover w-24 h-24 mr-4"
                    style={{ borderRadius: '12px' }}
                  />
                  <div className="flex-1">
                    <h3 className="text-base font-bold text-gray-900">{item.productName}</h3>
                    <div className="flex flex-wrap gap-1 mt-1">
                      {Object.entries(item.attributes).map(([k, v]) => (
                        <span key={k} className="px-2 py-1 text-xs text-gray-600 bg-gray-100" style={{ borderRadius: '6px' }}>
                          {k}: {v}
                        </span>
                      ))}
                    </div>
                    <div className="flex items-center gap-2 mt-2">
                      <span className="text-lg font-bold text-violet-800">{formatPrice(item.discountedPrice)}</span>
                      {item.originalPrice > item.discountedPrice && (
                        <span className="text-sm text-gray-500 line-through">{formatPrice(item.originalPrice)}</span>
                      )}
                    </div>
                    <div className="flex items-center justify-between mt-3">
                      <div className="flex items-center gap-2">
                        <button className="p-1.5 bg-gray-100 hover:bg-gray-200 disabled:opacity-50" style={{ borderRadius: '8px' }} disabled={item.quantity <= 1}>
                          <Minus className="w-3.5 h-3.5" />
                        </button>
                        <span className="w-8 font-semibold text-center">{item.quantity}</span>
                        <button className="p-1.5 bg-gray-100 hover:bg-gray-200 disabled:opacity-50" style={{ borderRadius: '8px' }} disabled={item.quantity >= item.availableStock}>
                          <Plus className="w-3.5 h-3.5" />
                        </button>
                      </div>
                      <button className="flex items-center gap-1 text-sm font-medium text-red-600 hover:text-red-700">
                        <Trash2 className="w-4 h-4" /> Xóa
                      </button>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>

          {/* Checkout Form */}
          <div className="space-y-4">
            {/* Receiver Info */}
            <div className="p-6 bg-white border shadow-lg border-violet-100/50" style={{ borderRadius: '20px' }}>
              <h2 className="mb-4 text-lg font-bold text-gray-900">Thông tin nhận hàng</h2>
              <input
                type="text"
                placeholder="Họ và tên"
                value={receiverName}
                onChange={e => setReceiverName(e.target.value)}
                className="w-full px-4 py-3 mb-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-violet-800/20"
              />
              <input
                type="text"
                placeholder="Số điện thoại"
                value={receiverPhone}
                onChange={e => setReceiverPhone(e.target.value)}
                className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-violet-800/20"
              />
            </div>

            {/* Address */}
            <div className="p-6 bg-white border shadow-lg border-violet-100/50" style={{ borderRadius: '20px' }}>
              <div className="flex items-center justify-between mb-4">
                <h2 className="flex items-center gap-2 text-lg font-bold text-gray-900">
                  <MapPin className="w-5 h-5 text-violet-800" />
                  Địa chỉ giao hàng
                </h2>
                {customer?.standardShippingAddress && (
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={useDefaultAddress}
                      onChange={e => setUseDefaultAddress(e.target.checked)}
                      className="w-4 h-4 rounded text-violet-800"
                    />
                    <span className="text-sm">Dùng địa chỉ mặc định</span>
                  </label>
                )}
              </div>

              {!useDefaultAddress && (
                <>
                  <div className="space-y-3">
                    <div className="relative">
                      <MapPin className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 left-4 top-1/2" />
                      <input
                        type="text"
                        placeholder="Tìm tỉnh/thành"
                        value={provinceSearch}
                        onChange={e => { setProvinceSearch(e.target.value); setShowProvinceDropdown(true); }}
                        onFocus={() => setShowProvinceDropdown(true)}
                        className="w-full py-3 pl-12 pr-4 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-violet-800/20"
                      />
                      {showProvinceDropdown && provinceSearch && (
                        <ul className="mt-1 dropdown">
                          {filteredProvinces.map(p => (
                            <li key={p.PROVINCE_ID} className="dropdown-item" onClick={() => handleProvinceSelect(p)}>
                              {p.PROVINCE_NAME}
                            </li>
                          ))}
                          {filteredProvinces.length === 0 && <li className="dropdown-item">Không tìm thấy</li>}
                        </ul>
                      )}
                    </div>

                    <div className="relative">
                      <MapPin className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 left-4 top-1/2" />
                      <input
                        type="text"
                        placeholder="Tìm xã/phường"
                        value={wardSearch}
                        onChange={e => { setWardSearch(e.target.value); setShowWardDropdown(true); }}
                        onFocus={() => setShowWardDropdown(true)}
                        disabled={!currentNewDistrict}
                        className="w-full py-3 pl-12 pr-4 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-violet-800/20 disabled:opacity-50"
                      />
                      {showWardDropdown && wardSearch && (
                        <ul className="mt-1 dropdown">
                          {filteredWards.map(w => (
                            <li key={w.WARDS_ID} className="dropdown-item" onClick={() => handleWardSelect(w)}>
                              {w.WARDS_NAME}
                            </li>
                          ))}
                          {filteredWards.length === 0 && <li className="dropdown-item">Không tìm thấy</li>}
                        </ul>
                      )}
                    </div>

                    <textarea
                      placeholder="Số nhà, tên đường..."
                      value={detailAddress}
                      onChange={e => setDetailAddress(e.target.value)}
                      rows={2}
                      className="w-full px-4 py-3 border border-gray-300 resize-none rounded-xl focus:outline-none focus:ring-2 focus:ring-violet-800/20"
                    />
                  </div>
                </>
              )}

              {useDefaultAddress && customer?.standardShippingAddress && (
                <div className="p-3 border bg-violet-50 border-violet-200 rounded-xl">
                  <p className="text-sm text-gray-700">
                    {customer.standardShippingAddress.detailAddress}, {customer.standardShippingAddress.wardsName}, {customer.standardShippingAddress.provinceName}
                  </p>
                </div>
              )}
            </div>

            {/* Payment Method */}
            <div className="p-6 bg-white border shadow-lg border-violet-100/50" style={{ borderRadius: '20px' }}>
              <h2 className="flex items-center gap-2 mb-4 text-lg font-bold text-gray-900">
                <CreditCard className="w-5 h-5 text-violet-800" />
                Phương thức thanh toán
              </h2>
              <div className="space-y-3">
                <label className={`flex items-center gap-3 p-3 border-2 rounded-xl cursor-pointer transition-all ${selectedPayment === 'COD' ? 'border-violet-800 bg-violet-50' : 'border-gray-200 hover:border-violet-400'}`}>
                  <input type="radio" name="payment" value="COD" checked={selectedPayment === 'COD'} onChange={() => setSelectedPayment('COD')} className="w-4 h-4 text-violet-800" />
                  <span className="font-medium">Thanh toán khi nhận hàng (COD)</span>
                </label>
                <label className={`flex items-center gap-3 p-3 border-2 rounded-xl cursor-pointer transition-all ${selectedPayment === 'VNPAY' ? 'border-violet-800 bg-violet-50' : 'border-gray-200 hover:border-violet-400'}`}>
                  <input type="radio" name="payment" value="VNPAY" checked={selectedPayment === 'VNPAY'} onChange={() => setSelectedPayment('VNPAY')} className="w-4 h-4 text-violet-800" />
                  <span className="font-medium">Thanh toán qua VNPAY</span>
                </label>
                <div className="flex items-center gap-2 p-3 text-sm text-yellow-800 border border-yellow-200 bg-yellow-50 rounded-xl">
                  <AlertCircle className="w-4 h-4" />
                  <span>MoMo: Tính năng đang được phát triển</span>
                </div>
              </div>
            </div>

            {/* Summary */}
            <div className="p-6 bg-white border shadow-lg border-violet-100/50" style={{ borderRadius: '20px' }}>
              <h2 className="mb-4 text-lg font-bold text-gray-900">Tổng cộng</h2>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-600">Tạm tính</span>
                  <span className="font-semibold">{formatPrice(calculateTotal())}</span>
                </div>
                <div className="flex justify-between text-gray-500">
                  <span>Phí vận chuyển</span>
                  <span>Tính sau</span>
                </div>
                <div className="flex justify-between text-gray-500">
                  <span>VAT</span>
                  <span>Tính sau</span>
                </div>
                <div className="flex justify-between pt-3 border-t border-gray-200">
                  <span className="font-bold">Tổng tiền</span>
                  <span className="text-xl font-bold text-violet-800">{formatPrice(calculateTotal())}</span>
                </div>
              </div>

              <button
                onClick={handleCheckout}
                disabled={submitting || cartItems.length === 0}
                className="w-full mt-4 py-3.5 text-white font-semibold bg-gradient-to-r from-violet-700 to-violet-800 hover:from-violet-800 hover:to-violet-900 rounded-xl shadow-lg hover:shadow-xl transition-all disabled:opacity-50"
              >
                {submitting ? 'Đang xử lý...' : 'Đặt hàng'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}