import { useState, useEffect } from 'react';
import { Trash2, Plus, Minus, ShoppingBag, CreditCard, MapPin, Check } from 'lucide-react';

// ============= TYPES =============
interface CartItem {
  id: number;
  productId: number;
  name: string;
  image: string;
  price: number;
  originalPrice?: number;
  quantity: number;
  selectedOptions: { [key: string]: string };
  stock: number;
}

interface UserAddress {
  address: string;
  isDefault: boolean;
}

interface PaymentMethod {
  id: string;
  name: string;
  icon: string;
}

// ============= MOCK API =============
const fetchCartItems = async (): Promise<CartItem[]> => {
  await new Promise(resolve => setTimeout(resolve, 500));
  
  return [
    {
      id: 1,
      productId: 101,
      name: 'iPhone 15 Pro Max',
      image: 'https://images.unsplash.com/photo-1678685888221-cda773a3dcdb?w=200&q=80',
      price: 29990000,
      originalPrice: 34990000,
      quantity: 1,
      selectedOptions: {
        'Màu sắc': 'Titan Tự Nhiên',
        'Dung lượng': '256GB',
      },
      stock: 10,
    },
    {
      id: 2,
      productId: 102,
      name: 'AirPods Pro Gen 2',
      image: 'https://images.unsplash.com/photo-1606841837239-c5a1a4a07af7?w=200&q=80',
      price: 6490000,
      quantity: 2,
      selectedOptions: {
        'Màu sắc': 'Trắng',
      },
      stock: 50,
    },
  ];
};

const fetchUserAddress = async (): Promise<UserAddress | null> => {
  await new Promise(resolve => setTimeout(resolve, 400));
  
  return {
    address: '123 Đường ABC, Phường XYZ, Quận 1, TP.HCM',
    isDefault: true,
  };
};

const submitOrder = async (orderData: any): Promise<{ success: boolean; orderId?: string }> => {
  await new Promise(resolve => setTimeout(resolve, 1000));
  
  console.log('Order submitted:', orderData);
  return { success: true, orderId: 'ORD' + Date.now() };
};

// ============= SHOPPING CART COMPONENT =============
interface ShoppingCartProps {
  onCheckoutSuccess?: (orderId: string) => void;
}

