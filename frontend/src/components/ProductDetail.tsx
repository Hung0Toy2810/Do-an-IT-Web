import { useState, useEffect } from 'react';
import { Heart, ShoppingCart, Star, Plus, Minus, CreditCard, Send } from 'lucide-react';

// ============= TYPES =============
interface ProductVariant {
  attributes: { [key: string]: string };
  stock: number;
  originalPrice: number; // Giá gốc cụ thể cho biến thể
  discountedPrice?: number; // Giá giảm cụ thể cho biến thể
}

interface Product {
  id: number;
  name: string;
  category: string;
  originalPrice: number;
  discountedPrice?: number;
  discountPercentage?: number;
  rating: number;
  reviewCount: number;
  images: string[];
  description: string;
  attributeOptions: { [key: string]: string[] };
  variants: ProductVariant[];
  specifications: { label: string; value: string }[];
}

interface Review {
  id: number;
  userName: string;
  rating: number;
  comment: string;
  date: string;
  avatar?: string;
}

// ============= MOCK API =============
const fetchProductDetail = async (productId: number): Promise<Product> => {
  await new Promise(resolve => setTimeout(resolve, 800));
  
  const products: { [key: number]: Product } = {
    1: {
      id: 1,
      name: 'iPhone 15 Pro Max',
      category: 'Điện thoại',
      originalPrice: 34990000,
      discountedPrice: 29990000,
      discountPercentage: 14,
      rating: 4.8,
      reviewCount: 1234,
      images: [
        'https://images.unsplash.com/photo-1678685888221-cda773a3dcdb?w=800&q=80',
        'https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=800&q=80',
        'https://images.unsplash.com/photo-1696446702403-d4eb1f6c3e7c?w=800&q=80',
      ],
      description: 'iPhone 15 Pro Max mang đến hiệu năng đỉnh cao với chip A17 Pro.',
      attributeOptions: {
        'Màu sắc': ['Titan Tự Nhiên', 'Titan Xanh', 'Titan Trắng'],
        'Dung lượng': ['256GB', '512GB', '1TB'],
      },
      variants: [
        { 
          attributes: { 'Màu sắc': 'Titan Tự Nhiên', 'Dung lượng': '256GB' }, 
          stock: 10, 
          originalPrice: 34990000, 
          discountedPrice: 29990000 
        },
        { 
          attributes: { 'Màu sắc': 'Titan Tự Nhiên', 'Dung lượng': '512GB' }, 
          stock: 5, 
          originalPrice: 37990000, 
          discountedPrice: 32990000 
        },
        { 
          attributes: { 'Màu sắc': 'Titan Tự Nhiên', 'Dung lượng': '1TB' }, 
          stock: 0, 
          originalPrice: 40990000, 
          discountedPrice: 35990000 
        },
        { 
          attributes: { 'Màu sắc': 'Titan Xanh', 'Dung lượng': '256GB' }, 
          stock: 8, 
          originalPrice: 34990000, 
          discountedPrice: 29990000 
        },
        { 
          attributes: { 'Màu sắc': 'Titan Xanh', 'Dung lượng': '512GB' }, 
          stock: 3, 
          originalPrice: 37990000, 
          discountedPrice: 32990000 
        },
        { 
          attributes: { 'Màu sắc': 'Titan Xanh', 'Dung lượng': '1TB' }, 
          stock: 2, 
          originalPrice: 40990000, 
          discountedPrice: 35990000 
        },
        { 
          attributes: { 'Màu sắc': 'Titan Trắng', 'Dung lượng': '256GB' }, 
          stock: 15, 
          originalPrice: 34990000, 
          discountedPrice: 29990000 
        },
        { 
          attributes: { 'Màu sắc': 'Titan Trắng', 'Dung lượng': '512GB' }, 
          stock: 0, 
          originalPrice: 37990000, 
          discountedPrice: 32990000 
        },
        { 
          attributes: { 'Màu sắc': 'Titan Trắng', 'Dung lượng': '1TB' }, 
          stock: 1, 
          originalPrice: 40990000, 
          discountedPrice: 35990000 
        },
      ],
      specifications: [
        { label: 'Màn hình', value: '6.7" Super Retina XDR OLED' },
        { label: 'Chip xử lý', value: 'Apple A17 Pro 6 nhân' },
        { label: 'RAM', value: '8GB' },
        { label: 'Camera sau', value: '48MP + 12MP + 12MP' },
        { label: 'Camera trước', value: '12MP' },
        { label: 'Pin', value: '4422 mAh' },
      ],
    },
  };

  return products[productId] || products[1];
};

