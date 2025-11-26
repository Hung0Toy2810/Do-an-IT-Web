// src/components/product/ProductActions.tsx
import { ShoppingCart, CreditCard, Plus, Minus } from 'lucide-react';

interface Props {
  price: { original: number; discounted?: number };
  stock: number;
  quantity: number;
  canAdd: boolean;
  adding?: boolean;
  onQuantityChange: (delta: number) => void;
  onAddToCart: () => void;
  onBuyNow: () => void;
}

const formatPrice = (p: number) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p * 1000);

export default function ProductActions({
  price,
  stock,
  quantity,
  canAdd,
  adding = false,
  onQuantityChange,
  onAddToCart,
  onBuyNow,
}: Props) {
  return (
    <div className="space-y-4">
      {/* Giá */}
      <div className="flex items-center gap-3">
        {price.discounted ? (
          <>
            <span className="text-2xl font-bold sm:text-3xl text-violet-800">
              {formatPrice(price.discounted)}
            </span>
            <span className="text-lg text-gray-500 line-through">
              {formatPrice(price.original)}
            </span>
          </>
        ) : (
          <span className="text-2xl font-bold text-gray-900 sm:text-3xl">
            {formatPrice(price.original)}
          </span>
        )}
      </div>

      {/* Tồn kho */}
      <p className={`text-sm font-semibold ${stock > 0 ? 'text-green-600' : 'text-red-600'}`}>
        {stock > 0 ? `Còn ${stock} sản phẩm` : 'Hết hàng'}
      </p>

      {/* Số lượng */}
      <div className="flex items-center gap-3">
        <button
          onClick={() => onQuantityChange(-1)}
          disabled={quantity <= 1 || stock === 0}
          className="p-2 transition-colors bg-gray-100 rounded-xl disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <Minus className="w-4 h-4" />
        </button>
        <span className="w-12 text-lg font-semibold text-center">{quantity}</span>
        <button
          onClick={() => onQuantityChange(1)}
          disabled={quantity >= stock || stock === 0}
          className="p-2 transition-colors bg-gray-100 rounded-xl disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <Plus className="w-4 h-4" />
        </button>
      </div>

      {/* Nút hành động */}
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <button
          onClick={onAddToCart}
          disabled={!canAdd || adding}
          className="flex items-center justify-center gap-2 py-3 text-sm font-semibold transition-all bg-white border-2 text-violet-800 border-violet-800 rounded-xl hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {adding ? (
            <>
              <div className="w-4 h-4 border-2 rounded-full border-violet-800 border-t-transparent animate-spin"></div>
              Đang thêm...
            </>
          ) : (
            <>
              <ShoppingCart className="w-5 h-5" />
              Thêm vào giỏ
            </>
          )}
        </button>

        <button
          onClick={onBuyNow}
          disabled={!canAdd || adding}
          className="flex items-center justify-center gap-2 py-3 text-sm font-semibold text-white transition-all shadow-lg bg-gradient-to-r from-violet-700 to-violet-800 rounded-xl hover:from-violet-800 hover:to-violet-900 disabled:opacity-50 disabled:cursor-not-allowed hover:shadow-xl"
        >
          {adding ? (
            <>
              <div className="w-4 h-4 border-2 border-white rounded-full border-t-transparent animate-spin"></div>
              Đang xử lý...
            </>
          ) : (
            <>
              <CreditCard className="w-5 h-5" />
              Mua ngay
            </>
          )}
        </button>
      </div>
    </div>
  );
}