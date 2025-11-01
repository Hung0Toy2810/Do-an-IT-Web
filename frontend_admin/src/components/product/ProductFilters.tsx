// src/components/product/ProductFilters.tsx

import { Filter, SortAsc, SortDesc } from 'lucide-react';

interface ProductFiltersProps {
  brands: string[];
  selectedBrand: string;
  priceSort: 'asc' | 'desc' | null;
  onBrandChange: (brand: string) => void;
  onPriceSortChange: (sort: 'asc' | 'desc' | null) => void;
}

export const ProductFilters = ({
  brands,
  selectedBrand,
  priceSort,
  onBrandChange,
  onPriceSortChange,
}: ProductFiltersProps) => {
  const handleClearFilters = () => {
    onBrandChange('');
    onPriceSortChange(null);
  };

  return (
    <div className="flex flex-wrap gap-3 p-4 mb-4 border rounded-lg bg-gray-50">
      <div className="flex items-center gap-2">
        <Filter className="w-4 h-4 text-gray-600" />
        <span className="text-sm font-medium text-gray-700">Lọc:</span>
      </div>
      
      {/* Brand filter */}
      <select
        value={selectedBrand}
        onChange={(e) => onBrandChange(e.target.value)}
        className="px-3 py-1 text-sm border rounded-lg focus:ring-2 focus:ring-violet-500"
      >
        <option value="">Tất cả hãng</option>
        {brands.map((brand) => (
          <option key={brand} value={brand}>{brand}</option>
        ))}
      </select>

      {/* Price sort */}
      <div className="flex gap-2">
        <button
          onClick={() => onPriceSortChange(priceSort === 'asc' ? null : 'asc')}
          className={`flex items-center gap-1 px-3 py-1 text-sm border rounded-lg ${
            priceSort === 'asc' ? 'bg-violet-100 border-violet-300' : 'hover:bg-gray-100'
          }`}
        >
          <SortAsc className="w-4 h-4" /> Giá tăng
        </button>
        <button
          onClick={() => onPriceSortChange(priceSort === 'desc' ? null : 'desc')}
          className={`flex items-center gap-1 px-3 py-1 text-sm border rounded-lg ${
            priceSort === 'desc' ? 'bg-violet-100 border-violet-300' : 'hover:bg-gray-100'
          }`}
        >
          <SortDesc className="w-4 h-4" /> Giá giảm
        </button>
      </div>

      {/* Clear filters */}
      {(selectedBrand || priceSort) && (
        <button
          onClick={handleClearFilters}
          className="px-3 py-1 text-sm text-red-600 border border-red-300 rounded-lg hover:bg-red-50"
        >
          Xóa bộ lọc
        </button>
      )}
    </div>
  );
};