export default function ShoppingCart({ onCheckoutSuccess }: ShoppingCartProps) {
  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [userAddress, setUserAddress] = useState<UserAddress | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  // Form states
  const [useDefaultAddress, setUseDefaultAddress] = useState(true);
  const [shippingAddress, setShippingAddress] = useState('');
  const [selectedPayment, setSelectedPayment] = useState('cod');

  const paymentMethods: PaymentMethod[] = [
    { id: 'cod', name: 'Thanh toán khi nhận hàng (COD)', icon: '💵' },
    { id: 'bank', name: 'Chuyển khoản ngân hàng', icon: '🏦' },
    { id: 'momo', name: 'Ví MoMo', icon: '📱' },
    { id: 'card', name: 'Thẻ tín dụng/ghi nợ', icon: '💳' },
  ];

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      const [items, address] = await Promise.all([
        fetchCartItems(),
        fetchUserAddress(),
      ]);
      setCartItems(items);
      setUserAddress(address);
      
      if (address) {
        setShippingAddress(address.address);
      }
      
      setLoading(false);
    };
    loadData();
  }, []);

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(price);
  };

  const handleQuantityChange = (itemId: number, delta: number) => {
    setCartItems(prev =>
      prev.map(item => {
        if (item.id === itemId) {
          const newQuantity = item.quantity + delta;
          if (newQuantity >= 1 && newQuantity <= item.stock) {
            return { ...item, quantity: newQuantity };
          }
        }
        return item;
      })
    );
  };

  const handleRemoveItem = (itemId: number) => {
    if (confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) {
      setCartItems(prev => prev.filter(item => item.id !== itemId));
    }
  };

  const calculateTotal = () => {
    return cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0);
  };

  const handleCheckout = async () => {
    // Validate
    if (cartItems.length === 0) {
      alert('Giỏ hàng trống');
      return;
    }

    const finalShippingAddress = useDefaultAddress && userAddress 
      ? userAddress.address 
      : shippingAddress;

    if (!finalShippingAddress) {
      alert('Vui lòng nhập địa chỉ giao hàng');
      return;
    }

    setSubmitting(true);
    
    const orderData = {
      items: cartItems,
      shippingAddress: finalShippingAddress,
      paymentMethod: selectedPayment,
      total: calculateTotal(),
    };

    const result = await submitOrder(orderData);
    
    setSubmitting(false);

    if (result.success && result.orderId) {
      alert(`Đặt hàng thành công! Mã đơn hàng: ${result.orderId}`);
      onCheckoutSuccess?.(result.orderId);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
        <div className="text-center">
          <div className="w-12 h-12 mx-auto mb-4 border-4 rounded-full animate-spin border-violet-800 border-t-transparent"></div>
          <p className="text-gray-600">Đang tải giỏ hàng...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen px-4 py-4 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-6">
      <div className="container mx-auto max-w-7xl">
        <div className="flex items-center gap-3 mb-4 sm:mb-6">
          <ShoppingBag className="w-6 h-6 sm:w-8 sm:h-8 text-violet-800" />
          <h1 className="text-2xl font-bold text-gray-900 sm:text-2xl">Giỏ hàng của bạn</h1>
        </div>

        <div className="grid grid-cols-1 gap-4 lg:grid-cols-3 sm:gap-6">
          {/* Left: Cart Items */}
          <div className="space-y-4 lg:col-span-2">
            {cartItems.length === 0 ? (
              <div 
                className="p-8 text-center bg-white border shadow-lg border-violet-100/50"
                style={{ borderRadius: '20px' }}
              >
                <ShoppingBag className="w-16 h-16 mx-auto mb-4 text-gray-300" />
                <p className="text-lg text-gray-500">Giỏ hàng trống</p>
              </div>
            ) : (
              cartItems.map(item => (
                <div
                  key={item.id}
                  className="flex p-4 bg-white border shadow-lg border-violet-100/50 sm:p-6"
                  style={{ borderRadius: '20px' }}
                >
                  <div className="flex w-full gap-4">
                    <img
                      src={item.image}
                      alt={item.name}
                      className="flex-shrink-0 object-cover w-20 h-20 sm:w-24 sm:h-24"
                      style={{ borderRadius: '12px' }}
                    />

                    <div className="flex flex-col flex-1 min-w-0">
                      <h3 className="mb-2 text-sm font-bold text-gray-900 sm:text-base">
                        {item.name}
                      </h3>

                      {/* Options - Fixed height container */}
                      <div className="mb-2" style={{ minHeight: '28px' }}>
                        {Object.keys(item.selectedOptions).length > 0 && (
                          <div className="flex flex-wrap gap-2">
                            {Object.entries(item.selectedOptions).map(([key, value]) => (
                              <span
                                key={key}
                                className="px-2 py-1 text-xs text-gray-600 bg-gray-100"
                                style={{ borderRadius: '6px' }}
                              >
                                {key}: {value}
                              </span>
                            ))}
                          </div>
                        )}
                      </div>

                      {/* Price - Fixed height container */}
                      <div className="mb-3" style={{ minHeight: '28px' }}>
                        <div className="flex items-center gap-2">
                          <span className="text-base font-bold sm:text-lg text-violet-800">
                            {formatPrice(item.price)}
                          </span>
                          {item.originalPrice && (
                            <span className="text-xs text-gray-500 line-through sm:text-sm">
                              {formatPrice(item.originalPrice)}
                            </span>
                          )}
                        </div>
                      </div>

                      {/* Quantity & Remove - Always at bottom */}
                      <div className="flex items-center justify-between mt-auto">
                        <div className="flex items-center gap-2">
                          <button
                            onClick={() => handleQuantityChange(item.id, -1)}
                            disabled={item.quantity <= 1}
                            className="p-1.5 bg-gray-100 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
                            style={{ borderRadius: '8px' }}
                          >
                            <Minus className="w-3.5 h-3.5 text-gray-700" />
                          </button>
                          <span className="text-sm font-semibold text-gray-900 min-w-[2rem] text-center">
                            {item.quantity}
                          </span>
                          <button
                            onClick={() => handleQuantityChange(item.id, 1)}
                            disabled={item.quantity >= item.stock}
                            className="p-1.5 bg-gray-100 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
                            style={{ borderRadius: '8px' }}
                          >
                            <Plus className="w-3.5 h-3.5 text-gray-700" />
                          </button>
                        </div>

                        <button
                          onClick={() => handleRemoveItem(item.id)}
                          className="flex items-center gap-1.5 text-xs sm:text-sm text-red-600 hover:text-red-700 font-medium transition-colors"
                        >
                          <Trash2 className="w-4 h-4" />
                          Xóa
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>

          {/* Right: Checkout Form */}
          <div className="space-y-4">
            {/* Shipping Address */}
            <div 
              className="p-4 bg-white border shadow-lg border-violet-100/50 sm:p-6"
              style={{ borderRadius: '20px' }}
            >
              <h2 className="flex items-center gap-2 mb-4 text-base font-bold text-gray-900 sm:text-lg">
                <MapPin className="w-5 h-5 text-violet-800" />
                Địa chỉ giao hàng
              </h2>

              {userAddress && (
                <label className="flex items-start gap-3 mb-4 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={useDefaultAddress}
                    onChange={(e) => setUseDefaultAddress(e.target.checked)}
                    className="w-4 h-4 mt-1 border-gray-300 rounded text-violet-800 focus:ring-violet-700"
                  />
                  <span className="text-sm text-gray-700">
                    Dùng địa chỉ giao hàng mặc định của bạn
                  </span>
                </label>
              )}

              {!useDefaultAddress && (
                <textarea
                  placeholder="Nhập địa chỉ giao hàng đầy đủ"
                  value={shippingAddress}
                  onChange={(e) => setShippingAddress(e.target.value)}
                  rows={3}
                  className="w-full px-4 py-2.5 text-sm border border-gray-300 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 resize-none"
                  style={{ borderRadius: '12px' }}
                />
              )}

              {useDefaultAddress && userAddress && (
                <div 
                  className="p-3 border bg-violet-50 border-violet-200"
                  style={{ borderRadius: '12px' }}
                >
                  <p className="text-sm text-gray-700">{userAddress.address}</p>
                </div>
              )}
            </div>

            {/* Payment Method */}
            <div 
              className="p-4 bg-white border shadow-lg border-violet-100/50 sm:p-6"
              style={{ borderRadius: '20px' }}
            >
              <h2 className="flex items-center gap-2 mb-4 text-base font-bold text-gray-900 sm:text-lg">
                <CreditCard className="w-5 h-5 text-violet-800" />
                Phương thức thanh toán
              </h2>

              <div className="space-y-2">
                {paymentMethods.map(method => (
                  <label
                    key={method.id}
                    className={`flex items-center gap-3 p-3 border-2 cursor-pointer transition-all ${
                      selectedPayment === method.id
                        ? 'border-violet-800 bg-violet-50'
                        : 'border-gray-200 hover:border-violet-400'
                    }`}
                    style={{ borderRadius: '12px' }}
                  >
                    <input
                      type="radio"
                      name="payment"
                      value={method.id}
                      checked={selectedPayment === method.id}
                      onChange={(e) => setSelectedPayment(e.target.value)}
                      className="w-4 h-4 border-gray-300 text-violet-800 focus:ring-violet-700"
                    />
                    <span className="text-xl">{method.icon}</span>
                    <span className="text-sm font-medium text-gray-900">{method.name}</span>
                  </label>
                ))}
              </div>
            </div>

            {/* Order Summary */}
            <div 
              className="p-4 bg-white border shadow-lg border-violet-100/50 sm:p-6"
              style={{ borderRadius: '20px' }}
            >
              <h2 className="mb-4 text-base font-bold text-gray-900 sm:text-lg">
                Tổng cộng
              </h2>

              <div className="space-y-3">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Tạm tính</span>
                  <span className="font-semibold text-gray-900">{formatPrice(calculateTotal())}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Phí vận chuyển</span>
                  <span className="font-semibold text-green-600">Miễn phí</span>
                </div>
                <div className="flex justify-between pt-3 border-t border-gray-200">
                  <span className="text-base font-bold text-gray-900">Tổng tiền</span>
                  <span className="text-xl font-bold text-violet-800">{formatPrice(calculateTotal())}</span>
                </div>
              </div>

              <button
                onClick={handleCheckout}
                disabled={cartItems.length === 0 || submitting}
                className="w-full mt-4 flex items-center justify-center gap-2 px-6 py-3.5 text-sm font-semibold text-white bg-gradient-to-r from-violet-700 to-violet-800 hover:from-violet-800 hover:to-violet-900 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
                style={{ borderRadius: '14px' }}
              >
                {submitting ? (
                  <>
                    <div className="w-5 h-5 border-2 border-white rounded-full animate-spin border-t-transparent"></div>
                    Đang xử lý...
                  </>
                ) : (
                  <>
                    <Check className="w-5 h-5" />
                    Đặt hàng
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}