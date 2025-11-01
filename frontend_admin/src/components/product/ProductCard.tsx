// src/components/product/ProductCard.tsx

import { Package, Trash2 } from 'lucide-react';
import { notify } from '../../utils/notify';
import { ProductCardDto } from '../../types/product.types';

interface ProductCardProps {
  product: ProductCardDto;
}

export const ProductCard = ({ product }: ProductCardProps) => {
  const discountPercent = product.originalPriceOfMinVariant > 0
    ? Math.round((1 - product.minDiscountedPrice / product.originalPriceOfMinVariant) * 100)
    : 0;

  return (
    <div className="overflow-hidden transition-shadow bg-white border rounded-xl hover:shadow-lg">
      <div className="relative aspect-square">
        {product.firstImage ? (
          <img 
            src={product.firstImage} 
            alt={product.name}
            className="object-cover w-full h-full"
          />
        ) : (
          <div className="flex items-center justify-center w-full h-full bg-gray-100">
            <Package className="w-16 h-16 text-gray-300" />
          </div>
        )}
        {discountPercent > 0 && (
          <div className="absolute px-2 py-1 text-xs font-bold text-white bg-red-500 rounded-lg top-2 right-2">
            -{discountPercent}%
          </div>
        )}
        {product.isDiscontinued && (
          <div className="absolute px-2 py-1 text-xs font-bold text-white bg-gray-600 rounded-lg top-2 left-2">
            Ngừng KD
          </div>
        )}
      </div>
      <div className="p-4">
        <p className="mb-1 text-xs text-gray-500">{product.brand}</p>
        <h4 className="mb-2 text-sm font-semibold line-clamp-2">{product.name}</h4>
        <div className="flex items-center gap-2 mb-3">
          <span className="text-lg font-bold text-violet-600">
            {product.minDiscountedPrice.toLocaleString('vi-VN')}đ
          </span>
          {discountPercent > 0 && (
            <span className="text-sm text-gray-400 line-through">
              {product.originalPriceOfMinVariant.toLocaleString('vi-VN')}đ
            </span>
          )}
        </div>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-1 text-sm">
            <span className="text-yellow-500">★</span>
            <span>{product.rating.toFixed(1)}</span>
            <span className="text-gray-400">({product.totalRatings})</span>
          </div>
          <button
            onClick={() => notify.warning('Chức năng xóa sản phẩm sẽ được thêm sau')}
            className="p-2 rounded-lg hover:bg-red-50"
          >
            <Trash2 className="w-4 h-4 text-red-600" />
          </button>
        </div>
      </div>
    </div>
  );
};