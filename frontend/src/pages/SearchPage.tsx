// src/pages/SearchPage.tsx - HOÀN CHỈNH & CHẠY NGAY
import React, { useState, useEffect, useCallback } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Search, X, Loader2, Filter } from 'lucide-react';
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

const SearchPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();

  const [keyword, setKeyword] = useState(searchParams.get('q') || '');
  const [inputValue, setInputValue] = useState(keyword);
  const [minPrice, setMinPrice] = useState(searchParams.get('min') || '');
  const [maxPrice, setMaxPrice] = useState(searchParams.get('max') || '');
  const [sortAsc, setSortAsc] = useState<'' | 'true' | 'false'>(
    (searchParams.get('sort') as '' | 'true' | 'false') || ''
  );

  const [products, setProducts] = useState<ProductCardDto[]>([]);
  const [loading, setLoading] = useState(false);

  // === Build query ===
  const buildQuery = useCallback(() => {
    const params = new URLSearchParams();
    if (keyword) params.append('keyword', keyword);
    if (minPrice) params.append('minPrice', (parseFloat(minPrice) * 1000).toString());
    if (maxPrice) params.append('maxPrice', (parseFloat(maxPrice) * 1000).toString());
    if (sortAsc) params.append('sortByPriceAscending', sortAsc);
    return params.toString();
  }, [keyword, minPrice, maxPrice, sortAsc]);

  // === Gọi API ===
  const performSearch = useCallback(async () => {
    if (!keyword.trim() && !minPrice && !maxPrice) return;

    setLoading(true);
    setProducts([]);

    try {
      const query = buildQuery();
      const res = await fetch(`http://localhost:5067/api/products/search?${query}`);
      if (!res.ok) throw new Error('Server error');

      const json = await res.json();
      console.log('API Response:', json); // DEBUG

      const { ProductIds = [], TotalCount = 0 } = json.data || {};

      if (ProductIds.length === 0) {
        setProducts([]);
        return;
      }

      const details = await Promise.all(
        ProductIds.map((id: number) =>
          fetch(`http://localhost:5067/api/products/card/${id}`).then((r) => r.json())
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

      setProducts(newProducts);
    } catch (err) {
      notify('error', 'Không thể tải kết quả');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [buildQuery]);

  // === Submit ===
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = inputValue.trim();
    setKeyword(trimmed);

    const newParams = new URLSearchParams();
    if (trimmed) newParams.set('q', trimmed);
    if (minPrice) newParams.set('min', minPrice);
    if (maxPrice) newParams.set('max', maxPrice);
    if (sortAsc) newParams.set('sort', sortAsc);
    setSearchParams(newParams);

    performSearch();
  };

  // === ĐỒNG BỘ URL → GỌI NGAY ===
  useEffect(() => {
    const q = searchParams.get('q') || '';
    const min = searchParams.get('min') || '';
    const max = searchParams.get('max') || '';
    const sort = searchParams.get('sort') || '';

    if (q !== keyword || min !== minPrice || max !== maxPrice || sort !== sortAsc) {
      setKeyword(q);
      setInputValue(q);
      setMinPrice(min);
      setMaxPrice(max);
      setSortAsc(sort as any);
      performSearch(); // GỌI NGAY KHI URL CÓ q=sạc
    }
  }, [searchParams, performSearch]);

  const handleBuyNow = (product: ProductCardDto) => {
    navigate(`/product/${product.slug}`);
  };

  const clearFilters = () => {
    setMinPrice('');
    setMaxPrice('');
    setSortAsc('');
    setSearchParams(keyword ? { q: keyword } : {});
    performSearch();
  };

  return (
    <div className="min-h-screen py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
      <div className="container px-4 mx-auto max-w-7xl">
        <form onSubmit={handleSearch} className="mb-6">
          <div className="relative max-w-2xl mx-auto">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              placeholder="Tìm kiếm sản phẩm..."
              className="w-full px-5 py-4 pr-12 text-lg bg-white border shadow-md border-violet-200 rounded-2xl focus:outline-none focus:ring-2 focus:ring-violet-800/30"
            />
            {inputValue && (
              <button
                type="button"
                onClick={() => {
                  setInputValue('');
                  setKeyword('');
                  setSearchParams({});
                  setProducts([]);
                }}
                className="absolute p-2 text-gray-400 -translate-y-1/2 right-12 top-1/2 hover:text-gray-600"
              >
                <X className="w-5 h-5" />
              </button>
            )}
            <button type="submit" className="absolute p-3 text-white -translate-y-1/2 right-3 top-1/2 bg-violet-800 rounded-xl">
              <Search className="w-5 h-5" />
            </button>
          </div>
        </form>

        <div className="flex flex-wrap items-center gap-3 mb-6 text-sm">
          <Filter className="w-4 h-4 text-violet-700" />
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
            <option value="">Sắp xếp</option>
            <option value="true">Giá tăng</option>
            <option value="false">Giá giảm</option>
          </select>
          <button onClick={clearFilters} className="px-3 py-1 text-red-600 border border-red-300 rounded">
            Xóa
          </button>
          <button onClick={performSearch} className="px-4 py-1 text-white rounded bg-violet-800">
            Áp dụng
          </button>
        </div>

        {loading ? (
          <div className="flex justify-center py-16">
            <Loader2 className="w-10 h-10 text-violet-800 animate-spin" />
          </div>
        ) : products.length === 0 ? (
          <div className="py-16 text-center">
            <p className="text-lg text-gray-600">Không tìm thấy sản phẩm cho "{keyword}"</p>
          </div>
        ) : (
          <>
            <p className="mb-4 text-sm text-gray-600">
              Tìm thấy <strong className="text-violet-800">{products.length}</strong> sản phẩm
            </p>
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {products.map((product) => (
                <ProductCard
                  key={product.id}
                  product={{
                    id: product.id,
                    slug: product.slug,
                    name: product.name,
                    image: product.firstImage || '/placeholder.svg',
                    originalPrice: product.originalPriceOfMinVariant * 1000,
                    discountedPrice: product.minDiscountedPrice * 1000,
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
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default SearchPage;