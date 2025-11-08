// src/pages/ProductDetail.tsx
import { useParams, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { notify } from '../components/NotificationProvider';

import ProductGallery from '../components/product/ProductGallery';
import ProductInfo from '../components/product/ProductInfo';
import ProductVariants from '../components/product/ProductVariants';
import ProductActions from '../components/product/ProductActions';
import ProductReviews from '../components/product/ProductReviews';
import ProductSpecifications from '../components/product/ProductSpecifications';

export default function ProductDetail() {
  const { slug } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [selectedAttrs, setSelectedAttrs] = useState<{ [k: string]: string }>({});
  const [quantity, setQuantity] = useState(1);
  const [isLiked, setIsLiked] = useState(false);

  useEffect(() => {
    const fetchProduct = async () => {
      if (!slug) return;
      try {
        const res = await fetch(`http://localhost:5067/api/products/slug/${slug}`);
        const json = await res.json();
        if (res.ok && json.data) {
          const p = json.data;
          setProduct(p);
          const init: any = {};
          Object.entries(p.attributeOptions).forEach(([k, v]) => init[k] = (v as string[])[0]);
          setSelectedAttrs(init);
        } else {
          notify('error', 'Không tìm thấy sản phẩm');
          navigate('/');
        }
      } catch {
        notify('error', 'Lỗi kết nối');
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
      ? ((1 - variant.discountedPrice / variant.originalPrice) * 100)
      : 0,
  };

  const stock = variant.stock;
  const canAdd = stock > 0 && quantity <= stock;

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
              setSelectedAttrs(prev => ({ ...prev, [k]: v }));
              setQuantity(1);
            }}
          />

          <ProductActions
            price={price}
            stock={stock}
            quantity={quantity}
            canAdd={canAdd}
            onQuantityChange={d => {
              const n = quantity + d;
              if (n >= 1 && n <= stock) setQuantity(n);
            }}
            onAddToCart={() => notify('success', `Đã thêm ${quantity} sản phẩm`)}
            onBuyNow={() => notify('info', 'Chuyển đến thanh toán...')}
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