const fetchProductReviews = async (productId: number): Promise<Review[]> => {
  await new Promise(resolve => setTimeout(resolve, 500));
  
  return [
    {
      id: 1,
      userName: 'Nguyễn Văn A',
      rating: 5,
      comment: 'Sản phẩm rất tốt, đúng như mô tả. Giao hàng nhanh!',
      date: '2025-01-15',
      avatar: 'https://i.pravatar.cc/150?img=1',
    },
    {
      id: 2,
      userName: 'Trần Thị B',
      rating: 4,
      comment: 'Chất lượng oke, giá hơi cao một chút.',
      date: '2025-01-10',
      avatar: 'https://i.pravatar.cc/150?img=2',
    },
    {
      id: 3,
      userName: 'Lê Văn C',
      rating: 5,
      comment: 'Máy đẹp, pin trâu, camera chụp sắc nét!',
      date: '2025-01-08',
      avatar: 'https://i.pravatar.cc/150?img=3',
    },
  ];
};

// ============= COMPONENT 1: PRODUCT IMAGES =============
interface ProductImagesProps {
  images: string[];
  productName: string;
  discountPercentage: number; // Phần trăm giảm giá tính động
  onToggleFavorite?: () => void;
  isFavorite?: boolean;
}

export const ProductImages: React.FC<ProductImagesProps> = ({
  images,
  productName,
  discountPercentage,
  onToggleFavorite,
  isFavorite = false,
}) => {
  const [selectedImage, setSelectedImage] = useState(0);

  return (
    <div 
      className="flex flex-col h-full overflow-hidden bg-white border shadow-lg border-violet-100/50"
      style={{ borderRadius: '20px' }}
    >
      <div className="relative flex-grow bg-gray-100">
        <img
          src={images[selectedImage]}
          alt={productName}
          className="object-cover w-full h-full"
        />

        {discountPercentage > 0 && (
          <div 
            className="absolute top-3 sm:top-4 left-3 sm:left-4 px-2 sm:px-3 py-1 sm:py-1.5 text-xs sm:text-sm font-bold text-white bg-violet-800 shadow-lg"
            style={{ borderRadius: '12px' }}
          >
            -{Math.round(discountPercentage)}%
          </div>
        )}

        <button
          onClick={onToggleFavorite}
          className="absolute top-3 sm:top-4 right-3 sm:right-4 p-2 sm:p-2.5 bg-white border border-gray-200 shadow-lg hover:bg-violet-50 transition-all"
          style={{ borderRadius: '12px' }}
        >
          <Heart
            className={`w-4 h-4 sm:w-5 sm:h-5 transition-colors ${
              isFavorite ? 'fill-violet-800 text-violet-800' : 'text-gray-400'
            }`}
          />
        </button>
      </div>

      {images.length > 1 && (
        <div className="flex justify-center gap-3 py-3 sm:gap-4 sm:py-4">
          {images.map((_, index) => (
            <button
              key={index}
              onClick={() => setSelectedImage(index)}
              className={`h-2.5 sm:h-3 rounded-full transition-all ${
                index === selectedImage
                  ? 'bg-violet-800 w-10 sm:w-12'
                  : 'bg-gray-300 hover:bg-violet-600 w-2.5 sm:w-3'
              }`}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// ============= COMPONENT 2: PRODUCT INFO =============
interface ProductInfoProps {
  product: Product;
  selectedAttributes: { [key: string]: string };
  quantity: number;
  currentStock: number;
  currentVariantPrice: { original: number; discounted?: number; discountPercentage: number };
  canAddToCart: boolean;
  onAttributeChange: (attributeName: string, value: string) => void;
  onQuantityChange: (delta: number) => void;
  onAddToCart: () => void;
  onBuyNow: () => void;
}

export const ProductInfo: React.FC<ProductInfoProps> = ({
  product,
  selectedAttributes,
  quantity,
  currentStock,
  currentVariantPrice,
  canAddToCart,
  onAttributeChange,
  onQuantityChange,
  onAddToCart,
  onBuyNow,
}) => {
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(price);
  };

  return (
    <div 
      className="flex flex-col h-full p-4 bg-white border shadow-lg border-violet-100/50 sm:p-6"
      style={{ borderRadius: '20px' }}
    >
      <p className="text-xs font-medium uppercase sm:text-sm text-violet-700">
        {product.category}
      </p>

      <h1 className="mt-2 text-xl font-bold text-gray-900 sm:text-2xl lg:text-3xl">
        {product.name}
      </h1>

      <div className="flex items-center gap-2 mt-2 sm:mt-3">
        <div className="flex items-center gap-0.5 sm:gap-1">
          {[...Array(5)].map((_, index) => (
            <Star
              key={index}
              className={`w-3 h-3 sm:w-4 sm:h-4 ${
                index < Math.floor(product.rating)
                  ? 'fill-yellow-400 text-yellow-400'
                  : 'text-gray-300'
              }`}
            />
          ))}
        </div>
        <span className="text-xs font-semibold text-gray-700 sm:text-sm">
          {product.rating}
        </span>
        <span className="text-xs text-gray-500 sm:text-sm">
          ({product.reviewCount} đánh giá)
        </span>
      </div>

      <div className="flex items-center gap-2 mt-3 sm:gap-3 sm:mt-4">
        {currentVariantPrice.discounted ? (
          <>
            <span className="text-2xl font-bold sm:text-3xl text-violet-800">
              {formatPrice(currentVariantPrice.discounted)}
            </span>
            <span className="text-base text-gray-500 line-through sm:text-lg">
              {formatPrice(currentVariantPrice.original)}
            </span>
          </>
        ) : (
          <span className="text-2xl font-bold text-gray-900 sm:text-3xl">
            {formatPrice(currentVariantPrice.original)}
          </span>
        )}
      </div>

      <p className="mt-3 text-xs leading-relaxed text-gray-600 sm:mt-4 sm:text-sm">
        {product.description}
      </p>

      {Object.entries(product.attributeOptions).map(([attributeName, options]) => (
        <div key={attributeName} className="mt-4 sm:mt-6">
          <label className="block mb-2 text-xs font-semibold text-gray-900 sm:text-sm sm:mb-3">
            {attributeName}: <span className="text-violet-700">{selectedAttributes[attributeName]}</span>
          </label>
          <div className="flex flex-wrap gap-2">
            {options.map((option) => (
              <button
                key={option}
                onClick={() => onAttributeChange(attributeName, option)}
                className={`px-3 sm:px-4 py-1.5 sm:py-2 text-xs sm:text-sm font-medium border-2 transition-all ${
                  selectedAttributes[attributeName] === option
                    ? 'border-violet-800 bg-violet-50 text-violet-900'
                    : 'border-gray-200 bg-white text-gray-700 hover:border-violet-400'
                }`}
                style={{ borderRadius: '10px' }}
              >
                {option}
              </button>
            ))}
          </div>
        </div>
      ))}

      <div className="mt-4 sm:mt-6">
        <span className={`text-xs sm:text-sm font-semibold ${
          currentStock > 0 ? 'text-green-600' : 'text-red-600'
        }`}>
          {currentStock > 0 ? `Còn ${currentStock} sản phẩm` : 'Hết hàng'}
        </span>
      </div>

      <div className="mt-4 sm:mt-6">
        <label className="block mb-2 text-xs font-semibold text-gray-900 sm:text-sm sm:mb-3">
          Số lượng
        </label>
        <div className="flex items-center gap-3">
          <button
            onClick={() => onQuantityChange(-1)}
            disabled={quantity <= 1 || currentStock === 0}
            className="p-1.5 sm:p-2 bg-gray-100 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
            style={{ borderRadius: '10px' }}
          >
            <Minus className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-gray-700" />
          </button>
          <span className="text-base sm:text-lg font-semibold text-gray-900 min-w-[2.5rem] sm:min-w-[3rem] text-center">
            {quantity}
          </span>
          <button
            onClick={() => onQuantityChange(1)}
            disabled={quantity >= currentStock || currentStock === 0}
            className="p-1.5 sm:p-2 bg-gray-100 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
            style={{ borderRadius: '10px' }}
          >
            <Plus className="w-3.5 h-3.5 sm:w-4 sm:h-4 text-gray-700" />
          </button>
        </div>
      </div>

      <div className="flex flex-col flex-shrink-0 gap-2 mt-4 sm:flex-row sm:gap-3 sm:mt-6">
        <button
          onClick={onAddToCart}
          disabled={!canAddToCart}
          className="flex-1 flex items-center justify-center gap-2 px-4 sm:px-6 py-2.5 sm:py-3.5 text-xs sm:text-sm font-semibold text-violet-800 bg-white border-2 border-violet-800 hover:bg-violet-50 shadow-sm hover:shadow-md hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
          style={{ borderRadius: '14px' }}
        >
          <ShoppingCart className="w-4 h-4 sm:w-5 sm:h-5" />
          Thêm vào giỏ
        </button>
        <button
          onClick={onBuyNow}
          disabled={!canAddToCart}
          className="flex-1 flex items-center justify-center gap-2 px-4 sm:px-6 py-2.5 sm:py-3.5 text-xs sm:text-sm font-semibold text-white bg-gradient-to-r from-violet-700 to-violet-800 hover:from-violet-800 hover:to-violet-900 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
          style={{ borderRadius: '14px' }}
        >
          <CreditCard className="w-4 h-4 sm:w-5 sm:h-5" />
          Mua ngay
        </button>
      </div>
    </div>
  );
};

// ============= COMPONENT 3: PRODUCT REVIEWS =============
interface ProductReviewsProps {
  reviews: Review[];
  productRating: number;
}

export const ProductReviews: React.FC<ProductReviewsProps> = ({
  reviews,
  productRating,
}) => {
  const [newComment, setNewComment] = useState('');
  const [newRating, setNewRating] = useState(5);

  const handleSubmitReview = () => {
    if (!newComment.trim()) {
      alert('Vui lòng nhập nội dung đánh giá');
      return;
    }
    alert(`Đã gửi đánh giá: ${newRating} sao - ${newComment}`);
    setNewComment('');
    setNewRating(5);
  };

  return (
    <div 
      className="p-4 bg-white border shadow-lg border-violet-100/50 sm:p-6"
      style={{ borderRadius: '20px' }}
    >
      <h2 className="mb-3 text-base font-bold text-gray-900 sm:text-lg sm:mb-4">Đánh giá sản phẩm</h2>

      <div 
        className="p-3 mb-4 border sm:mb-6 sm:p-4 bg-violet-50 border-violet-200"
        style={{ borderRadius: '14px' }}
      >
        <h3 className="mb-2 text-xs font-semibold text-gray-900 sm:text-sm sm:mb-3">Viết đánh giá của bạn</h3>
        
        <div className="flex items-center gap-2 mb-2 sm:mb-3">
          <span className="text-xs font-medium text-gray-700 sm:text-sm">Đánh giá:</span>
          {[1, 2, 3, 4, 5].map((star) => (
            <button
              key={star}
              onClick={() => setNewRating(star)}
              className="focus:outline-none"
            >
              <Star
                className={`w-4 h-4 sm:w-5 sm:h-5 transition-colors ${
                  star <= newRating
                    ? 'fill-yellow-400 text-yellow-400'
                    : 'text-gray-300 hover:text-yellow-400'
                }`}
              />
            </button>
          ))}
        </div>

        <textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Chia sẻ trải nghiệm của bạn về sản phẩm..."
          rows={3}
          className="w-full px-3 py-2 text-xs border border-gray-300 resize-none sm:px-4 sm:py-3 sm:text-sm focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800"
          style={{ borderRadius: '12px' }}
        />

        <button
          onClick={handleSubmitReview}
          className="mt-2 sm:mt-3 flex items-center gap-2 px-3 sm:px-4 py-1.5 sm:py-2 text-xs sm:text-sm font-semibold text-white bg-violet-800 hover:bg-violet-900 transition-all"
          style={{ borderRadius: '10px' }}
        >
          <Send className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
          Gửi đánh giá
        </button>
      </div>

      <div className="space-y-3 overflow-y-auto sm:space-y-4 max-h-96">
        {reviews.map((review) => (
          <div 
            key={review.id} 
            className="p-3 border border-gray-200 sm:p-4"
            style={{ borderRadius: '12px' }}
          >
            <div className="flex items-start gap-2 sm:gap-3">
              <img
                src={review.avatar}
                alt={review.userName}
                className="flex-shrink-0 w-8 h-8 rounded-full sm:w-10 sm:h-10"
              />
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between gap-2">
                  <h4 className="text-xs font-semibold text-gray-900 truncate sm:text-sm">{review.userName}</h4>
                  <span className="text-[10px] sm:text-xs text-gray-500 flex-shrink-0">{review.date}</span>
                </div>
                <div className="flex items-center gap-0.5 sm:gap-1 mt-1">
                  {[...Array(5)].map((_, index) => (
                    <Star
                      key={index}
                      className={`w-3 h-3 sm:w-3.5 sm:h-3.5 ${
                        index < review.rating
                          ? 'fill-yellow-400 text-yellow-400'
                          : 'text-gray-300'
                      }`}
                    />
                  ))}
                </div>
                <p className="mt-1.5 sm:mt-2 text-xs sm:text-sm text-gray-700">{review.comment}</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// ============= COMPONENT 4: PRODUCT SPECIFICATIONS =============
interface ProductSpecificationsProps {
  specifications: { label: string; value: string }[];
}

export const ProductSpecifications: React.FC<ProductSpecificationsProps> = ({
  specifications,
}) => {
  return (
    <div 
      className="p-4 bg-white border shadow-lg border-violet-100/50 sm:p-6"
      style={{ borderRadius: '20px' }}
    >
      <h2 className="mb-3 text-base font-bold text-gray-900 sm:text-lg sm:mb-4">Thông số kỹ thuật</h2>
      <div className="space-y-2 sm:space-y-3">
        {specifications.map((spec, index) => (
          <div
            key={index}
            className="flex justify-between py-2 border-b border-gray-100 sm:py-3 last:border-0"
          >
            <span className="text-xs font-medium text-gray-600 sm:text-sm">{spec.label}</span>
            <span className="text-xs font-semibold text-right text-gray-900 sm:text-sm">{spec.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
};

// ============= DEMO COMPONENT =============
export default function ProductDetailDemo() {
  const [product, setProduct] = useState<Product | null>(null);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedAttributes, setSelectedAttributes] = useState<{ [key: string]: string }>({});
  const [quantity, setQuantity] = useState(1);
  const [isLiked, setIsLiked] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      const [productData, reviewsData] = await Promise.all([
        fetchProductDetail(1),
        fetchProductReviews(1),
      ]);
      setProduct(productData);
      setReviews(reviewsData);
      
      const initialAttributes: { [key: string]: string } = {};
      Object.entries(productData.attributeOptions).forEach(([key, values]) => {
        initialAttributes[key] = values[0];
      });
      setSelectedAttributes(initialAttributes);
      
      setLoading(false);
    };
    loadData();
  }, []);

  if (loading || !product) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
        <div className="text-center">
          <div className="w-12 h-12 mx-auto mb-4 border-4 rounded-full animate-spin border-violet-800 border-t-transparent"></div>
          <p className="text-gray-600">Đang tải...</p>
        </div>
      </div>
    );
  }

  const currentVariant = product.variants.find(v =>
    Object.entries(selectedAttributes).every(([key, value]) => v.attributes[key] === value)
  );
  
  const currentStock = currentVariant?.stock || 0;
  const canAddToCart = currentStock > 0 && quantity <= currentStock;

  // Tính giá và phần trăm giảm giá dựa trên biến thể
  const currentVariantPrice = {
    original: currentVariant?.originalPrice || product.originalPrice,
    discounted: currentVariant?.discountedPrice || product.discountedPrice,
    discountPercentage: currentVariant && currentVariant.discountedPrice
      ? ((1 - currentVariant.discountedPrice / currentVariant.originalPrice) * 100)
      : (product.discountedPrice ? ((1 - product.discountedPrice / product.originalPrice) * 100) : 0),
  };

  const handleAttributeChange = (attributeName: string, value: string) => {
    setSelectedAttributes(prev => ({ ...prev, [attributeName]: value }));
    setQuantity(1);
  };

  const handleQuantityChange = (delta: number) => {
    const newQuantity = quantity + delta;
    if (newQuantity >= 1 && newQuantity <= currentStock) {
      setQuantity(newQuantity);
    }
  };

  return (
    <div className="min-h-screen py-4 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-6">
      <div className="container max-w-screen-xl mx-auto">
        {/* Row 1: Images & Info */}
        <div className="grid items-stretch grid-cols-1 gap-4 px-4 lg:px-8 sm:px-6 lg:grid-cols-2 sm:gap-6">
          <ProductImages
            images={product.images}
            productName={product.name}
            discountPercentage={currentVariantPrice.discountPercentage}
            onToggleFavorite={() => setIsLiked(!isLiked)}
            isFavorite={isLiked}
          />
          
          <ProductInfo
            product={product}
            selectedAttributes={selectedAttributes}
            quantity={quantity}
            currentStock={currentStock}
            currentVariantPrice={currentVariantPrice}
            canAddToCart={canAddToCart}
            onAttributeChange={handleAttributeChange}
            onQuantityChange={handleQuantityChange}
            onAddToCart={() => alert(`Thêm ${quantity} sản phẩm vào giỏ`)}
            onBuyNow={() => alert(`Mua ${quantity} sản phẩm`)}
          />
        </div>

        {/* Row 2: Reviews & Specifications */}
        <div className="grid grid-cols-1 gap-4 px-4 mt-4 lg:px-8 sm:px-6 lg:grid-cols-2 sm:gap-6 sm:mt-6">
          <div className="order-2 lg:order-1">
            <ProductReviews reviews={reviews} productRating={product.rating} />
          </div>
          <div className="order-1 lg:order-2">
            <ProductSpecifications specifications={product.specifications} />
          </div>
        </div>
      </div>
    </div>
  );
}