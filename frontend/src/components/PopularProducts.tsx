// src/components/PopularProducts.tsx
import React, { useState, useEffect } from 'react';
import ProductCard from './ProductCard';
import { Loader2 } from 'lucide-react';

const ITEMS_PER_PAGE = 20;

interface ProductCardDto {
  id: number;
  name: string;
  slug: string;
  brand: string;
  firstImage?: string;
  minDiscountedPrice: number;
  originalPriceOfMinVariant: number;
  rating: number;
  totalRatings: number;
}

const PopularProducts: React.FC = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const [products, setProducts] = useState<ProductCardDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalItems, setTotalItems] = useState(0);

  const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        const res = await fetch('http://localhost:5067/api/featured/top-30-today');
        if (!res.ok) throw new Error();
        const ids: number[] = await res.json();

        setTotalItems(ids.length);
        if (ids.length === 0) return setProducts([]);

        const start = (currentPage - 1) * ITEMS_PER_PAGE;
        const end = start + ITEMS_PER_PAGE;
        const pageIds = ids.slice(start, end);

        const details = await Promise.all(
          pageIds.map(id =>
            fetch(`http://localhost:5067/api/products/card/${id}`)
              .then(r => r.json())
              .catch(() => ({ data: null }))
          )
        );

        const newProducts: ProductCardDto[] = details
          .filter(r => r.data)
          .map(r => ({
            id: r.data.id,
            name: r.data.name,
            slug: r.data.slug,
            brand: r.data.brand,
            firstImage: r.data.firstImage || '/placeholder.svg',
            minDiscountedPrice: r.data.minDiscountedPrice,
            originalPriceOfMinVariant: r.data.originalPriceOfMinVariant,
            rating: r.data.rating,
            totalRatings: r.data.totalRatings,
          }));

        setProducts(prev => currentPage === 1 ? newProducts : [...prev, ...newProducts]);
      } catch (err) {
        console.error('Lỗi tải sản phẩm phổ biến:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [currentPage]);

  useEffect(() => {
    if (totalPages > 0 && currentPage > totalPages) {
      setCurrentPage(totalPages);
    }
  }, [totalPages]);

  const renderPagination = () => {
    const pages = [];
    for (let i = 1; i <= totalPages; i++) {
      if (i === 1 || i === totalPages || (i >= currentPage - 1 && i <= currentPage + 1)) {
        pages.push(
          <button
            key={i}
            onClick={() => setCurrentPage(i)}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-all ${
              currentPage === i
                ? 'bg-violet-600 text-white shadow-md'
                : 'bg-white text-violet-700 border border-violet-300 hover:bg-violet-50'
            }`}
          >
            {i}
          </button>
        );
      } else if (i === currentPage - 2 || i === currentPage + 2) {
        pages.push(<span key={i} className="px-2 py-2 text-violet-400">...</span>);
      }
    }
    return pages;
  };

  if (loading && products.length === 0) {
    return (
      <section className="py-16 bg-violet-50 from-violet-50 via-purple-50 to-pink-50">
        <div className="px-4 mx-auto text-center max-w-7xl sm:px-6 lg:px-8">
          <Loader2 className="w-12 h-12 mx-auto animate-spin text-violet-600" />
          <p className="mt-4 text-violet-700">Đang tải sản phẩm đang hot...</p>
        </div>
      </section>
    );
  }

  if (products.length === 0) return null;

  return (
    <section className="py-16 bg-violet-50 from-violet-50 via-purple-50 to-pink-50">
      <div className="px-4 mx-auto max-w-7xl sm:px-6 lg:px-8">
        <div className="flex flex-col mb-10 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-3xl font-bold text-violet-900">
              Đang hot hôm nay
            </h2>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          {products.map((p) => (
            <ProductCard
              key={p.id}
              product={{
                id: p.id,
                name: p.name,
                slug: p.slug,
                brand: p.brand,                                 // Hiển thị đúng brand
                image: p.firstImage || '/placeholder.svg',
                originalPrice: p.originalPriceOfMinVariant,     // Không nhân 1000
                discountedPrice: p.minDiscountedPrice,          // Không nhân 1000
                discountPercentage: p.minDiscountedPrice < p.originalPriceOfMinVariant
                  ? Math.round(((p.originalPriceOfMinVariant - p.minDiscountedPrice) / p.originalPriceOfMinVariant) * 100)
                  : undefined,
                rating: p.rating,
                totalReviews: p.totalRatings,
              }}
              onAddToCart={() => {}}
            />
          ))}
        </div>

        {totalPages > 1 && (
          <div className="flex items-center justify-center gap-3 mt-12">
            <button
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
              className="px-5 py-2.5 text-sm font-medium bg-white border border-violet-300 text-violet-700 rounded-lg hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
            >
              ← Trước
            </button>

            <div className="flex items-center gap-2">
              {renderPagination()}
            </div>

            <button
              onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages}
              className="px-5 py-2.5 text-sm font-medium bg-white border border-violet-300 text-violet-700 rounded-lg hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed transition"
            >
              Sau →
            </button>
          </div>
        )}
      </div>
    </section>
  );
};

export default PopularProducts;