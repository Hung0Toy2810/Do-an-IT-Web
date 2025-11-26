// src/components/product/EditProductPriceModal.tsx
'use client';
import { useState, useEffect, useRef } from 'react';
import {
  DollarSign,
  TrendingDown,
  Power,
  Upload,
  X,
  Package,
  Plus,
  Minus,
  ChevronRight,
} from 'lucide-react';
import { Modal } from '../ui/Modal';
import { notify } from '../../utils/notify';
import { api } from '../../services/productApi';
import { ProductCardDto } from '../../types/product.types';

const BACKEND_API_URL =
  process.env.NODE_ENV === 'production'
    ? 'https://your-production-domain.com'
    : 'http://localhost:5067';

interface VariantPrice {
  slug: string;
  attributes: Record<string, string>;
  originalPrice: number;
  discountedPrice: number;
  images: string[];
  newImages: File[];
}

interface ShipmentBatch {
  batchCode: string;
  importedQuantity: number;
  remainingQuantity: number;
  importPrice: number | null;
  importedAt: string;
  variantSlug: string;
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
  const inventoryRef = useRef<HTMLDivElement>(null);

  // Inventory
  const [selectedVariantIndex, setSelectedVariantIndex] = useState<number | null>(null);
  const [batches, setBatches] = useState<ShipmentBatch[]>([]);
  const [loadingBatches, setLoadingBatches] = useState(false);
  const [showImportForm, setShowImportForm] = useState(false);
  const [showExportForm, setShowExportForm] = useState<string | null>(null);

  // Form
  const [importQuantity, setImportQuantity] = useState<number>(0);
  const [importPrice, setImportPrice] = useState<number>(0);
  const [exportQuantity, setExportQuantity] = useState<number>(0);
  const [processingInventory, setProcessingInventory] = useState(false);

  useEffect(() => {
    if (isOpen && product) {
      loadProductDetail();
    } else if (!isOpen) {
      resetForm();
    }
  }, [isOpen, product]);

  useEffect(() => {
    if (selectedVariantIndex !== null && variants[selectedVariantIndex]) {
      loadBatches(variants[selectedVariantIndex].slug);
      // Trượt mượt sang kho trên mobile
      if (window.innerWidth < 1024 && inventoryRef.current) {
        inventoryRef.current.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }
  }, [selectedVariantIndex]);

  const resetForm = () => {
    setVariants([]);
    setIsDiscontinued(false);
    setErrors([]);
    setSelectedVariantIndex(null);
    setBatches([]);
    setShowImportForm(false);
    setShowExportForm(null);
    setImportQuantity(0);
    setImportPrice(0);
    setExportQuantity(0);
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
      setIsDiscontinued(detail.isDiscontinued || false);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Lỗi khi tải chi tiết sản phẩm';
      notify.error(message);
      onClose();
    } finally {
      setLoadingDetail(false);
    }
  };

  const loadBatches = async (variantSlug: string) => {
    if (!product) return;
    try {
      setLoadingBatches(true);
      const url = `${BACKEND_API_URL}/api/inventory/batches/all?productSlug=${product.slug}&variantSlug=${variantSlug}`;
      const response = await fetch(url);
      const text = await response.text();
      if (!response.ok || text.trim().startsWith('<!') || text.includes('doctype')) {
        throw new Error('Không kết nối được với backend kho hàng');
      }
      const result = JSON.parse(text);
      setBatches(result.data || []);
    } catch (err: any) {
      console.error('Load batches error:', err);
      notify.error(err.message || 'Không thể tải danh sách lô hàng');
      setBatches([]);
    } finally {
      setLoadingBatches(false);
    }
  };

