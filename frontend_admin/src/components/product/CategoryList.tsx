// src/components/product/CategoryList.tsx

import { ChevronDown, Edit2, Trash2 } from 'lucide-react';
import { CategoryDto, SubCategoryDto } from '../../types/product.types';

interface CategoryListProps {
  categories: CategoryDto[];
  expandedCats: Set<number>;
  selectedSub: SubCategoryDto | null;
  onToggleExpand: (catId: number) => void;
  onSelectSub: (sub: SubCategoryDto) => void;
  onEditCategory: (cat: CategoryDto) => void;
  onDeleteCategory: (id: number) => void;
  onCreateSubCategory: (cat: CategoryDto) => void;
  onEditSubCategory: (sub: SubCategoryDto) => void;
  onDeleteSubCategory: (id: number) => void;
}

export const CategoryList = ({
  categories,
  expandedCats,
  selectedSub,
  onToggleExpand,
  onSelectSub,
  onEditCategory,
  onDeleteCategory,
  onCreateSubCategory,
  onEditSubCategory,
  onDeleteSubCategory,
}: CategoryListProps) => {
  return (
    <div className="space-y-3">
      {categories.map((cat) => (
        <div key={cat.id} className="overflow-hidden border rounded-lg">
          <div className="flex items-center justify-between px-4 py-3 bg-gradient-to-r from-violet-50 to-purple-50">
            <div className="flex items-center gap-2">
              <button
                onClick={() => onToggleExpand(cat.id)}
                className="p-1 rounded hover:bg-white/50"
              >
                <ChevronDown 
                  className={`w-4 h-4 transition-transform ${
                    expandedCats.has(cat.id) ? 'rotate-180' : ''
                  }`} 
                />
              </button>
              <span className="font-medium text-violet-900">{cat.name}</span>
            </div>
            <div className="flex items-center gap-1">
              <button
                onClick={() => onEditCategory(cat)}
                className="p-1 rounded hover:bg-white/50"
              >
                <Edit2 className="w-4 h-4 text-violet-700" />
              </button>
              <button
                onClick={() => {
                  if (confirm(`Xóa danh mục "${cat.name}"?`)) {
                    onDeleteCategory(cat.id);
                  }
                }}
                className="p-1 rounded hover:bg-white/50"
              >
                <Trash2 className="w-4 h-4 text-red-600" />
              </button>
            </div>
          </div>

          {expandedCats.has(cat.id) && (
            <div className="border-t">
              {cat.subCategories.length > 0 ? (
                <div className="divide-y">
                  {cat.subCategories.map((sub) => (
                    <div
                      key={sub.id}
                      className={`flex items-center justify-between px-4 py-3 text-sm ${
                        selectedSub?.id === sub.id ? 'bg-violet-100' : 'hover:bg-gray-50'
                      }`}
                    >
                      <button
                        onClick={() => onSelectSub(sub)}
                        className="flex-1 text-left"
                      >
                        {sub.name}
                      </button>
                      <div className="flex items-center gap-1">
                        <button
                          onClick={() => onEditSubCategory(sub)}
                          className="p-1 rounded hover:bg-white/50"
                        >
                          <Edit2 className="w-3.5 h-3.5 text-violet-600" />
                        </button>
                        <button
                          onClick={() => {
                            if (confirm(`Xóa "${sub.name}"?`)) {
                              onDeleteSubCategory(sub.id);
                            }
                          }}
                          className="p-1 rounded hover:bg-white/50"
                        >
                          <Trash2 className="w-3.5 h-3.5 text-red-600" />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="px-4 py-3 text-sm italic text-gray-500">
                  Chưa có danh mục con
                </div>
              )}
              <button
                onClick={() => onCreateSubCategory(cat)}
                className="w-full px-4 py-2 text-sm font-medium border-t text-violet-600 hover:bg-violet-50"
              >
                + Thêm danh mục con
              </button>
            </div>
          )}
        </div>
      ))}
    </div>
  );
};