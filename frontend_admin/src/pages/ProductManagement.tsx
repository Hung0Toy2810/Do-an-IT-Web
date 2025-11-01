// src/pages/ProductManagement.tsx
'use client';

import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Loader2, Plus, Package } from 'lucide-react';
import { notify } from '../utils/notify';
import { api } from '../services/productApi';
import { CategoryDto, SubCategoryDto } from '../types/product.types';
import { CategoryList } from '../components/product/CategoryList';
import { ProductsSection } from '../components/product/ProductsSection';
import { CategoryModals } from '../components/product/CategoryModals';
import { useCategoryMutations } from '../hooks/useCategoryMutations';

export default function ProductManagement() {
  const queryClient = useQueryClient();
  const [selectedSub, setSelectedSub] = useState<SubCategoryDto | null>(null);
  const [expandedCats, setExpandedCats] = useState<Set<number>>(new Set());

  // Filter states
  const [selectedBrand, setSelectedBrand] = useState<string>('');
  const [priceSort, setPriceSort] = useState<'asc' | 'desc' | null>(null);

  // Modal States
  const [showCreateCat, setShowCreateCat] = useState(false);
  const [showEditCat, setShowEditCat] = useState<CategoryDto | null>(null);
  const [showCreateSub, setShowCreateSub] = useState<CategoryDto | null>(null);
  const [showEditSub, setShowEditSub] = useState<SubCategoryDto | null>(null);
  const [showAddProduct, setShowAddProduct] = useState(false);

  // Form Inputs
  const [catName, setCatName] = useState('');
  const [subName, setSubName] = useState('');

  // Queries
  const { data: categoriesResponse, isLoading, error } = useQuery<{ data: CategoryDto[] }, Error>({
    queryKey: ['categories'],
    queryFn: api.getCategories,
  });

  const categories = categoriesResponse?.data;

  // Mutations
  const mutations = useCategoryMutations(queryClient, {
    onCreateCatSuccess: () => {
      setShowCreateCat(false);
      setCatName('');
    },
    onUpdateCatSuccess: () => {
      setShowEditCat(null);
      setCatName('');
    },
    onCreateSubSuccess: () => {
      setShowCreateSub(null);
      setSubName('');
    },
    onUpdateSubSuccess: () => {
      setShowEditSub(null);
      setSubName('');
    },
  });

  const toggleExpand = (catId: number) => {
    const newSet = new Set(expandedCats);
    if (newSet.has(catId)) newSet.delete(catId);
    else newSet.add(catId);
    setExpandedCats(newSet);
  };

  const handleSelectSubCategory = (sub: SubCategoryDto) => {
    setSelectedSub(sub);
    setSelectedBrand('');
    setPriceSort(null);
  };

  return (
    <>
      <div className="space-y-6">
        {/* Header */}
        <div className="p-6 bg-white border shadow-lg border-violet-100/50 rounded-2xl">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="mb-2 text-2xl font-bold text-transparent lg:text-3xl bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text">
                Quản lý sản phẩm
              </h1>
              <p className="text-sm font-medium text-gray-600">
                Quản lý danh mục, sản phẩm, tồn kho
              </p>
            </div>
            <button
              onClick={() => {
                setCatName('');
                setShowCreateCat(true);
              }}
              className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white rounded-xl bg-gradient-to-r from-violet-600 to-violet-700 hover:shadow-lg"
            >
              <Plus className="w-4 h-4" /> Thêm danh mục
            </button>
          </div>
        </div>

        {/* Main Content */}
        <div className="p-8 bg-white border shadow-lg lg:p-12 border-violet-100/50 rounded-2xl">
          <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
            {/* Categories */}
            <div className="lg:col-span-1">
              <h3 className="mb-4 text-lg font-semibold text-gray-900">Danh mục</h3>

              {isLoading ? (
                <div className="flex justify-center py-8">
                  <Loader2 className="w-6 h-6 animate-spin text-violet-600" />
                </div>
              ) : error ? (
                <div className="p-4 text-sm text-red-600 rounded-lg bg-red-50">
                  Lỗi: {error.message}
                </div>
              ) : categories?.length ? (
                <CategoryList
                  categories={categories}
                  expandedCats={expandedCats}
                  selectedSub={selectedSub}
                  onToggleExpand={toggleExpand}
                  onSelectSub={handleSelectSubCategory}
                  onEditCategory={(cat) => {
                    setCatName(cat.name);
                    setShowEditCat(cat);
                  }}
                  onDeleteCategory={(id) => mutations.deleteCat.mutate({ id })}
                  onCreateSubCategory={(cat) => {
                    setSubName('');
                    setShowCreateSub(cat);
                  }}
                  onEditSubCategory={(sub) => {
                    setSubName(sub.name);
                    setShowEditSub(sub);
                  }}
                  onDeleteSubCategory={(id) => mutations.deleteSub.mutate({ id })}
                />
              ) : (
                <div className="py-12 text-center text-gray-500">
                  <Package className="w-16 h-16 mx-auto mb-4 text-gray-300" />
                  <p>Chưa có danh mục nào</p>
                </div>
              )}
            </div>

            {/* Products */}
            <ProductsSection
              selectedSub={selectedSub}
              selectedBrand={selectedBrand}
              priceSort={priceSort}
              onBrandChange={setSelectedBrand}
              onPriceSortChange={setPriceSort}
              onAddProduct={() => setShowAddProduct(true)}
            />
          </div>
        </div>
      </div>

      {/* Modals – ĐÃ TRUYỀN ĐỦ PROPS */}
      <CategoryModals
        showCreateCat={showCreateCat}
        showEditCat={showEditCat}
        showCreateSub={showCreateSub}
        showEditSub={showEditSub}
        showAddProduct={showAddProduct}
        selectedSub={selectedSub} // TRUYỀN
        setShowAddProduct={setShowAddProduct} // TRUYỀN
        queryClient={queryClient} // TRUYỀN

        catName={catName}
        subName={subName}
        selectedSubSlug={selectedSub?.slug || ''}

        onCatNameChange={setCatName}
        onSubNameChange={setSubName}
        onCloseCreateCat={() => setShowCreateCat(false)}
        onCloseEditCat={() => setShowEditCat(null)}
        onCloseCreateSub={() => setShowCreateSub(null)}
        onCloseEditSub={() => setShowEditSub(null)}
        onCloseAddProduct={() => setShowAddProduct(false)}

        onCreateCat={() => {
          const name = catName.trim();
          if (!name) {
            notify.warning('Vui lòng nhập tên danh mục');
            return;
          }
          mutations.createCat.mutate({ name });
        }}
        onUpdateCat={() => {
          const name = catName.trim();
          if (!name || !showEditCat) return;
          mutations.updateCat.mutate({ id: showEditCat.id, data: { name } });
        }}
        onCreateSub={() => {
          const name = subName.trim();
          if (!name || !showCreateSub) {
            notify.warning('Vui lòng nhập tên danh mục con');
            return;
          }
          mutations.createSub.mutate({ name, categoryId: showCreateSub.id });
        }}
        onUpdateSub={() => {
          const name = subName.trim();
          if (!name || !showEditSub) return;
          mutations.updateSub.mutate({
            id: showEditSub.id,
            data: { name, categoryId: showEditSub.categoryId },
          });
        }}

        mutations={mutations}
      />
    </>
  );
}