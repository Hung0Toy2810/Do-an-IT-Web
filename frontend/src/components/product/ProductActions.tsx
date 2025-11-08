// src/components/product/ProductActions.tsx
import { ShoppingCart, CreditCard, Plus, Minus } from 'lucide-react';

interface Props {
  price: { original: number; discounted?: number };
  stock: number;
  quantity: number;
  canAdd: boolean;
  onQuantityChange: (delta: number) => void;
  onAddToCart: () => void;
  onBuyNow: () => void;
}

const formatPrice = (p: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p * 1000);

export default function ProductActions({ price, stock, quantity, canAdd, onQuantityChange, onAddToCart, onBuyNow }: Props) {
  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        {price.discounted ? (
          <>
            <span className="text-2xl font-bold sm:text-3xl text-violet-800">{formatPrice(price.discounted)}</span>
            <span className="text-lg text-gray-500 line-through">{formatPrice(price.original)}</span>
          </>
        ) : (
          <span className="text-2xl font-bold text-gray-900 sm:text-3xl">{formatPrice(price.original)}</span>
        )}
      </div>

      <p className={`text-sm font-semibold ${stock > 0 ? 'text-green-600' : 'text-red-600'}`}>
        {stock > 0 ? `Còn ${stock} sản phẩm` : 'Hết hàng'}
      </p>

      <div className="flex items-center gap-3">
        <button
          onClick={() => onQuantityChange(-1)}
          disabled={quantity <= 1 || stock === 0}
          className="p-2 bg-gray-100 rounded-xl disabled:opacity-50"
        >
          <Minus className="w-4 h-4" />
        </button>
        <span className="w-12 text-lg font-semibold text-center">{quantity}</span>
        <button
          onClick={() => onQuantityChange(1)}
          disabled={quantity >= stock || stock === 0}
          className="p-2 bg-gray-100 rounded-xl disabled:opacity-50"
        >
          <Plus className="w-4 h-4" />
        </button>
      </div>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <button
          onClick={onAddToCart}
          disabled={!canAdd}
          className="flex items-center justify-center gap-2 py-3 text-sm font-semibold bg-white border-2 text-violet-800 border-violet-800 rounded-xl hover:bg-violet-50 disabled:opacity-50"
        >
          <ShoppingCart className="w-5 h-5" />
          Thêm vào giỏ
        </button>
        <button
          onClick={onBuyNow}
          disabled={!canAdd}
          className="flex items-center justify-center gap-2 py-3 text-sm font-semibold text-white bg-gradient-to-r from-violet-700 to-violet-800 rounded-xl hover:from-violet-800 hover:to-violet-900 disabled:opacity-50"
        >
          <CreditCard className="w-5 h-5" />
          Mua ngay
        </button>
      </div>
    </div>
  );
}