// src/pages/SubCategoryPage.tsx
import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useSearchParams, useNavigate } from 'react-router-dom';
import { Loader2, Filter } from 'lucide-react';
import ProductCard from '../components/ProductCard';
import { notify } from '../components/NotificationProvider';

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

const SubCategoryPage: React.FC = () => {
  const { slug } = useParams<{ slug: string }>();
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();

  const [products, setProducts] = useState<ProductCardDto[]>([]);
  const [brands, setBrands] = useState<string[]>([]);
  const [selectedBrand, setSelectedBrand] = useState(searchParams.get('brand') || '');
  const [minPrice, setMinPrice] = useState(searchParams.get('min') || '');
  const [maxPrice, setMaxPrice] = useState(searchParams.get('max') || '');
  const [sortAsc, setSortAsc] = useState<'' | 'true' | 'false'>(
    (searchParams.get('sort') as '' | 'true' | 'false') || ''
  );

  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [totalCount, setTotalCount] = useState(0);

  const observer = useRef<IntersectionObserver | null>(null);
  const lastProductRef = useCallback(
    (node: HTMLDivElement | null) => {
      if (loadingMore) return;
      if (observer.current) observer.current.disconnect();
      observer.current = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting && hasMore) {
          setPage((prev) => prev + 1);
        }
      });
      if (node) observer.current.observe(node);
    },
    [loadingMore, hasMore]
  );

  const buildQuery = useCallback(() => {
    const params = new URLSearchParams();
    if (selectedBrand) params.append('brand', selectedBrand);
    if (minPrice) params.append('minPrice', (parseFloat(minPrice) * 1000).toString());
    if (maxPrice) params.append('maxPrice', (parseFloat(maxPrice) * 1000).toString());
    if (sortAsc) params.append('sortByPriceAscending', sortAsc);
    params.append('page', page.toString());
    params.append('pageSize', '12');
    return params.toString();
  }, [selectedBrand, minPrice, maxPrice, sortAsc, page]);

  const fetchProducts = useCallback(async () => {
    if (!slug) return;

    const isFirstPage = page === 1;
    if (isFirstPage) {
      setLoading(true);
      setProducts([]);
    } else {
      setLoadingMore(true);
    }

    try {
      const query = buildQuery();
      const res = await fetch(`http://localhost:5067/api/products/subcategory/${slug}?${query}`);
      if (!res.ok) throw new Error('Lỗi server');

      const json = await res.json();
      console.log('SubCategory API Response:', json);

      const { productIds = [], totalCount = 0 } = json.data || {};

      setTotalCount(totalCount);
      setHasMore(page * 12 < totalCount);

      if (productIds.length === 0) {
        setProducts([]);
        return;
      }

      const details = await Promise.all(
        productIds.map((id: number) =>
          fetch(`http://localhost:5067/api/products/card/${id}`)
            .then((r) => r.json())
            .catch(() => ({ data: null }))
        )
      );

      const newProducts: ProductCardDto[] = details
        .filter((r) => r.data)
        .map((r) => ({
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

      setProducts((prev) => (isFirstPage ? newProducts : [...prev, ...newProducts]));
    } catch (err) {
      notify('error', 'Không thể tải sản phẩm');
      console.error(err);
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, [slug, buildQuery, page]);

  const fetchBrands = useCallback(async () => {
    if (!slug) return;
    try {
      const res = await fetch(`http://localhost:5067/api/products/subcategory/${slug}/brands`);
      if (!res.ok) throw new Error('Lỗi server');
      const json = await res.json();
      console.log('Brands API Response:', json);
      setBrands(json.data?.brands || []);
    } catch (err) {
      console.error(err);
    }
  }, [slug]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  useEffect(() => {
    fetchBrands();
  }, [fetchBrands]);

  useEffect(() => {
    const params = new URLSearchParams();
    if (selectedBrand) params.set('brand', selectedBrand);
    if (minPrice) params.set('min', minPrice);
    if (maxPrice) params.set('max', maxPrice);
    if (sortAsc) params.set('sort', sortAsc);
    setSearchParams(params);
  }, [selectedBrand, minPrice, maxPrice, sortAsc, setSearchParams]);

  useEffect(() => {
    const brand = searchParams.get('brand') || '';
    const min = searchParams.get('min') || '';
    const max = searchParams.get('max') || '';
    const sort = searchParams.get('sort') || '';

    if (brand !== selectedBrand || min !== minPrice || max !== maxPrice || sort !== sortAsc) {
      setSelectedBrand(brand);
      setMinPrice(min);
      setMaxPrice(max);
      setSortAsc(sort as any);
      setPage(1);
    }
  }, [searchParams]);

  useEffect(() => {
    setPage(1);
  }, [selectedBrand, minPrice, maxPrice, sortAsc]);

  const handleBuyNow = (product: ProductCardDto) => {
    navigate(`/product/${product.slug}`);
  };

  const clearFilters = () => {
    setSelectedBrand('');
    setMinPrice('');
    setMaxPrice('');
    setSortAsc('');
    setSearchParams({});
    setPage(1);
  };

  const handlePageChange = (newPage: number) => {
    setPage(newPage);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const totalPages = Math.ceil(totalCount / 12);

  const renderPagination = () => {
    const pages = [];
    
    for (let i = 1; i <= totalPages; i++) {
      // Hiển thị trang đầu, trang cuối, và các trang gần trang hiện tại
      if (
        i === 1 ||
        i === totalPages ||
        (i >= page - 1 && i <= page + 1)
      ) {
        pages.push(
          <button
            key={i}
            onClick={() => handlePageChange(i)}
            disabled={loadingMore}
            className={`px-4 py-2 text-sm font-medium rounded-lg transition-colors ${
              page === i
                ? 'bg-violet-600 text-white shadow-md'
                : 'bg-white text-violet-700 border border-violet-300 hover:bg-violet-50'
            } disabled:opacity-50 disabled:cursor-not-allowed`}
          >
            {i}
          </button>
        );
      } else if (i === page - 2 || i === page + 2) {
        pages.push(
          <span key={i} className="px-2 py-2 text-violet-400">
            ...
          </span>
        );
      }
    }
    
    return pages;
  };

  if (!slug) {
    return <div className="py-24 text-center">Không tìm thấy danh mục</div>;
  }

  return (
    <div className="min-h-screen py-12 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
      <div className="container px-4 mx-auto max-w-7xl">
        <h1 className="mb-8 text-3xl font-bold capitalize text-violet-900">
          {slug.replace(/-/g, ' ')}
        </h1>

        <div className="flex flex-wrap items-center gap-3 mb-8 text-sm">
          <Filter className="w-4 h-4 text-violet-700" />
          <span className="font-medium text-violet-800">Lọc:</span>

          <select
            value={selectedBrand}
            onChange={(e) => setSelectedBrand(e.target.value)}
            className="px-3 py-1 border rounded border-violet-300 focus:ring-2 focus:ring-violet-500"
          >
            <option value="">Tất cả thương hiệu</option>
            {brands.map((b) => (
              <option key={b} value={b}>
                {b}
              </option>
            ))}
          </select>

          <input
            type="number"
            placeholder="Từ (nghìn)"
            value={minPrice}
            onChange={(e) => setMinPrice(e.target.value)}
            className="w-24 px-2 py-1 border rounded border-violet-300"
          />
          <span>-</span>
          <input
            type="number"
            placeholder="Đến (nghìn)"
            value={maxPrice}
            onChange={(e) => setMaxPrice(e.target.value)}
            className="w-24 px-2 py-1 border rounded border-violet-300"
          />

          <select
            value={sortAsc}
            onChange={(e) => setSortAsc(e.target.value as any)}
            className="px-3 py-1 border rounded border-violet-300"
          >
            <option value="">Mặc định</option>
            <option value="true">Giá tăng dần</option>
            <option value="false">Giá giảm dần</option>
          </select>

          {(selectedBrand || minPrice || maxPrice || sortAsc) && (
            <button
              onClick={clearFilters}
              className="px-3 py-1 text-red-600 border border-red-300 rounded hover:bg-red-50"
            >
              Xóa
            </button>
          )}
        </div>

        {loading ? (
          <div className="flex justify-center py-24">
            <Loader2 className="w-10 h-10 text-violet-800 animate-spin" />
          </div>
        ) : products.length === 0 ? (
          <div className="py-24 text-center">
            <p className="text-lg text-gray-600">Không có sản phẩm nào trong danh mục này</p>
          </div>
        ) : (
          <>
            <p className="mb-6 text-sm text-gray-600">
              Hiển thị <strong className="text-violet-800">{products.length}</strong> trong{' '}
              <strong className="text-violet-800">{totalCount}</strong> sản phẩm
            </p>

            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
              {products.map((product, index) => (
                <div
                  key={product.id}
                  ref={index === products.length - 1 ? lastProductRef : null}
                >
                  <ProductCard
                    product={{
                      id: product.id,
                      slug: product.slug,
                      name: product.name,
                      image: product.firstImage || '/placeholder.svg',
                      originalPrice: product.originalPriceOfMinVariant,
                      discountedPrice: product.minDiscountedPrice,
                      discountPercentage:
                        product.minDiscountedPrice < product.originalPriceOfMinVariant
                          ? Math.round(
                              ((product.originalPriceOfMinVariant - product.minDiscountedPrice) /
                                product.originalPriceOfMinVariant) *
                                100
                            )
                          : undefined,
                      rating: product.rating,
                      totalReviews: product.totalRatings,
                      category: product.brand,
                    }}
                    onAddToCart={() => handleBuyNow(product)}
                  />
                </div>
              ))}
            </div>

            {loadingMore && (
              <div className="flex justify-center py-12">
                <Loader2 className="w-8 h-8 text-violet-800 animate-spin" />
              </div>
            )}

            {/* Number Pagination */}
            {totalCount > 12 && (
              <div className="flex items-center justify-center gap-2 py-12 mt-8">
                <button
                  onClick={() => handlePageChange(Math.max(1, page - 1))}
                  disabled={page === 1 || loadingMore}
                  className="px-4 py-2 text-sm font-medium transition-colors bg-white border rounded-lg text-violet-700 border-violet-300 hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  ← Trước
                </button>

                <div className="flex gap-2">
                  {renderPagination()}
                </div>

                <button
                  onClick={() => handlePageChange(Math.min(totalPages, page + 1))}
                  disabled={page >= totalPages || loadingMore}
                  className="px-4 py-2 text-sm font-medium transition-colors bg-white border rounded-lg text-violet-700 border-violet-300 hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Sau →
                </button>
              </div>
            )}

            {!hasMore && products.length > 0 && (
              <p className="py-10 text-sm text-center text-gray-500">
                Đã hiển thị tất cả sản phẩm
              </p>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default SubCategoryPage;