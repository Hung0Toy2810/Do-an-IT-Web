// src/pages/ProductDetail.tsx
import React, { useState, useEffect, useCallback } from 'react';
import { Heart, ShoppingCart, Star, Plus, Minus, CreditCard, Send, User } from 'lucide-react';
import { useParams, useNavigate } from 'react-router-dom';
import { getCookie } from '../utils/cookies';
import { notify } from '../components/NotificationProvider';
import { format } from 'date-fns';

// ============= TYPES =============
interface Variant {
  slug: string;
  attributes: { [key: string]: string };
  stock: number;
  originalPrice: number;
  discountedPrice?: number;
  images: string[];
  specifications: { label: string; value: string }[];
}

interface Product {
  id: number;
  name: string;
  slug: string;
  brand: string;
  description: string;
  isDiscontinued: boolean;
  variants: Variant[];
  attributeOptions: { [key: string]: string[] };
  rating: number;
  totalRatings: number;
}

interface CommentDto {
  id: number;
  content: string;
  rating: number;
  createdAt: string;
  customerName: string;
  avatarUrl: string;
}

// ============= API: ADD TO CART =============
const API_BASE = 'http://localhost:5067';

const addToCart = async (
  productId: number,
  variantSlug: string,
  quantity: number
): Promise<{ success: boolean; message: string }> => {
  const token = getCookie('auth_token');
  if (!token) {
    return { success: false, message: 'Vui lòng đăng nhập để thêm vào giỏ hàng' };
  }

  try {
    console.log('Calling POST /api/cart:', { productId, variantSlug, quantity });

    const response = await fetch(`${API_BASE}/api/cart`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
      body: JSON.stringify({ productId, variantSlug, quantity }),
    });

    const result = await response.json();
    console.log('API Response:', response.status, result);

    const message = result.message || 'Lỗi không xác định';
    return { success: response.ok, message };
  } catch (error) {
    console.error('Add to cart error:', error);
    return { success: false, message: 'Lỗi kết nối đến server' };
  }
};

// ============= COMPONENT: PRODUCT IMAGES =============
interface ProductImagesProps {
  images: string[];
  productName: string;
  discountPercentage: number;
  onToggleFavorite?: () => void;
  isFavorite?: boolean;
}

