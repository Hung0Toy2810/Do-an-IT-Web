// src/hooks/useCategoryMutations.ts
import {
  useMutation,
  QueryClient,
  UseMutationResult,
} from '@tanstack/react-query';
import { notify } from '../utils/notify';
import { api } from '../services/productApi';
import {
  UpdateCategoryDto,
  UpdateSubCategoryDto,
  CategoryDto,
  SubCategoryDto,
  CreateSubCategoryDto,
} from '../types/product.types';

// === XỬ LÝ LỖI ===
const handleError = (err: unknown) => {
  if (err instanceof Error) {
    notify.error(err.message);
  } else {
    notify.error('Có lỗi xảy ra');
  }
};

// === CALLBACKS ===
interface UseCategoryMutationsCallbacks {
  onCreateCatSuccess?: () => void;
  onUpdateCatSuccess?: () => void;
  onCreateSubSuccess?: () => void;
  onUpdateSubSuccess?: () => void;
}

// === ĐỊNH NGHĨA KIỂU DỮ LIỆU ĐẦU VÀO ===
interface CreateCategoryVariables {
  name: string;
}
interface UpdateCategoryVariables {
  id: number;
  data: UpdateCategoryDto;
}
interface DeleteCategoryVariables {
  id: number;
}
interface CreateSubCategoryVariables {
  categoryId: number;
  name: string;
}
interface UpdateSubCategoryVariables {
  id: number;
  data: UpdateSubCategoryDto;
}
interface DeleteSubCategoryVariables {
  id: number;
}

// === KIỂU TRẢ VỀ CỦA HOOK ===
export interface UseCategoryMutationsReturn {
  createCat: UseMutationResult<CategoryDto, Error, CreateCategoryVariables>;
  updateCat: UseMutationResult<CategoryDto, Error, UpdateCategoryVariables>;
  deleteCat: UseMutationResult<void, Error, DeleteCategoryVariables>;
  createSub: UseMutationResult<SubCategoryDto, Error, CreateSubCategoryVariables>;
  updateSub: UseMutationResult<SubCategoryDto, Error, UpdateSubCategoryVariables>;
  deleteSub: UseMutationResult<void, Error, DeleteSubCategoryVariables>;
}

// === HOOK CHÍNH ===
export const useCategoryMutations = (
  queryClient: QueryClient,
  callbacks: UseCategoryMutationsCallbacks = {}
): UseCategoryMutationsReturn => {
  const createCat = useMutation<
    CategoryDto,
    Error,
    CreateCategoryVariables
  >({
    mutationFn: api.createCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      notify.success('Tạo danh mục thành công');
      callbacks.onCreateCatSuccess?.();
    },
    onError: handleError,
  });

  const updateCat = useMutation<
    CategoryDto,
    Error,
    UpdateCategoryVariables
  >({
    mutationFn: ({ id, data }) => api.updateCategory(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      notify.success('Cập nhật danh mục thành công');
      callbacks.onUpdateCatSuccess?.();
    },
    onError: handleError,
  });

  // SỬA: LẤY .id từ variables
  const deleteCat = useMutation<void, Error, DeleteCategoryVariables>({
    mutationFn: (variables) => api.deleteCategory(variables.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      notify.success('Xóa danh mục thành công');
    },
    onError: handleError,
  });

  const createSub = useMutation<
    SubCategoryDto,
    Error,
    CreateSubCategoryVariables
  >({
    mutationFn: (variables) => api.createSubCategory(variables as CreateSubCategoryDto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      notify.success('Tạo danh mục con thành công');
      callbacks.onCreateSubSuccess?.();
    },
    onError: handleError,
  });

  const updateSub = useMutation<
    SubCategoryDto,
    Error,
    UpdateSubCategoryVariables
  >({
    mutationFn: ({ id, data }) => api.updateSubCategory(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      notify.success('Cập nhật danh mục con thành công');
      callbacks.onUpdateSubSuccess?.();
    },
    onError: handleError,
  });

  // SỬA: LẤY .id từ variables
  const deleteSub = useMutation<void, Error, DeleteSubCategoryVariables>({
    mutationFn: (variables) => api.deleteSubCategory(variables.id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      notify.success('Xóa danh mục con thành công');
    },
    onError: handleError,
  });

  return {
    createCat,
    updateCat,
    deleteCat,
    createSub,
    updateSub,
    deleteSub,
  };
};