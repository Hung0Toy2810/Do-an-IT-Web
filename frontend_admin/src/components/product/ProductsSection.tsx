// src/components/product/ProductsSection.tsx
'use client';

import { useQuery } from '@tanstack/react-query';
import { Loader2, Plus, Package } from 'lucide-react';
import { api } from '../../services/productApi';
import { SubCategoryDto, ProductCardDto } from '../../types/product.types';
import { ProductTableRow } from './ProductTableRow';
import { ProductFilters } from './ProductFilters';
import { notify } from '../../utils/notify';

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
  // 1. Lấy danh sách ID + totalCount từ API search-all
  const { data: listResponse, isLoading: listLoading } = useQuery({
    queryKey: ['products-search-all', selectedSub?.slug, selectedBrand, priceSort],
    queryFn: async () => {
      if (!selectedSub) return null;

      return api.searchAll({
        subCategorySlug: selectedSub.slug,
        brand: selectedBrand || undefined,
        sortByPriceAscending:
          priceSort === 'asc' ? true : priceSort === 'desc' ? false : undefined,
      });
    },
    enabled: !!selectedSub,
  });

  const productIds: number[] = listResponse?.data?.productIds || [];
  const totalCount = listResponse?.data?.totalCount || 0;

  // 2. Lấy danh sách brands (vẫn dùng API cũ)
  const { data: brandsResponse } = useQuery({
    queryKey: ['brands', selectedSub?.slug],
    queryFn: () => api.getBrandsBySubCategory(selectedSub!.slug),
    enabled: !!selectedSub,
    select: (res) => res?.data?.brands || [],
  });

  const brands: string[] = brandsResponse || [];

  // 3. Lấy chi tiết từng sản phẩm theo ID
  const { data: productCards = [], isLoading: cardsLoading } = useQuery<ProductCardDto[]>({
    queryKey: ['product-cards', productIds],
    queryFn: async () => {
      if (productIds.length === 0) return [];

      const results = await Promise.all(
        productIds.map((id) =>
          api
            .getProductCard(id)
            .then((res) => res.data as ProductCardDto)
            .catch((err) => {
              console.warn(`Lỗi lấy sản phẩm ID ${id}:`, err);
              return null;
            })
        )
      );

      return results.filter((card): card is ProductCardDto => card !== null);
    },
    enabled: productIds.length > 0,
    staleTime: 1000 * 60, // cache 1 phút
  });

  if (!selectedSub) {
    return (
      <div className="flex flex-col items-center justify-center text-gray-500 h-96">
        <Package className="w-16 h-16 mb-4 text-gray-300" />
        <p>Chọn danh mục con để xem sản phẩm</p>
      </div>
    );
  }

  const isLoading = listLoading || cardsLoading;

  return (
    <div className="lg:col-span-2">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Sản phẩm</h3>
        <button
          onClick={onAddProduct}
          className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white transition-shadow rounded-lg bg-gradient-to-r from-violet-600 to-violet-700 hover:shadow-lg"
        >
          <Plus className="w-4 h-4" />
          Thêm sản phẩm
        </button>
      </div>

      <ProductFilters
        brands={brands}
        selectedBrand={selectedBrand}
        priceSort={priceSort}
        onBrandChange={onBrandChange}
        onPriceSortChange={onPriceSortChange}
      />

      {isLoading ? (
        <div className="flex justify-center py-12">
          <Loader2 className="w-8 h-8 animate-spin text-violet-600" />
        </div>
      ) : productCards.length > 0 ? (
        <>
          <div className="mb-3 text-sm text-gray-600">
            Tìm thấy <strong>{totalCount}</strong> sản phẩm
            {productCards.length < totalCount && (
              <span className="text-orange-600">
                {' '}
                (đang hiển thị {productCards.length})
              </span>
            )}
          </div>

          <div className="overflow-x-auto border rounded-lg shadow-sm">
            <table className="w-full">
              <thead className="border-b bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase">
                    ID
                  </th>
                  <th className="px-4 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase">
                    Sản phẩm
                  </th>
                  <th className="px-4 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase">
                    Trạng thái
                  </th>
                  <th className="px-4 py-3 text-xs font-medium tracking-wider text-left text-gray-500 uppercase">
                    Hành động
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {productCards.map((product) => (
                  <ProductTableRow
                    key={product.id}
                    product={product}
                    onEdit={() => notify.info('Chức năng sửa sẽ được thêm sau')}
                    onDelete={() => notify.warning('Chức năng xóa sản phẩm sẽ được thêm sau')}
                  />
                ))}
              </tbody>
            </table>
          </div>
        </>
      ) : (
        <div className="p-12 text-center border-2 border-dashed bg-gray-50 rounded-xl">
          <Package className="w-16 h-16 mx-auto mb-4 text-gray-400" />
          <p className="font-medium text-gray-600">Không tìm thấy sản phẩm</p>
          <p className="mt-2 text-sm text-gray-500">
            {selectedBrand || priceSort
              ? 'Thử điều chỉnh bộ lọc thương hiệu hoặc giá'
              : 'Hãy thêm sản phẩm đầu tiên cho danh mục này'}
          </p>
        </div>
      )}
    </div>
  );
};