// src/components/ProductCard.tsx
import React, { useState } from 'react';
import { Heart, ShoppingCart, Star } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

interface Product {
  id: number;
  name: string;
  image: string;
  originalPrice: number;
  discountedPrice?: number;
  discountPercentage?: number;
  rating: number;
  totalReviews: number;
  category: string;
  slug: string;
}

interface ProductCardProps {
  product: Product;
  onAddToCart?: (product: Product) => void;
  onToggleFavorite?: (productId: number) => void;
  isFavorite?: boolean;
}

const ProductCard: React.FC<ProductCardProps> = ({
  product,
  onAddToCart,
  onToggleFavorite,
  isFavorite = false,
}) => {
  const [isLiked, setIsLiked] = useState(isFavorite);
  const navigate = useNavigate();

  const handleToggleFavorite = (e: React.MouseEvent) => {
    e.stopPropagation();
    setIsLiked(!isLiked);
    onToggleFavorite?.(product.id);
  };

  const handleBuyNow = (e: React.MouseEvent) => {
    e.stopPropagation();
    onAddToCart?.(product);
  };

  const handleClickCard = () => {
    navigate(`/product/${product.slug}`);
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(price);
  };

  return (
    <div
      onClick={handleClickCard}
      className="relative flex flex-col overflow-hidden transition-all bg-white border border-gray-200 shadow-sm cursor-pointer rounded-2xl group hover:shadow-lg"
      style={{ fontFamily: 'Roboto, sans-serif' }}
    >
      {/* Image Container */}
      <div className="relative overflow-hidden bg-gray-100 aspect-square">
        <img
          src={product.image}
          alt={product.name}
          className="object-cover w-full h-full transition-transform duration-300 group-hover:scale-110"
          loading="lazy"
          onError={(e) => {
            e.currentTarget.src = '/placeholder.svg';
          }}
        />

        {/* Discount Label */}
        {product.discountPercentage && (
          <div className="absolute px-1.5 py-0.5 text-xs font-bold text-white rounded-xl top-1.5 left-1.5 bg-violet-800 sm:px-2.5 sm:top-2.5 sm:left-2.5">
            -{product.discountPercentage}%
          </div>
        )}

        {/* Favorite Button */}
        <button
          onClick={handleToggleFavorite}
          className="absolute p-1 transition-all bg-white border border-gray-200 shadow-sm top-1.5 right-1.5 rounded-md hover:bg-violet-50 focus:outline-none focus:ring-1 focus:ring-violet-800/20 sm:p-1.5 sm:top-2.5 sm:right-2.5"
          aria-label={isLiked ? 'Bỏ yêu thích' : 'Yêu thích'}
        >
          <Heart
            className={`w-3.5 h-3.5 sm:w-4 sm:h-4 transition-colors ${
              isLiked
                ? 'fill-violet-800 text-violet-800'
                : 'text-gray-400 hover:text-violet-800'
            }`}
          />
        </button>
      </div>

      {/* Content - Flex Layout */}
      <div className="flex flex-col flex-1 p-2.5 sm:p-3.5">
        {/* Top Content */}
        <div className="flex-shrink-0">
          {/* Category */}
          <p className="text-xs font-medium text-gray-500 uppercase">
            {product.category}
          </p>

          {/* Product Name */}
          <h3 className="mt-1 text-xs font-semibold leading-tight text-gray-900 sm:text-sm line-clamp-2">
            {product.name}
          </h3>

          {/* Rating */}
          <div className="flex items-center gap-1 mt-1.5 sm:mt-2 sm:gap-1.5">
            <div className="flex items-center gap-0.5">
              {[...Array(5)].map((_, index) => (
                <Star
                  key={index}
                  className={`w-2.5 h-2.5 sm:w-3.5 sm:h-3.5 ${
                    index < Math.floor(product.rating)
                      ? 'fill-yellow-400 text-yellow-400'
                      : 'text-gray-300'
                  }`}
                />
              ))}
            </div>
            <span className="text-xs text-gray-600">
              {product.rating}
            </span>
          </div>
        </div>

        {/* Spacer */}
        <div className="flex-grow"></div>

        {/* Bottom Content */}
        <div className="flex-shrink-0">
          {/* Price */}
          <div className="flex items-center gap-1.5 mt-2">
            {product.discountedPrice ? (
              <>
                <span className="text-xs font-bold text-violet-800 sm:text-base">
                  {formatPrice(product.discountedPrice)}
                </span>
                <span className="text-[10px] sm:text-xs text-gray-500 line-through">
                  {formatPrice(product.originalPrice)}
                </span>
              </>
            ) : (
              <span className="text-xs font-bold text-gray-900 sm:text-base">
                {formatPrice(product.originalPrice)}
              </span>
            )}
          </div>

          {/* Add to Cart Button */}
          <button
            onClick={handleBuyNow}
            className="flex items-center justify-center w-full gap-1 px-2 py-1 mt-2 text-sm font-medium text-white transition-all border shadow-sm bg-violet-800 border-violet-800 rounded-xl hover:bg-violet-900 focus:outline-none focus:ring-2 focus:ring-violet-300 sm:gap-2 sm:px-3 sm:py-2"
          >
            <ShoppingCart className="w-3 h-3 sm:w-4 sm:h-4" />
            Mua ngay
          </button>
        </div>
      </div>
    </div>
  );
};

export default ProductCard;