const ProductImages: React.FC<ProductImagesProps> = ({
  images,
  productName,
  discountPercentage,
  onToggleFavorite,
  isFavorite = false,
}) => {
  const [selectedImage, setSelectedImage] = useState(0);

  return (
    <div className="flex flex-col h-full overflow-hidden bg-white border shadow-2xl border-violet-100/50" style={{ borderRadius: '20px' }}>
      <div className="relative flex-grow bg-gray-100">
        <img
          src={images[selectedImage] || '/placeholder.svg'}
          alt={productName}
          className="object-cover w-full h-full"
          onError={(e) => (e.currentTarget.src = '/placeholder.svg')}
        />

        {discountPercentage > 0 && (
          <div className="absolute top-4 left-4 px-3 py-1.5 text-sm font-bold text-white bg-violet-800 shadow-lg" style={{ borderRadius: '12px' }}>
            -{Math.round(discountPercentage)}%
          </div>
        )}

        <button
          onClick={onToggleFavorite}
          className="absolute top-4 right-4 p-2.5 bg-white border border-gray-200 shadow-lg hover:bg-violet-50 transition-all"
          style={{ borderRadius: '12px' }}
        >
          <Heart className={`w-5 h-5 transition-colors ${isFavorite ? 'fill-violet-800 text-violet-800' : 'text-gray-400'}`} />
        </button>
      </div>

      {images.length > 1 && (
        <div className="flex justify-center gap-4 py-4">
          {images.map((_, index) => (
            <button
              key={index}
              onClick={() => setSelectedImage(index)}
              className={`h-3 rounded-full transition-all ${index === selectedImage ? 'bg-violet-800 w-12' : 'bg-gray-300 hover:bg-violet-600 w-3'}`}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// ============= COMPONENT: PRODUCT INFO =============
interface ProductInfoProps {
  product: Product;
  selectedAttributes: { [key: string]: string };
  quantity: number;
  currentStock: number;
  currentVariantPrice: { original: number; discounted?: number; discountPercentage: number };
  canAddToCart: boolean;
  adding?: boolean;
  onAttributeChange: (attributeName: string, value: string) => void;
  onQuantityChange: (delta: number) => void;
  onAddToCart: () => void;
  onBuyNow: () => void;
}

const ProductInfo: React.FC<ProductInfoProps> = ({
  product,
  selectedAttributes,
  quantity,
  currentStock,
  currentVariantPrice,
  canAddToCart,
  adding = false,
  onAttributeChange,
  onQuantityChange,
  onAddToCart,
  onBuyNow,
}) => {
  const formatPrice = (price: number) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price * 1000);
  };

  return (
    <div className="flex flex-col h-full p-6 bg-white border shadow-2xl border-violet-100/50 sm:p-8" style={{ borderRadius: '20px' }}>
      <p className="text-sm font-semibold uppercase text-violet-700">{product.brand}</p>
      <h1 className="mt-2 text-2xl font-bold text-transparent sm:text-3xl bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text">
        {product.name}
      </h1>

      <div className="flex items-center gap-2 mt-3">
        <div className="flex items-center gap-1">
          {[...Array(5)].map((_, i) => (
            <Star key={i} className={`w-4 h-4 sm:w-5 sm:h-5 ${i < Math.floor(product.rating) ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}`} />
          ))}
        </div>
        <span className="text-sm font-semibold text-gray-700">{product.rating.toFixed(1)}</span>
        <span className="text-sm text-gray-500">({product.totalRatings} đánh giá)</span>
      </div>

      <div className="flex items-center gap-3 mt-4">
        {currentVariantPrice.discounted ? (
          <>
            <span className="text-3xl font-bold text-violet-800">{formatPrice(currentVariantPrice.discounted)}</span>
            <span className="text-lg text-gray-500 line-through">{formatPrice(currentVariantPrice.original)}</span>
          </>
        ) : (
          <span className="text-3xl font-bold text-gray-900">{formatPrice(currentVariantPrice.original)}</span>
        )}
      </div>

      <p className="mt-4 text-sm leading-relaxed text-gray-600">{product.description}</p>

      {Object.entries(product.attributeOptions).map(([attr, options]) => (
        <div key={attr} className="mt-6 space-y-2">
          <label className="block text-sm font-semibold text-gray-700">
            {attr}: <span className="text-violet-700">{selectedAttributes[attr]}</span>
          </label>
          <div className="flex flex-wrap gap-2">
            {options.map((opt) => (
              <button
                key={opt}
                onClick={() => onAttributeChange(attr, opt)}
                className={`px-4 py-2.5 text-sm font-medium border-2 transition-all ${
                  selectedAttributes[attr] === opt
                    ? 'border-violet-800 bg-violet-50 text-violet-900'
                    : 'border-gray-200 bg-white text-gray-700 hover:border-violet-400'
                }`}
                style={{ borderRadius: '14px' }}
              >
                {opt}
              </button>
            ))}
          </div>
        </div>
      ))}

      <div className="mt-6">
        <span className={`text-sm font-semibold ${currentStock > 0 ? 'text-green-600' : 'text-red-600'}`}>
          {currentStock > 0 ? `Còn ${currentStock} sản phẩm` : 'Hết hàng'}
        </span>
      </div>

      <div className="mt-6 space-y-2">
        <label className="block text-sm font-semibold text-gray-700">Số lượng</label>
        <div className="flex items-center gap-3">
          <button
            onClick={() => onQuantityChange(-1)}
            disabled={quantity <= 1 || currentStock === 0}
            className="p-2 transition-all bg-gray-100 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
            style={{ borderRadius: '12px' }}
          >
            <Minus className="w-4 h-4 text-gray-700" />
          </button>
          <span className="text-lg font-semibold text-gray-900 min-w-[3rem] text-center">{quantity}</span>
          <button
            onClick={() => onQuantityChange(1)}
            disabled={quantity >= currentStock || currentStock === 0}
            className="p-2 transition-all bg-gray-100 hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
            style={{ borderRadius: '12px' }}
          >
            <Plus className="w-4 h-4 text-gray-700" />
          </button>
        </div>
      </div>

      <div className="flex gap-3 mt-6">
        <button
          onClick={onAddToCart}
          disabled={!canAddToCart || adding}
          className="flex-1 flex items-center justify-center gap-2 px-6 py-3.5 text-sm font-semibold text-violet-800 bg-white border-2 border-violet-800 hover:bg-violet-50 shadow-sm hover:shadow-md hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ borderRadius: '14px' }}
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
          disabled={!canAddToCart || adding}
          className="flex-1 flex items-center justify-center gap-2 px-6 py-3.5 text-sm font-semibold text-white bg-gradient-to-r from-violet-700 to-violet-800 hover:from-violet-800 hover:to-violet-900 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ borderRadius: '14px' }}
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
};

// ============= COMPONENT: PRODUCT REVIEWS =============
interface ProductReviewsProps {
  productId: number;
}

const ProductReviews: React.FC<ProductReviewsProps> = ({ productId }) => {
  const [comments, setComments] = useState<CommentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [nextLastCommentId, setNextLastCommentId] = useState<number | null>(null);

  const [newContent, setNewContent] = useState('');
  const [newRating, setNewRating] = useState(5);
  const [submitting, setSubmitting] = useState(false);

  const fetchComments = useCallback(
    async (lastCommentId?: number | null) => {
      if (lastCommentId === null) return;
      const isLoadMore = lastCommentId !== undefined;
      if (isLoadMore) setLoadingMore(true);
      else setLoading(true);

      try {
        const url = lastCommentId
          ? `http://localhost:5067/api/comments/product/${productId}?lastCommentId=${lastCommentId}&pageSize=5`
          : `http://localhost:5067/api/comments/product/${productId}`;

        const response = await fetch(url);
        const result = await response.json();

        if (response.ok && result.data) {
          const newComments: CommentDto[] = result.data.Comments || [];
          setHasMore(result.data.HasMore);
          setNextLastCommentId(result.data.NextLastCommentId);
          setComments((prev) => (isLoadMore ? [...prev, ...newComments] : newComments));
        } else {
          notify('error', result.message || 'Không thể tải đánh giá');
        }
      } catch (error) {
        notify('error', 'Không thể kết nối đến server');
      } finally {
        setLoading(false);
        setLoadingMore(false);
      }
    },
    [productId]
  );

  const handleSubmitReview = async () => {
    if (!newContent.trim()) {
      notify('warning', 'Vui lòng nhập nội dung đánh giá');
      return;
    }

    const token = getCookie('auth_token');
    if (!token) {
      notify('warning', 'Vui lòng đăng nhập để đánh giá');
      return;
    }

    setSubmitting(true);
    try {
      const response = await fetch('http://localhost:5067/api/comments', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify({ content: newContent, rating: newRating, productId }),
      });

      const result = await response.json();
      if (response.ok) {
        notify('success', result.message || 'Gửi đánh giá thành công');
        setNewContent('');
        setNewRating(5);
        await fetchComments();
      } else {
        notify('error', result.message || 'Gửi đánh giá thất bại');
      }
    } catch (error) {
      notify('error', 'Không thể kết nối đến server');
    } finally {
      setSubmitting(false);
    }
  };

  const handleLoadMore = () => nextLastCommentId && fetchComments(nextLastCommentId);

  useEffect(() => {
    fetchComments();
  }, [fetchComments]);

  const formatDate = (date: string) => {
    try {
      return format(new Date(date), 'dd/MM/yyyy HH:mm');
    } catch {
      return 'Vừa xong';
    }
  };

  return (
    <div className="p-6 bg-white border shadow-2xl border-violet-100/50 sm:p-8" style={{ borderRadius: '20px' }}>
      <h2 className="mb-6 text-lg font-bold text-gray-900 sm:text-xl">Đánh giá sản phẩm</h2>

      <div className="p-4 mb-6 border bg-violet-50 border-violet-200 sm:p-5" style={{ borderRadius: '14px' }}>
        <h3 className="mb-3 text-sm font-semibold text-gray-900">Viết đánh giá của bạn</h3>
        <div className="flex items-center gap-2 mb-3">
          <span className="text-sm font-medium text-gray-700">Đánh giá:</span>
          {[1, 2, 3, 4, 5].map((star) => (
            <button key={star} onClick={() => setNewRating(star)} className="focus:outline-none" disabled={submitting}>
              <Star
                className={`w-5 h-5 transition-colors ${
                  star <= newRating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300 hover:text-yellow-400'
                }`}
              />
            </button>
          ))}
        </div>
        <textarea
          value={newContent}
          onChange={(e) => setNewContent(e.target.value)}
          placeholder="Chia sẻ trải nghiệm..."
          rows={3}
          disabled={submitting}
          className="w-full px-4 py-3 text-sm transition-all border border-gray-200 resize-none bg-gray-50 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 focus:bg-white disabled:opacity-50"
          style={{ borderRadius: '14px' }}
        />
        <button
          onClick={handleSubmitReview}
          disabled={submitting || !newContent.trim()}
          className="flex items-center gap-2 px-4 py-2 mt-3 text-sm font-semibold text-white transition-all bg-violet-800 hover:bg-violet-900 disabled:opacity-50"
          style={{ borderRadius: '12px' }}
        >
          <Send className="w-4 h-4" />
          {submitting ? 'Đang gửi...' : 'Gửi'}
        </button>
      </div>

      <div className="space-y-4">
        {loading ? (
          <div className="flex justify-center py-8">
            <div className="w-8 h-8 border-4 rounded-full border-violet-800 border-t-transparent animate-spin"></div>
          </div>
        ) : comments.length === 0 ? (
          <p className="py-6 text-sm text-center text-gray-500">Chưa có đánh giá</p>
        ) : (
          <>
            {comments.map((c) => (
              <div key={c.id} className="p-4 border border-gray-200" style={{ borderRadius: '14px' }}>
                <div className="flex items-start gap-3">
                  <div className="w-10 h-10 overflow-hidden bg-gray-200 rounded-full">
                    {c.avatarUrl ? (
                      <img src={c.avatarUrl} alt={c.customerName} className="object-cover w-full h-full" />
                    ) : (
                      <div className="flex items-center justify-center w-full h-full bg-gradient-to-br from-violet-600 to-violet-800">
                        <User className="w-5 h-5 text-white" />
                      </div>
                    )}
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between">
                      <h4 className="text-sm font-semibold text-gray-900">{c.customerName}</h4>
                      <span className="text-xs text-gray-500">{formatDate(c.createdAt)}</span>
                    </div>
                    <div className="flex gap-1 mt-1">
                      {[...Array(5)].map((_, i) => (
                        <Star key={i} className={`w-3.5 h-3.5 ${i < c.rating ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}`} />
                      ))}
                    </div>
                    <p className="mt-2 text-sm text-gray-700 break-words">{c.content}</p>
                  </div>
                </div>
              </div>
            ))}
            {hasMore && (
              <button
                onClick={handleLoadMore}
                disabled={loadingMore}
                className="w-full py-3 mt-4 text-sm font-semibold transition-all bg-white border-2 shadow-sm text-violet-800 border-violet-200 hover:bg-violet-50 hover:shadow-md disabled:opacity-50"
                style={{ borderRadius: '14px' }}
              >
                {loadingMore ? 'Đang tải...' : 'Xem thêm đánh giá'}
              </button>
            )}
          </>
        )}
      </div>
    </div>
  );
};

// ============= COMPONENT: PRODUCT SPECIFICATIONS =============
interface ProductSpecificationsProps {
  specifications: { label: string; value: string }[];
}

const ProductSpecifications: React.FC<ProductSpecificationsProps> = ({ specifications }) => {
  return (
    <div className="p-6 bg-white border shadow-2xl border-violet-100/50 sm:p-8" style={{ borderRadius: '20px' }}>
      <h2 className="mb-6 text-lg font-bold text-gray-900 sm:text-xl">Thông số kỹ thuật</h2>
      <div className="space-y-3">
        {specifications.map((spec, i) => (
          <div key={i} className="flex justify-between py-3 border-b border-gray-100 last:border-0">
            <span className="text-sm font-medium text-gray-600">{spec.label}</span>
            <span className="text-sm font-semibold text-right text-gray-900">{spec.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
};

// ============= MAIN COMPONENT =============
export default function ProductDetail() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();

  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedAttributes, setSelectedAttributes] = useState<{ [key: string]: string }>({});
  const [quantity, setQuantity] = useState(1);
  const [isLiked, setIsLiked] = useState(false);
  const [addingToCart, setAddingToCart] = useState(false);

  useEffect(() => {
    const fetchProduct = async () => {
      if (!slug) return;
      setLoading(true);
      try {
        const response = await fetch(`http://localhost:5067/api/products/slug/${slug}`);
        const result = await response.json();
        if (response.ok && result.data) {
          const p: Product = result.data;
          setProduct(p);

          const initial: { [key: string]: string } = {};
          Object.entries(p.attributeOptions).forEach(([k, v]) => {
            initial[k] = v[0];
          });
          setSelectedAttributes(initial);
        } else {
          notify('error', result.message || 'Không tìm thấy sản phẩm');
          navigate('/');
        }
      } catch (error) {
        notify('error', 'Không thể kết nối đến server');
        navigate('/');
      } finally {
        setLoading(false);
      }
    };
    fetchProduct();
  }, [slug, navigate]);

  if (loading || !product) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
        <div className="text-center">
          <div className="w-12 h-12 mx-auto mb-4 border-4 rounded-full border-violet-800 border-t-transparent animate-spin"></div>
          <p className="text-gray-600">Đang tải...</p>
        </div>
      </div>
    );
  }

  const currentVariant = product.variants.find((v) =>
    Object.entries(selectedAttributes).every(([k, val]) => v.attributes[k] === val)
  ) || product.variants[0];

  const currentStock = currentVariant.stock;
  const canAddToCart = currentStock > 0 && quantity <= currentStock;

  const currentPrice = {
    original: currentVariant.originalPrice,
    discounted: currentVariant.discountedPrice,
    discountPercentage: currentVariant.discountedPrice
      ? ((1 - currentVariant.discountedPrice / currentVariant.originalPrice) * 100)
      : 0,
  };

  const handleAttributeChange = (attr: string, val: string) => {
    setSelectedAttributes((prev) => ({ ...prev, [attr]: val }));
    setQuantity(1);
  };

  const handleQuantityChange = (delta: number) => {
    const newQty = quantity + delta;
    if (newQty >= 1 && newQty <= currentStock) setQuantity(newQty);
  };

  const handleAddToCart = async () => {
    if (!canAddToCart || addingToCart) return;
    setAddingToCart(true);
    const { success, message } = await addToCart(product.id, currentVariant.slug, quantity);
    setAddingToCart(false);
    success ? notify('success', message) : notify('error', message);
  };

  const handleBuyNow = async () => {
    if (!canAddToCart || addingToCart) return;
    setAddingToCart(true);
    const { success, message } = await addToCart(product.id, currentVariant.slug, quantity);
    setAddingToCart(false);
    if (success) {
      notify('success', message);
      navigate('/cart', { replace: true });
    } else {
      notify('error', message);
    }
  };

  return (
    <div className="min-h-screen py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50 sm:py-12">
      <div className="container max-w-screen-xl px-4 mx-auto sm:px-6 lg:px-8">
        <div className="grid gap-6 lg:grid-cols-2 lg:gap-8">
          <ProductImages
            images={currentVariant.images}
            productName={product.name}
            discountPercentage={currentPrice.discountPercentage}
            onToggleFavorite={() => setIsLiked(!isLiked)}
            isFavorite={isLiked}
          />
          <ProductInfo
            product={product}
            selectedAttributes={selectedAttributes}
            quantity={quantity}
            currentStock={currentStock}
            currentVariantPrice={currentPrice}
            canAddToCart={canAddToCart}
            adding={addingToCart}
            onAttributeChange={handleAttributeChange}
            onQuantityChange={handleQuantityChange}
            onAddToCart={handleAddToCart}
            onBuyNow={handleBuyNow}
          />
        </div>

        <div className="grid gap-6 mt-6 lg:grid-cols-2 lg:gap-8 lg:mt-8">
          <div className="lg:order-1">
            <ProductReviews productId={product.id} />
          </div>
          <div className="lg:order-2">
            <ProductSpecifications specifications={currentVariant.specifications} />
          </div>
        </div>
      </div>
    </div>
  );
}