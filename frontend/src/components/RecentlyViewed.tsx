// src/components/RecentlyViewed.tsx
import React, { useState, useEffect } from 'react';
import ProductCard from './ProductCard';
import { getRecentProducts, clearRecentProducts } from '../utils/recentProducts';
import { X } from 'lucide-react';

const ITEMS_PER_PAGE = 20; // Có thể đổi thành 12, 15, 20...

const RecentlyViewed: React.FC = () => {
  const [currentPage, setCurrentPage] = useState(1);
  const allProducts = getRecentProducts();

  // Tính toán phân trang
  const totalPages = Math.ceil(allProducts.length / ITEMS_PER_PAGE);
  const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
  const currentProducts = allProducts.slice(startIndex, startIndex + ITEMS_PER_PAGE);

  useEffect(() => {
    // Nếu trang hiện tại vượt quá số trang có → quay về trang cuối
    if (currentPage > totalPages && totalPages > 0) {
      setCurrentPage(totalPages);
    }
  }, [allProducts.length, currentPage, totalPages]);

  const handleClearHistory = () => {
    if (confirm('Xóa toàn bộ lịch sử xem sản phẩm?')) {
      clearRecentProducts();
      setCurrentPage(1);
      window.location.reload(); // hoặc dùng state để re-render mượt hơn
    }
  };

  const renderPagination = () => {
    const pages = [];
    for (let i = 1; i <= totalPages; i++) {
      if (
        i === 1 ||
        i === totalPages ||
        (i >= currentPage - 1 && i <= currentPage + 1)
      ) {
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
        pages.push(
          <span key={i} className="px-2 py-2 text-violet-400">
            ...
          </span>
        );
      }
    }
    return pages;
  };

  if (allProducts.length === 0) {
    return null;
  }

  return (
    <section className="py-16 bg-violet-50 from-violet-50 via-purple-50 to-pink-50">
      <div className="px-4 mx-auto max-w-7xl sm:px-6 lg:px-8">
        {/* Tiêu đề + Xóa lịch sử */}
        <div className="flex flex-col mb-10 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="text-3xl font-bold sm:text-3xl text-violet-900">
              Sản phẩm bạn vừa xem
            </h2>
          </div>

          <button
            onClick={handleClearHistory}
            className="mt-4 sm:mt-0 flex items-center gap-2 px-5 py-2.5 text-sm font-medium text-red-600 bg-white border border-red-300 rounded-lg hover:bg-red-50 transition"
          >
            <X className="w-4 h-4" />
            Xóa lịch sử xem
          </button>
        </div>

        {/* Danh sách sản phẩm */}
        <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          {currentProducts.map((p) => (
            <ProductCard
              key={p.id}
              product={{
                ...p,
                category: p.category || 'Đã xem gần đây',
                discountPercentage: p.discountedPrice
                  ? Math.round(((p.originalPrice - p.discountedPrice) / p.originalPrice) * 100)
                  : undefined,
              }}
              onAddToCart={() => {}}
            />
          ))}
        </div>

        {/* Phân trang kiểu số đẹp */}
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

export default RecentlyViewed;