  const handleImportStock = async () => {
    if (!product || selectedVariantIndex === null) return;
    if (importQuantity <= 0) {
      notify.error('Số lượng nhập phải lớn hơn 0');
      return;
    }
    const variant = variants[selectedVariantIndex];
    try {
      setProcessingInventory(true);
      const response = await fetch(`${BACKEND_API_URL}/api/inventory/import`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          productSlug: product.slug,
          variantSlug: variant.slug,
          quantity: importQuantity,
          importPrice: importPrice > 0 ? importPrice : null,
        }),
      });
      const text = await response.text();
      if (!response.ok || text.trim().startsWith('<!')) {
        throw new Error('Nhập hàng thất bại');
      }
      const result = JSON.parse(text);
      notify.success(`Nhập hàng thành công! Mã lô: ${result.data.batchCode}`);
      await loadBatches(variant.slug);
      setShowImportForm(false);
      setImportQuantity(0);
      setImportPrice(0);
    } catch (err: any) {
      notify.error(err.message || 'Lỗi nhập hàng');
    } finally {
      setProcessingInventory(false);
    }
  };

  const handleExportStock = async (batchCode: string) => {
    if (exportQuantity <= 0) {
      notify.error('Số lượng xuất phải lớn hơn 0');
      return;
    }
    const batch = batches.find(b => b.batchCode === batchCode);
    if (batch && exportQuantity > batch.remainingQuantity) {
      notify.error(`Chỉ còn ${batch.remainingQuantity} sản phẩm trong lô này`);
      return;
    }
    try {
      setProcessingInventory(true);
      const response = await fetch(`${BACKEND_API_URL}/api/inventory/export`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          batchCode,
          quantity: exportQuantity,
        }),
      });
      if (!response.ok) throw new Error('Xuất hàng thất bại');
      notify.success('Xuất hàng thành công!');
      if (selectedVariantIndex !== null) {
        await loadBatches(variants[selectedVariantIndex].slug);
      }
      setShowExportForm(null);
      setExportQuantity(0);
    } catch (err: any) {
      notify.error(err.message || 'Lỗi xuất hàng');
    } finally {
      setProcessingInventory(false);
    }
  };

  const updatePrice = (index: number, field: 'originalPrice' | 'discountedPrice', value: number) => {
    setVariants(prev =>
      prev.map((v, i) => (i === index ? { ...v, [field]: value } : v))
    );
  };

  const handleImageSelect = (variantIndex: number, files: FileList | null) => {
    if (!files?.length) return;
    const imageFiles = Array.from(files);
    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    const invalid = imageFiles.filter(f => !validTypes.includes(f.type));
    if (invalid.length > 0) {
      notify.error('Chỉ chấp nhận JPG, PNG, WEBP');
      return;
    }
    const oversized = imageFiles.filter(f => f.size > 5 * 1024 * 1024);
    if (oversized.length > 0) {
      notify.error('Ảnh không được quá 5MB');
      return;
    }
    setVariants(prev =>
      prev.map((v, i) =>
        i === variantIndex ? { ...v, newImages: [...v.newImages, ...imageFiles] } : v
      )
    );
    fileInputRefs.current[variantIndex]?.reset?.();
  };

  const removeExistingImage = (variantIndex: number, imgIndex: number) => {
    setVariants(prev =>
      prev.map((v, i) =>
        i === variantIndex
          ? { ...v, images: v.images.filter((_, idx) => idx !== imgIndex) }
          : v
      )
    );
  };

  const removeNewImage = (variantIndex: number, imgIndex: number) => {
    setVariants(prev =>
      prev.map((v, i) =>
        i === variantIndex
          ? { ...v, newImages: v.newImages.filter((_, idx) => idx !== imgIndex) }
          : v
      )
    );
  };

  const handleSubmit = async () => {
    if (!product) return;
    const priceErrors = variants.flatMap((v, i) => {
      const errs: string[] = [];
      if (v.originalPrice <= 0) errs.push(`Biến thể ${i + 1}: Giá gốc phải > 0`);
      if (v.discountedPrice <= 0) errs.push(`Biến thể ${i + 1}: Giá giảm phải > 0`);
      if (v.discountedPrice > v.originalPrice)
        errs.push(`Biến thể ${i + 1}: Giá giảm phải ≤ giá gốc`);
      return errs;
    });
    if (priceErrors.length) {
      setErrors(priceErrors);
      return;
    }
    try {
      setLoading(true);
      for (let i = 0; i < variants.length; i++) {
        if (variants[i].newImages.length > 0) {
          await api.updateVariantImages(product.slug, variants[i].slug, variants[i].newImages);
        }
      }
      const priceUpdates = variants.map(v => ({
        productSlug: product.slug,
        variantSlug: v.slug,
        originalPrice: v.originalPrice,
        discountedPrice: v.discountedPrice,
      }));
      await api.bulkUpdatePrices(priceUpdates);
      await api.updateIsDiscontinued(product.slug, isDiscontinued);
      notify.success('Cập nhật sản phẩm thành công!');
      onSuccess?.();
      onClose();
    } catch (err: any) {
      const msg = err.message || 'Lỗi server';
      setErrors([msg]);
      notify.error(msg);
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (date: string) =>
    new Date(date).toLocaleString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });

  const formatCurrency = (value: number | null) =>
    value == null ? 'Chưa có' : new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);

  if (!product) return null;

  const totalStock = batches.reduce((sum, b) => sum + b.remainingQuantity, 0);

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Quản lý Sản phẩm"
      size="custom"
      className="max-w-[70vw] w-full mx-auto"
    >
      <div className="flex flex-col lg:grid lg:grid-cols-12 gap-6 h-[78vh] text-sm">
        {/* LEFT: GIÁ & ẢNH */}
        <div className="flex flex-col overflow-hidden lg:col-span-8">
          <div className="flex-1 pb-20 pr-2 overflow-y-auto lg:pb-2">
            {loadingDetail ? (
              <div className="flex items-center justify-center h-full text-gray-500">Đang tải...</div>
            ) : (
              <>
                <div className="flex items-center gap-4 p-5 mb-5 border shadow-sm rounded-xl bg-gray-50">
                  {product.firstImage && (
                    <img
                      src={product.firstImage}
                      alt={product.name}
                      className="object-cover w-16 h-16 border rounded-lg shadow-sm"
                    />
                  )}
                  <div>
                    <p className="text-lg font-bold text-gray-900">{product.name}</p>
                    <p className="text-sm text-gray-500">{product.brand}</p>
                  </div>
                </div>

                <div className="p-5 mb-5 bg-white border shadow-sm rounded-xl">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <Power className="w-5 h-5 text-gray-600" />
                      <span className="font-semibold text-gray-800">Trạng thái kinh doanh</span>
                    </div>
                    <label className="relative inline-flex items-center cursor-pointer">
                      <input
                        type="checkbox"
                        checked={isDiscontinued}
                        onChange={e => setIsDiscontinued(e.target.checked)}
                        className="sr-only peer"
                      />
                      <div className="w-12 h-7 bg-gray-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:bg-red-600 after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-6 after:w-6 after:transition-all"></div>
                      <span className="ml-3 font-medium text-gray-700">
                        {isDiscontinued ? 'Ngừng kinh doanh' : 'Đang bán'}
                      </span>
                    </label>
                  </div>
                </div>

                <div className="space-y-5">
                  <div className="flex items-center gap-2 mb-4">
                    <DollarSign className="w-5 h-5 text-violet-600" />
                    <h3 className="text-lg font-bold text-gray-900">Biến thể & Giá bán</h3>
                  </div>

                  {variants.map((variant, index) => {
                    const discount = variant.originalPrice - variant.discountedPrice;
                    const percent = variant.originalPrice > 0 ? Math.round((discount / variant.originalPrice) * 100) : 0;
                    const isSelected = selectedVariantIndex === index;

                    return (
                      <div
                        key={variant.slug}
                        onClick={() => setSelectedVariantIndex(index)}
                        className={`p-5 border-2 rounded-xl cursor-pointer transition-all shadow-sm ${
                          isSelected
                            ? 'border-violet-500 bg-violet-50 ring-2 ring-violet-200'
                            : 'border-gray-200 bg-white hover:border-violet-400 hover:shadow-md'
                        }`}
                      >
                        <div className="flex items-center justify-between mb-4">
                          <div className="flex flex-wrap gap-2">
                            {Object.entries(variant.attributes).map(([k, v]) => (
                              <span
                                key={k}
                                className="px-3 py-1 text-xs font-medium text-blue-700 bg-blue-100 rounded-full"
                              >
                                {k}: {v}
                              </span>
                            ))}
                          </div>
                          {isSelected && <ChevronRight className="w-6 h-6 text-violet-600" />}
                        </div>

                        <div className="grid grid-cols-1 gap-4 mb-5 sm:grid-cols-3">
                          <div>
                            <label className="block mb-1 text-xs font-medium text-gray-600">Giá gốc</label>
                            <input
                              type="number"
                              value={variant.originalPrice}
                              onChange={e => updatePrice(index, 'originalPrice', parseFloat(e.target.value) || 0)}
                              onClick={e => e.stopPropagation()}
                              className="w-full px-3 py-2.5 text-right border rounded-lg focus:ring-2 focus:ring-violet-400 text-sm"
                              min="0"
                              step="1000"
                            />
                          </div>
                          <div>
                            <label className="block mb-1 text-xs font-medium text-gray-600">Giá giảm</label>
                            <input
                              type="number"
                              value={variant.discountedPrice}
                              onChange={e => updatePrice(index, 'discountedPrice', parseFloat(e.target.value) || 0)}
                              onClick={e => e.stopPropagation()}
                              className="w-full px-3 py-2.5 text-right border rounded-lg focus:ring-2 focus:ring-violet-400 text-sm"
                              min="0"
                              step="1000"
                            />
                          </div>
                          <div>
                            <label className="block mb-1 text-xs font-medium text-gray-600">Tiết kiệm</label>
                            <div className="flex items-center justify-center h-10 border-2 border-green-300 rounded-lg bg-green-50">
                              {discount > 0 ? (
                                <span className="flex items-center gap-1 text-sm font-bold text-green-700">
                                  <TrendingDown className="w-4 h-4" />
                                  {percent}%
                                </span>
                              ) : (
                                <span className="text-sm text-gray-400">—</span>
                              )}
                            </div>
                          </div>
                        </div>

                        <div onClick={e => e.stopPropagation()}>
                          <label className="block mb-2 text-xs font-medium text-gray-600">
                            Hình ảnh ({variant.images.length + variant.newImages.length})
                          </label>
                          <div className="grid grid-cols-6 gap-2 sm:grid-cols-8">
                            {variant.images.map((img, i) => (
                              <div key={`old-${i}`} className="relative group aspect-square">
                                <img
                                  src={img}
                                  alt=""
                                  className="object-cover w-full h-full border-2 border-gray-300 rounded-lg shadow-sm"
                                />
                                <button
                                  onClick={() => removeExistingImage(index, i)}
                                  className="absolute -top-1.5 -right-1.5 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition shadow"
                                >
                                  <X className="w-3 h-3" />
                                </button>
                              </div>
                            ))}
                            {variant.newImages.map((file, i) => (
                              <div key={`new-${i}`} className="relative group aspect-square">
                                <img
                                  src={URL.createObjectURL(file)}
                                  alt=""
                                  className="object-cover w-full h-full border-2 border-green-500 rounded-lg shadow-sm"
                                />
                                <span className="absolute top-1 left-1 px-1.5 py-0.5 text-xs font-bold text-white bg-green-600 rounded">
                                  Mới
                                </span>
                                <button
                                  onClick={() => removeNewImage(index, i)}
                                  className="absolute -top-1.5 -right-1.5 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition shadow"
                                >
                                  <X className="w-3 h-3" />
                                </button>
                              </div>
                            ))}
                            <label
                              htmlFor={`upload-${index}`}
                              className="flex flex-col items-center justify-center transition border-2 border-gray-400 border-dashed rounded-lg cursor-pointer aspect-square hover:border-violet-500 hover:bg-violet-50 bg-gray-50"
                            >
                              <Upload className="w-6 h-6 text-gray-400" />
                              <span className="mt-1 text-xs text-gray-500">Thêm</span>
                            </label>
                          </div>
                          <input
                            ref={el => (fileInputRefs.current[index] = el)}
                            id={`upload-${index}`}
                            type="file"
                            multiple
                            accept="image/jpeg,image/jpg,image/png,image/webp"
                            onChange={e => handleImageSelect(index, e.target.files)}
                            className="hidden"
                          />
                          <p className="mt-2 text-xs text-gray-500">JPG, PNG, WEBP • Tối đa 5MB</p>
                        </div>
                      </div>
                    );
                  })}
                </div>

                {errors.length > 0 && (
                  <div className="p-4 mt-5 border-2 border-red-300 rounded-xl bg-red-50">
                    {errors.map((e, i) => (
                      <div key={i} className="text-sm text-red-700">• {e}</div>
                    ))}
                  </div>
                )}
              </>
            )}
          </div>
        </div>

        {/* RIGHT: QUẢN LÝ KHO */}
        <div
          ref={inventoryRef}
          className="flex flex-col pt-4 overflow-hidden border-t-2 border-gray-200 lg:col-span-4 bg-gray-50 lg:border-t-0 lg:border-l-2 lg:pt-0 lg:pl-5 lg:pr-3"
        >
          <div className="flex-1 px-1 pb-4 overflow-y-auto">
            {selectedVariantIndex !== null ? (
              <>
                <div className="flex items-center justify-between px-2 mb-4">
                  <div className="flex items-center gap-2">
                    <Package className="w-6 h-6 text-violet-600" />
                    <h3 className="text-lg font-bold text-gray-900">Quản lý kho</h3>
                  </div>
                  <div className="text-right bg-white px-3 py-1.5 rounded-lg shadow-sm border">
                    <div className="text-xs text-gray-600">Tồn kho</div>
                    <div className="text-2xl font-bold text-violet-700">{totalStock}</div>
                  </div>
                </div>

                <div className="p-3 mx-1 mb-5 border-2 border-violet-300 rounded-xl bg-violet-100">
                  <div className="flex flex-wrap gap-1.5">
                    {Object.entries(variants[selectedVariantIndex].attributes).map(([k, v]) => (
                      <span
                        key={k}
                        className="px-2.5 py-1 text-xs font-medium bg-white text-violet-800 rounded-full shadow-sm"
                      >
                        {k}: {v}
                      </span>
                    ))}
                  </div>
                </div>

                <button
                  onClick={() => setShowImportForm(!showImportForm)}
                  disabled={processingInventory}
                  className="flex items-center justify-center w-full gap-2 py-3 mx-1 mb-5 text-base font-medium text-white transition bg-green-600 rounded-xl hover:bg-green-700 disabled:opacity-60"
                >
                  <Plus className="w-5 h-5" />
                  Nhập hàng mới
                </button>

                {showImportForm && (
                  <div className="p-4 mx-1 mb-5 border-2 border-green-400 shadow-sm rounded-xl bg-green-50">
                    <div className="space-y-3">
                      <div>
                        <label className="block mb-1.5 text-sm font-medium">Số lượng nhập *</label>
                        <input
                          type="number"
                          value={importQuantity}
                          onChange={e => setImportQuantity(parseInt(e.target.value) || 0)}
                          className="w-full px-3 py-2.5 border rounded-lg focus:ring-2 focus:ring-green-400 text-sm"
                          min="1"
                          placeholder="VD: 100"
                        />
                      </div>
                      <div>
                        <label className="block mb-1.5 text-sm font-medium">Giá nhập (tùy chọn)</label>
                        <input
                          type="number"
                          value={importPrice}
                          onChange={e => setImportPrice(parseFloat(e.target.value) || 0)}
                          className="w-full px-3 py-2.5 border rounded-lg focus:ring-2 focus:ring-green-400 text-sm"
                          min="0"
                          placeholder="VD: 125000"
                        />
                      </div>
                      <div className="flex gap-2">
                        <button
                          onClick={() => {
                            setShowImportForm(false);
                            setImportQuantity(0);
                            setImportPrice(0);
                          }}
                          className="flex-1 py-2.5 text-sm font-medium border-2 rounded-lg hover:bg-gray-100 transition"
                        >
                          Hủy
                        </button>
                        <button
                          onClick={handleImportStock}
                          disabled={processingInventory || importQuantity < 1}
                          className="flex-1 py-2.5 text-sm font-medium text-white bg-green-600 rounded-lg hover:bg-green-700 disabled:opacity-60 transition"
                        >
                          {processingInventory ? 'Đang xử lý...' : 'Xác nhận'}
                        </button>
                      </div>
                    </div>
                  </div>
                )}

                <div className="px-1">
                  <h4 className="mb-3 text-base font-bold text-gray-900">Lô hàng ({batches.length})</h4>
                  {loadingBatches ? (
                    <div className="py-10 text-center text-gray-500">Đang tải...</div>
                  ) : batches.length === 0 ? (
                    <div className="py-10 text-center text-gray-400">Chưa có lô hàng</div>
                  ) : (
                    <div className="space-y-3">
                      {batches.map(batch => (
                        <div
                          key={batch.batchCode}
                          className={`p-4 border-2 rounded-xl shadow-sm transition-all mx-1 ${
                            batch.remainingQuantity === 0 ? 'bg-gray-100 opacity-70' : 'bg-white'
                          }`}
                        >
                          <div className="flex items-start justify-between mb-2">
                            <div>
                              <div className="text-base font-bold text-gray-900">{batch.batchCode}</div>
                              <div className="text-xs text-gray-600">{formatDate(batch.importedAt)}</div>
                            </div>
                            <div className="text-right">
                              <div className="text-xl font-bold text-violet-700">{batch.remainingQuantity}</div>
                              <div className="text-xs text-gray-500">/ {batch.importedQuantity} nhập</div>
                            </div>
                          </div>
                          <div className="mb-3 text-xs text-gray-700">
                            Giá nhập: <span className="font-medium">{formatCurrency(batch.importPrice)}</span>
                          </div>
                          {batch.remainingQuantity > 0 && (
                            <>
                              {showExportForm === batch.batchCode ? (
                                <div className="flex gap-2">
                                  <input
                                    type="number"
                                    value={exportQuantity}
                                    onChange={e => setExportQuantity(parseInt(e.target.value) || 0)}
                                    min="1"
                                    max={batch.remainingQuantity}
                                    className="flex-1 px-3 py-2 text-sm text-center border rounded-lg"
                                    placeholder="SL"
                                  />
                                  <button
                                    onClick={() => handleExportStock(batch.batchCode)}
                                    disabled={processingInventory || exportQuantity < 1}
                                    className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 disabled:opacity-60"
                                  >
                                    OK
                                  </button>
                                  <button
                                    onClick={() => {
                                      setShowExportForm(null);
                                      setExportQuantity(0);
                                    }}
                                    className="px-3 py-2 text-sm font-medium border rounded-lg hover:bg-gray-100"
                                  >
                                    Hủy
                                  </button>
                                </div>
                              ) : (
                                <button
                                  onClick={() => setShowExportForm(batch.batchCode)}
                                  className="w-full flex items-center justify-center gap-2 py-2.5 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition"
                                >
                                  <Minus className="w-4 h-4" />
                                  Xuất kho
                                </button>
                              )}
                            </>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </>
            ) : (
              <div className="flex flex-col items-center justify-center h-full text-gray-500">
                <Package className="w-16 h-16 mb-4 opacity-40" />
                <p className="text-sm text-center">
                  Chọn một biến thể bên trái<br />để quản lý kho hàng
                </p>
              </div>
            )}
          </div>
        </div>

        {/* NÚT CỐ ĐỊNH DƯỚI CÙNG TRÊN MOBILE */}
        <div className="fixed bottom-0 left-0 right-0 z-50 flex gap-3 p-4 bg-white border-t shadow-lg lg:hidden">
          <button
            onClick={onClose}
            disabled={loading}
            className="flex-1 py-3 text-base font-medium transition border-2 border-gray-300 rounded-xl hover:bg-gray-50 disabled:opacity-50"
          >
            Hủy
          </button>
          <button
            onClick={handleSubmit}
            disabled={loading || variants.length === 0}
            className="flex-1 py-3 text-base font-medium text-white transition bg-violet-600 rounded-xl hover:bg-violet-700 disabled:opacity-50"
          >
            {loading ? 'Đang lưu...' : 'Lưu thay đổi'}
          </button>
        </div>

        {/* NÚT TRÊN DESKTOP */}
        <div className="hidden col-span-12 gap-3 pt-4 bg-white border-t lg:flex">
          <button
            onClick={onClose}
            disabled={loading}
            className="flex-1 py-3 text-base font-medium transition border-2 border-gray-300 rounded-xl hover:bg-gray-50 disabled:opacity-50"
          >
            Hủy
          </button>
          <button
            onClick={handleSubmit}
            disabled={loading || variants.length === 0}
            className="flex-1 py-3 text-base font-medium text-white transition bg-violet-600 rounded-xl hover:bg-violet-700 disabled:opacity-50"
          >
            {loading ? 'Đang lưu...' : 'Lưu thay đổi'}
          </button>
        </div>
      </div>
    </Modal>
  );
};