// src/components/product/EditProductPriceModal.tsx
'use client';
import { useState, useEffect, useRef } from 'react';
import { DollarSign, TrendingDown, Power, Upload, X, ImageIcon } from 'lucide-react';
import { Modal } from '../ui/Modal';
import { notify } from '../../utils/notify';
import { api } from '../../services/productApi';
import { ProductCardDto } from '../../types/product.types';

interface VariantPrice {
  slug: string;
  attributes: Record<string, string>;
  originalPrice: number;
  discountedPrice: number;
  images: string[];
  newImages: File[];
}

interface EditProductPriceModalProps {
  isOpen: boolean;
  onClose: () => void;
  product: ProductCardDto | null;
  onSuccess?: () => void;
}

export const EditProductPriceModal = ({
  isOpen,
  onClose,
  product,
  onSuccess,
}: EditProductPriceModalProps) => {
  const [variants, setVariants] = useState<VariantPrice[]>([]);
  const [isDiscontinued, setIsDiscontinued] = useState(false);
  const [loading, setLoading] = useState(false);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const fileInputRefs = useRef<Record<number, HTMLInputElement | null>>({});

  useEffect(() => {
    if (isOpen && product) {
      loadProductDetail();
    } else if (!isOpen) {
      resetForm();
    }
  }, [isOpen, product]);

  const resetForm = () => {
    setVariants([]);
    setIsDiscontinued(false);
    setErrors([]);
  };

  const loadProductDetail = async () => {
    if (!product) return;
    
    try {
      setLoadingDetail(true);
      const response = await api.getProductDetailById(product.id);
      const detail = response.data;
      
      const variantPrices: VariantPrice[] = detail.variants.map((v: any) => ({
        slug: v.slug,
        attributes: v.attributes,
        originalPrice: v.originalPrice,
        discountedPrice: v.discountedPrice,
        images: v.images || [],
        newImages: [],
      }));
      
      setVariants(variantPrices);
      setIsDiscontinued(detail.isDiscontinued);
      
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Lỗi khi tải chi tiết sản phẩm';
      notify.error(message);
      onClose();
    } finally {
      setLoadingDetail(false);
    }
  };

  const updatePrice = (index: number, field: 'originalPrice' | 'discountedPrice', value: number) => {
    setVariants(prev => prev.map((v, i) => 
      i === index ? { ...v, [field]: value } : v
    ));
  };

  const handleImageSelect = (variantIndex: number, files: FileList | null) => {
    if (!files || files.length === 0) return;

    const imageFiles = Array.from(files);

    // Validate file types
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    const invalidFiles = imageFiles.filter(f => !validTypes.includes(f.type));
    
    if (invalidFiles.length > 0) {
      notify.error('Chỉ chấp nhận file ảnh (JPG, PNG, WEBP)');
      return;
    }

    // Validate file size (max 5MB per file)
    const maxSize = 5 * 1024 * 1024;
    const oversizedFiles = imageFiles.filter(f => f.size > maxSize);
    
    if (oversizedFiles.length > 0) {
      notify.error('Kích thước file không được vượt quá 5MB');
      return;
    }

    // Add to newImages array
    setVariants(prev => prev.map((v, i) => 
      i === variantIndex ? { ...v, newImages: [...v.newImages, ...imageFiles] } : v
    ));

    // Clear file input
    if (fileInputRefs.current[variantIndex]) {
      fileInputRefs.current[variantIndex]!.value = '';
    }
  };

  const removeExistingImage = (variantIndex: number, imageIndex: number) => {
    setVariants(prev => prev.map((v, i) => 
      i === variantIndex 
        ? { ...v, images: v.images.filter((_, idx) => idx !== imageIndex) }
        : v
    ));
  };

  const removeNewImage = (variantIndex: number, imageIndex: number) => {
    setVariants(prev => prev.map((v, i) => 
      i === variantIndex 
        ? { ...v, newImages: v.newImages.filter((_, idx) => idx !== imageIndex) }
        : v
    ));
  };

  const handleSubmit = async () => {
    if (!product) return;
    
    setErrors([]);

    // Validate giá
    const priceErrors = variants.flatMap((v, i) => {
      const errs: string[] = [];
      if (v.originalPrice <= 0) errs.push(`Variant ${i + 1}: Giá gốc phải > 0`);
      if (v.discountedPrice <= 0) errs.push(`Variant ${i + 1}: Giá giảm phải > 0`);
      if (v.discountedPrice > v.originalPrice) errs.push(`Variant ${i + 1}: Giá giảm phải ≤ giá gốc`);
      return errs;
    });

    if (priceErrors.length) {
      setErrors(priceErrors);
      return;
    }

    try {
      setLoading(true);

      // 1. Upload ảnh cho các variants có ảnh mới
      for (let i = 0; i < variants.length; i++) {
        const variant = variants[i];
        if (variant.newImages.length > 0) {
          try {
            await api.updateVariantImages(product.slug, variant.slug, variant.newImages);
          } catch (err) {
            throw new Error(`Lỗi upload ảnh cho variant ${i + 1}: ${err instanceof Error ? err.message : 'Unknown error'}`);
          }
        }
      }

      // 2. Cập nhật giá hàng loạt
      const priceUpdates = variants.map(v => ({
        productSlug: product.slug,
        variantSlug: v.slug,
        originalPrice: v.originalPrice,
        discountedPrice: v.discountedPrice,
      }));

      await api.bulkUpdatePrices(priceUpdates);

      // 3. Cập nhật trạng thái discontinued
      await api.updateIsDiscontinued(product.slug, isDiscontinued);

      notify.success('Cập nhật thành công!');
      onSuccess?.();
      onClose();
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Lỗi server';
      setErrors([message]);
      notify.error(message);
    } finally {
      setLoading(false);
    }
  };

  if (!product) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Cập nhật Giá & Hình ảnh" size="xl">
      <div className="space-y-5 text-sm">
        {loadingDetail ? (
          <div className="flex items-center justify-center py-12">
            <div className="text-gray-500">Đang tải thông tin...</div>
          </div>
        ) : (
          <>
            {/* Thông tin sản phẩm */}
            <div className="p-3 border rounded-lg bg-gray-50">
              <div className="flex items-center gap-3">
                {product.firstImage && (
                  <img 
                    src={product.firstImage} 
                    alt={product.name}
                    className="object-cover w-12 h-12 border rounded"
                  />
                )}
                <div className="flex-1">
                  <p className="font-medium">{product.name}</p>
                  <p className="text-xs text-gray-500">{product.brand}</p>
                </div>
              </div>
            </div>

            {/* Trạng thái ngừng kinh doanh */}
            <div className="p-3 border rounded-lg">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Power className="w-4 h-4 text-gray-600" />
                  <span className="font-medium">Trạng thái kinh doanh</span>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={isDiscontinued}
                    onChange={(e) => setIsDiscontinued(e.target.checked)}
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-violet-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-red-500"></div>
                  <span className="ml-3 text-sm">
                    {isDiscontinued ? 'Ngừng kinh doanh' : 'Đang bán'}
                  </span>
                </label>
              </div>
            </div>

            {/* Danh sách variants - SCROLLABLE */}
            <div className="max-h-[50vh] overflow-y-auto pr-2 -mr-2">
              <div className="flex items-center gap-2 mb-3">
                <DollarSign className="w-4 h-4 text-gray-600" />
                <span className="font-medium">Giá & Hình ảnh các biến thể</span>
              </div>
              
              {variants.length > 0 ? (
                <div className="space-y-4">
                  {variants.map((variant, index) => {
                    const discount = variant.originalPrice - variant.discountedPrice;
                    const discountPercent = variant.originalPrice > 0 
                      ? Math.round((discount / variant.originalPrice) * 100)
                      : 0;
                    
                    const totalImages = variant.images.length + variant.newImages.length;
                    
                    return (
                      <div key={variant.slug} className="p-4 border rounded-lg bg-gray-50">
                        {/* Variant header */}
                        <div className="flex flex-wrap gap-1 mb-3">
                          {Object.entries(variant.attributes).map(([key, value]) => (
                            <span key={key} className="px-2 py-1 text-xs font-medium text-blue-700 bg-blue-100 rounded">
                              {key}: {value}
                            </span>
                          ))}
                        </div>

                        {/* Giá */}
                        <div className="grid grid-cols-3 gap-3 mb-3">
                          <div>
                            <label className="block mb-1 text-xs font-medium text-gray-600">
                              Giá gốc
                            </label>
                            <input
                              type="number"
                              value={variant.originalPrice}
                              onChange={(e) => updatePrice(index, 'originalPrice', parseFloat(e.target.value) || 0)}
                              className="w-full px-3 py-2 text-right border rounded-lg focus:ring-2 focus:ring-violet-200"
                              min="0"
                              step="1000"
                            />
                          </div>
                          <div>
                            <label className="block mb-1 text-xs font-medium text-gray-600">
                              Giá giảm
                            </label>
                            <input
                              type="number"
                              value={variant.discountedPrice}
                              onChange={(e) => updatePrice(index, 'discountedPrice', parseFloat(e.target.value) || 0)}
                              className="w-full px-3 py-2 text-right border rounded-lg focus:ring-2 focus:ring-violet-200"
                              min="0"
                              step="1000"
                            />
                          </div>
                          <div>
                            <label className="block mb-1 text-xs font-medium text-gray-600">
                              Tiết kiệm
                            </label>
                            <div className="flex items-center justify-center h-10 px-3 py-2 border border-green-200 rounded-lg bg-green-50">
                              {discount > 0 ? (
                                <div className="flex items-center gap-1 font-medium text-green-600">
                                  <TrendingDown className="w-4 h-4" />
                                  <span>{discountPercent}%</span>
                                </div>
                              ) : (
                                <span className="text-gray-400">-</span>
                              )}
                            </div>
                          </div>
                        </div>

                        {/* Hình ảnh */}
                        <div>
                          <label className="block mb-2 text-xs font-medium text-gray-600">
                            Hình ảnh ({totalImages})
                          </label>
                          
                          {/* Grid preview images */}
                          <div className="grid grid-cols-5 gap-2 mb-2">
                            {/* Existing images */}
                            {variant.images.map((img, imgIdx) => (
                              <div key={`existing-${imgIdx}`} className="relative group">
                                <img 
                                  src={img} 
                                  alt={`Image ${imgIdx + 1}`}
                                  className="object-cover w-full border rounded aspect-square"
                                />
                                <button
                                  type="button"
                                  onClick={() => removeExistingImage(index, imgIdx)}
                                  className="absolute p-1 transition-opacity bg-red-500 rounded-full shadow-lg opacity-0 -top-1 -right-1 group-hover:opacity-100"
                                  title="Xóa ảnh"
                                >
                                  <X className="w-3 h-3 text-white" />
                                </button>
                              </div>
                            ))}

                            {/* New images (preview from File object) */}
                            {variant.newImages.map((file, imgIdx) => (
                              <div key={`new-${imgIdx}`} className="relative group">
                                <img 
                                  src={URL.createObjectURL(file)} 
                                  alt={`New ${imgIdx + 1}`}
                                  className="object-cover w-full border-2 border-green-500 rounded aspect-square"
                                />
                                <div className="absolute top-0 left-0 px-1 py-0.5 text-xs text-white bg-green-500 rounded-tl rounded-br">
                                  Mới
                                </div>
                                <button
                                  type="button"
                                  onClick={() => removeNewImage(index, imgIdx)}
                                  className="absolute p-1 transition-opacity bg-red-500 rounded-full shadow-lg opacity-0 -top-1 -right-1 group-hover:opacity-100"
                                  title="Xóa ảnh"
                                >
                                  <X className="w-3 h-3 text-white" />
                                </button>
                              </div>
                            ))}

                            {/* Upload placeholder */}
                            <label
                              htmlFor={`file-input-${index}`}
                              className="flex flex-col items-center justify-center w-full transition-colors border-2 border-gray-300 border-dashed rounded cursor-pointer aspect-square hover:border-violet-400 hover:bg-violet-50"
                            >
                              <Upload className="w-6 h-6 mb-1 text-gray-400" />
                              <span className="text-xs text-gray-500">Upload</span>
                            </label>
                          </div>

                          <input
                            ref={el => fileInputRefs.current[index] = el}
                            type="file"
                            accept="image/jpeg,image/jpg,image/png,image/webp"
                            multiple
                            onChange={(e) => handleImageSelect(index, e.target.files)}
                            className="hidden"
                            id={`file-input-${index}`}
                          />
                          
                          <p className="mt-1 text-xs text-gray-500">
                            JPG, PNG, WEBP • Tối đa 5MB/ảnh • Chọn nhiều ảnh cùng lúc
                          </p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center py-12 border rounded-lg">
                  <ImageIcon className="w-12 h-12 mb-2 text-gray-300" />
                  <p className="text-gray-400">Không có biến thể nào</p>
                </div>
              )}
            </div>

            {/* Errors */}
            {errors.length > 0 && (
              <div className="p-3 text-xs text-red-600 border border-red-200 rounded-lg bg-red-50">
                {errors.map((err, i) => (
                  <div key={i}>• {err}</div>
                ))}
              </div>
            )}

            {/* Actions */}
            <div className="flex gap-2 pt-2 border-t">
              <button
                type="button"
                onClick={onClose}
                disabled={loading || loadingDetail}
                className="flex-1 px-4 py-2 text-sm transition-colors border rounded-lg hover:bg-gray-50 disabled:opacity-50"
              >
                Hủy
              </button>
              <button
                type="button"
                onClick={handleSubmit}
                disabled={loading || loadingDetail || !variants.length}
                className="flex-1 px-4 py-2 text-sm text-white transition-colors rounded-lg bg-violet-600 hover:bg-violet-700 disabled:opacity-50"
              >
                {loading ? 'Đang lưu...' : 'Lưu thay đổi'}
              </button>
            </div>
          </>
        )}
      </div>
    </Modal>
  );
};