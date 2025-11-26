// src/pages/ProductDetail.tsx
import { useParams, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { getCookie } from '../utils/cookies';
import { notify } from '../components/NotificationProvider';
import { isTokenValid } from '../utils/auth';

import ProductGallery from '../components/product/ProductGallery';
import ProductInfo from '../components/product/ProductInfo';
import ProductVariants from '../components/product/ProductVariants';
import ProductActions from '../components/product/ProductActions';
import ProductReviews from '../components/product/ProductReviews';
import ProductSpecifications from '../components/product/ProductSpecifications';

const API_BASE = 'http://localhost:5067';

const addToCart = async (
  productId: number,
  variantSlug: string,
  quantity: number
): Promise<{ success: boolean; message: string }> => {
  const token = getCookie('auth_token');
  if (!token || !isTokenValid()) {
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

    return {
      success: response.ok,
      message,
    };
  } catch (error) {
    console.error('Add to cart error:', error);
    return { success: false, message: 'Lỗi kết nối đến server' };
  }
};

export default function ProductDetail() {
  const { slug } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [selectedAttrs, setSelectedAttrs] = useState<{ [k: string]: string }>({});
  const [quantity, setQuantity] = useState(1);
  const [isLiked, setIsLiked] = useState(false);
  const [addingToCart, setAddingToCart] = useState(false);

  useEffect(() => {
    const fetchProduct = async () => {
      if (!slug) return;
      try {
        const res = await fetch(`${API_BASE}/api/products/slug/${slug}`);
        const json = await res.json();
        if (res.ok && json.data) {
          const p = json.data;
          setProduct(p);
          const init: any = {};
          Object.entries(p.attributeOptions).forEach(([k, v]) => {
            init[k] = (v as string[])[0];
          });
          setSelectedAttrs(init);
        } else {
          notify('error', json.message || 'Không tìm thấy sản phẩm');
          navigate('/');
        }
      } catch {
        notify('error', 'Lỗi kết nối đến server');
        navigate('/');
      } finally {
        setLoading(false);
      }
    };
    fetchProduct();
  }, [slug, navigate]);

  if (loading || !product) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="w-10 h-10 border-4 rounded-full border-violet-800 border-t-transparent animate-spin"></div>
      </div>
    );
  }

  const variant = product.variants.find((v: any) =>
    Object.entries(selectedAttrs).every(([k, val]) => v.attributes[k] === val)
  ) || product.variants[0];

  const price = {
    original: variant.originalPrice,
    discounted: variant.discountedPrice,
    discountPercentage: variant.discountedPrice
      ? Math.round((1 - variant.discountedPrice / variant.originalPrice) * 100)
      : 0,
  };

  const stock = variant.stock;
  const canAdd = stock > 0 && quantity <= stock;

  const handleAddToCart = async () => {
    if (!canAdd || addingToCart) return;
    setAddingToCart(true);
    const { success, message } = await addToCart(product.id, variant.slug, quantity);
    setAddingToCart(false);
    success ? notify('success', message) : notify('error', message);
  };

  const handleBuyNow = async () => {
    if (!canAdd || addingToCart) return;
    setAddingToCart(true);
    const { success, message } = await addToCart(product.id, variant.slug, quantity);
    setAddingToCart(false);
    if (success) {
      notify('success', message);
      navigate('/cart', { replace: true });
    } else {
      notify('error', message);
    }
  };

  return (
    <div className="container px-4 py-6 mx-auto max-w-7xl sm:py-8">
      <div className="grid gap-6 lg:grid-cols-2 lg:gap-8">
        {/* Cột trái */}
        <ProductGallery
          images={variant.images}
          productName={product.name}
          discountPercentage={price.discountPercentage}
          isFavorite={isLiked}
          onToggleFavorite={() => setIsLiked(!isLiked)}
        />

        {/* Cột phải */}
        <div className="space-y-6">
          <ProductInfo
            name={product.name}
            brand={product.brand}
            description={product.description}
            rating={product.rating}
            totalRatings={product.totalRatings}
          />

          <ProductVariants
            attributeOptions={product.attributeOptions}
            selected={selectedAttrs}
            onChange={(k, v) => {
              setSelectedAttrs((prev) => ({ ...prev, [k]: v }));
              setQuantity(1);
            }}
          />

          {/* ĐÃ SỬA: onBuyNow được truyền đúng */}
          <ProductActions
            price={price}
            stock={stock}
            quantity={quantity}
            canAdd={canAdd}
            adding={addingToCart}
            onQuantityChange={(d) => {
              const n = quantity + d;
              if (n >= 1 && n <= stock) setQuantity(n);
            }}
            onAddToCart={handleAddToCart}
            onBuyNow={handleBuyNow}
          />
        </div>
      </div>

      {/* Dưới cùng */}
      <div className="grid gap-6 mt-8 lg:grid-cols-2 lg:gap-8">
        <ProductReviews productId={product.id} />
        <ProductSpecifications specs={variant.specifications} />
      </div>
    </div>
  );
}