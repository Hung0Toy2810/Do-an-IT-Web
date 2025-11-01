// src/components/product/ProductsSection.tsx

import { useQuery } from '@tanstack/react-query';
import { Loader2, Plus, Package } from 'lucide-react';
import { api, authFetch } from '../../services/productApi';
import { SubCategoryDto, ProductCardDto } from '../../types/product.types';
import { ProductFilters } from './ProductFilters';
import { ProductCard } from './ProductCard';

const API_BASE = 'http://localhost:5067';

interface ProductsSectionProps {
  selectedSub: SubCategoryDto | null;
  selectedBrand: string;
  priceSort: 'asc' | 'desc' | null;
  onBrandChange: (brand: string) => void;
  onPriceSortChange: (sort: 'asc' | 'desc' | null) => void;
  onAddProduct: () => void;
}

export const ProductsSection = ({
  selectedSub,
  selectedBrand,
  priceSort,
  onBrandChange,
  onPriceSortChange,
  onAddProduct,
}: ProductsSectionProps) => {
  // Products query
  const { data: productsResponse, isLoading: productsLoading } = useQuery({
    queryKey: ['products', selectedSub?.slug, selectedBrand, priceSort],
    queryFn: () => {
      if (!selectedSub) return Promise.resolve({ data: { productIds: [] } });
      return api.getProductsBySubCategory(selectedSub.slug, {
        brand: selectedBrand || undefined,
        sortByPriceAscending: priceSort !== null ? priceSort === 'asc' : undefined,
      });
    },
    enabled: !!selectedSub,
  });

  // Brands query
  const { data: brandsResponse } = useQuery({
    queryKey: ['brands', selectedSub?.slug],
    queryFn: () => {
      if (!selectedSub) return Promise.resolve({ data: { brands: [] } });
      return api.getBrandsBySubCategory(selectedSub.slug);
    },
    enabled: !!selectedSub,
  });

  const productIds = productsResponse?.data?.productIds || [];
  const brands = brandsResponse?.data?.brands || [];

  // Product cards query
  const { data: productCardsResponse } = useQuery<ProductCardDto[], Error>({
  queryKey: ['product-cards', productIds],
  queryFn: async () => {
    if (productIds.length === 0) return [];
    const cards = await Promise.all(
      productIds.map((id: number) =>
        authFetch(`${API_BASE}/api/products/card/${id}`)
          .then(res => res.data)
          .catch(() => null)
      )
    );
    return cards.filter(Boolean) as ProductCardDto[];
  },
  enabled: productIds.length > 0,
});

  const productCards = productCardsResponse || [];

  return (
    <div className="lg:col-span-2">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Sản phẩm</h3>
        {selectedSub && (
          <button
            onClick={onAddProduct}
            className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white rounded-lg bg-gradient-to-r from-violet-600 to-violet-700 hover:shadow-lg"
          >
            <Plus className="w-4 h-4" /> Thêm sản phẩm
          </button>
        )}
      </div>

      {selectedSub ? (
        <>
          <ProductFilters
            brands={brands}
            selectedBrand={selectedBrand}
            priceSort={priceSort}
            onBrandChange={onBrandChange}
            onPriceSortChange={onPriceSortChange}
          />

          {productsLoading ? (
            <div className="flex justify-center py-12">
              <Loader2 className="w-8 h-8 animate-spin text-violet-600" />
            </div>
          ) : productCards.length > 0 ? (
            <>
              <div className="mb-3 text-sm text-gray-600">
                Tìm thấy <strong>{productCards.length}</strong> sản phẩm
              </div>
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
                {productCards.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </div>
            </>
          ) : (
            <div className="p-12 text-center border-2 border-dashed bg-gray-50 rounded-xl">
              <Package className="w-16 h-16 mx-auto mb-4 text-gray-400" />
              <p className="font-medium text-gray-600">
                Không tìm thấy sản phẩm nào
              </p>
              <p className="mt-2 text-sm text-gray-500">
                {selectedBrand || priceSort 
                  ? 'Thử điều chỉnh bộ lọc' 
                  : 'Thêm sản phẩm mới cho danh mục này'
                }
              </p>
            </div>
          )}
        </>
      ) : (
        <div className="p-12 text-center border-2 border-dashed bg-gray-50 rounded-xl">
          <Package className="w-16 h-16 mx-auto mb-4 text-gray-400" />
          <p className="font-medium text-gray-600">Chọn một danh mục con để xem sản phẩm</p>
        </div>
      )}
    </div>
  );
};