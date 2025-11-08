// src/components/product/ProductTableRow.tsx
'use client';

import { useState } from 'react';
import { Trash2, Edit2, Package } from 'lucide-react';
import { ProductCardDto } from '../../types/product.types';
import { EditProductPriceModal } from './EditProductPriceModal';

interface ProductTableRowProps {
  product: ProductCardDto;
  onEdit: (product: ProductCardDto) => void;
  onDelete: (id: number) => void;
  onRefresh?: () => void;
}

export const ProductTableRow = ({ product, onEdit, onDelete, onRefresh }: ProductTableRowProps) => {
  const [editModalOpen, setEditModalOpen] = useState(false);

  const handleEdit = (product: ProductCardDto) => {
    setEditModalOpen(true);
    onEdit(product);
  };

  const handleEditSuccess = () => {
    setEditModalOpen(false);
    onRefresh?.();
  };

  return (
    <>
      <tr className="transition-colors border-b hover:bg-gray-50">
        <td className="px-4 py-3 font-mono text-xs text-gray-600">#{product.id}</td>

        <td className="max-w-md px-4 py-3">
          <div className="flex items-center gap-3">
            {product.firstImage ? (
              <img
                src={product.firstImage}
                alt={product.name}
                className="object-cover w-10 h-10 border rounded"
              />
            ) : (
              <div className="flex items-center justify-center w-10 h-10 bg-gray-100 border rounded">
                <Package className="w-5 h-5 text-gray-400" />
              </div>
            )}
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium line-clamp-1">{product.name}</p>
              <p className="text-xs text-gray-500">{product.brand}</p>
            </div>
          </div>
        </td>

        <td className="px-4 py-3">
          <span
            className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
              product.isDiscontinued
                ? 'bg-gray-100 text-gray-700'
                : 'bg-green-100 text-green-700'
            }`}
          >
            {product.isDiscontinued ? 'Ngừng KD' : 'Đang bán'}
          </span>
        </td>

        <td className="px-4 py-3">
          <div className="flex items-center gap-2">
            <button
              onClick={() => handleEdit(product)}
              className="p-1.5 rounded hover:bg-blue-50 transition-colors"
              title="Cập nhật giá & hình ảnh"
            >
              <Edit2 className="w-4 h-4 text-blue-600" />
            </button>
            <button
              onClick={() => onDelete(product.id)}
              className="p-1.5 rounded hover:bg-red-50 transition-colors"
              title="Xóa sản phẩm"
            >
              <Trash2 className="w-4 h-4 text-red-600" />
            </button>
          </div>
        </td>
      </tr>

      <EditProductPriceModal
        isOpen={editModalOpen}
        onClose={() => setEditModalOpen(false)}
        product={product}
        onSuccess={handleEditSuccess}
      />
    </>
  );
};