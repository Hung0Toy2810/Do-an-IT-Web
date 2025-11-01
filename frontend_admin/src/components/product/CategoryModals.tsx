// src/components/product/CategoryModals.tsx
import { QueryClient } from '@tanstack/react-query';
import { Modal } from '../ui/Modal';
import { AddProductModal } from './AddProductModal';
import { CategoryDto, SubCategoryDto } from '../../types/product.types';
import { UseCategoryMutationsReturn } from '../../hooks/useCategoryMutations';
import { notify } from '../../utils/notify';

interface CategoryModalsProps {
  showCreateCat: boolean;
  showEditCat: CategoryDto | null;
  showCreateSub: CategoryDto | null;
  showEditSub: SubCategoryDto | null;
  showAddProduct: boolean;
  selectedSub: SubCategoryDto | null;
  setShowAddProduct: (open: boolean) => void;
  queryClient: QueryClient;

  catName: string;
  subName: string;
  selectedSubSlug: string;
  onCatNameChange: (name: string) => void;
  onSubNameChange: (name: string) => void;
  onCloseCreateCat: () => void;
  onCloseEditCat: () => void;
  onCloseCreateSub: () => void;
  onCloseEditSub: () => void;
  onCloseAddProduct: () => void;
  onCreateCat: () => void;
  onUpdateCat: () => void;
  onCreateSub: () => void;
  onUpdateSub: () => void;
  mutations: UseCategoryMutationsReturn;
}

export const CategoryModals = ({
  showCreateCat,
  showEditCat,
  showCreateSub,
  showEditSub,
  showAddProduct,
  selectedSub,
  setShowAddProduct,
  queryClient,
  catName,
  subName,
  onCatNameChange,
  onSubNameChange,
  onCloseCreateCat,
  onCloseEditCat,
  onCloseCreateSub,
  onCloseEditSub,
  onCreateCat,
  onUpdateCat,
  onCreateSub,
  onUpdateSub,
  mutations,
}: CategoryModalsProps) => {
  return (
    <>
      {/* Tạo danh mục */}
      <Modal isOpen={showCreateCat} onClose={onCloseCreateCat} title="Tạo danh mục mới">
        <input
          type="text"
          placeholder="Tên danh mục"
          value={catName}
          onChange={(e) => onCatNameChange(e.target.value)}
          className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-violet-500 focus:border-violet-500"
          autoFocus
        />
        <div className="flex gap-3 mt-4">
          <button onClick={onCloseCreateCat} className="flex-1 px-4 py-2 border rounded-lg hover:bg-gray-50">
            Hủy
          </button>
          <button
            onClick={onCreateCat}
            disabled={mutations.createCat.isPending}
            className="flex-1 px-4 py-2 text-white rounded-lg bg-gradient-to-r from-violet-600 to-violet-700 hover:shadow-lg disabled:opacity-70"
          >
            {mutations.createCat.isPending ? 'Đang tạo...' : 'Tạo'}
          </button>
        </div>
      </Modal>

      {/* Sửa danh mục */}
      <Modal isOpen={!!showEditCat} onClose={onCloseEditCat} title="Sửa danh mục">
        <input
          type="text"
          value={catName}
          onChange={(e) => onCatNameChange(e.target.value)}
          className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-violet-500"
        />
        <div className="flex gap-3 mt-4">
          <button onClick={onCloseEditCat} className="flex-1 px-4 py-2 border rounded-lg hover:bg-gray-50">
            Hủy
          </button>
          <button
            onClick={onUpdateCat}
            disabled={mutations.updateCat.isPending}
            className="flex-1 px-4 py-2 text-white rounded-lg bg-gradient-to-r from-violet-600 to-violet-700 hover:shadow-lg disabled:opacity-70"
          >
            {mutations.updateCat.isPending ? 'Đang lưu...' : 'Lưu'}
          </button>
        </div>
      </Modal>

      {/* Tạo danh mục con */}
      <Modal isOpen={!!showCreateSub} onClose={onCloseCreateSub} title="Tạo danh mục con">
        <input
          type="text"
          placeholder="Tên danh mục con"
          value={subName}
          onChange={(e) => onSubNameChange(e.target.value)}
          className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-violet-500"
        />
        <div className="flex gap-3 mt-4">
          <button onClick={onCloseCreateSub} className="flex-1 px-4 py-2 border rounded-lg hover:bg-gray-50">
            Hủy
          </button>
          <button
            onClick={onCreateSub}
            disabled={mutations.createSub.isPending}
            className="flex-1 px-4 py-2 text-white rounded-lg bg-gradient-to-r from-violet-600 to-violet-700 hover:shadow-lg disabled:opacity-70"
          >
            {mutations.createSub.isPending ? 'Đang tạo...' : 'Tạo'}
          </button>
        </div>
      </Modal>

      {/* Sửa danh mục con */}
      <Modal isOpen={!!showEditSub} onClose={onCloseEditSub} title="Sửa danh mục con">
        <input
          type="text"
          value={subName}
          onChange={(e) => onSubNameChange(e.target.value)}
          className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-violet-500"
        />
        <div className="flex gap-3 mt-4">
          <button onClick={onCloseEditSub} className="flex-1 px-4 py-2 border rounded-lg hover:bg-gray-50">
            Hủy
          </button>
          <button
            onClick={onUpdateSub}
            disabled={mutations.updateSub.isPending}
            className="flex-1 px-4 py-2 text-white rounded-lg bg-gradient-to-r from-violet-600 to-violet-700 hover:shadow-lg disabled:opacity-70"
          >
            {mutations.updateSub.isPending ? 'Đang lưu...' : 'Lưu'}
          </button>
        </div>
      </Modal>

      {/* Add Product Modal */}
      <AddProductModal
        isOpen={showAddProduct}
        onClose={() => setShowAddProduct(false)}
        subCategoryId={selectedSub?.id}
        onSuccess={() => {
          queryClient.invalidateQueries({ queryKey: ['products', selectedSub?.id] });
          notify.success('Tải lại danh sách sản phẩm');
        }}
      />
    </>
  );
};