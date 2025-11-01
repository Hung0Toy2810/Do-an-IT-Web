// src/components/product/AddProductModal.tsx
'use client';

import { useState, useEffect, useMemo, useRef } from 'react';
import { Plus, Trash2, X, Edit2, Check } from 'lucide-react';
import { Modal } from '../ui/Modal';
import { notify } from '../../utils/notify';
import { api } from '../../services/productApi';
import { slugify } from '../../utils/slugify';

interface Attribute {
  key: string;
  values: string[];
}

interface Specification {
  label: string;
  value: string;
}

interface VariantDraft {
  slug: string;
  attributes: Record<string, string>;
  originalPrice: number;
  discountedPrice: number;
  specifications: Specification[];
}

interface AddProductModalProps {
  isOpen: boolean;
  onClose: () => void;
  subCategoryId?: number;
  onSuccess?: () => void;
}

export const AddProductModal = ({
  isOpen,
  onClose,
  subCategoryId,
  onSuccess,
}: AddProductModalProps) => {
  const [name, setName] = useState('');
  const [brand, setBrand] = useState('');
  const [description, setDescription] = useState('');
  const [slug, setSlug] = useState('');
  const [attributes, setAttributes] = useState<Attribute[]>([]);
  const [variants, setVariants] = useState<VariantDraft[]>([]);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);

  const attrKeyRef = useRef<HTMLInputElement>(null);
  const attrValueRef = useRef<HTMLInputElement>(null);
  const [editingAttrKey, setEditingAttrKey] = useState<string | null>(null);

  // State cho việc edit specifications
  const [editingSpec, setEditingSpec] = useState<{ variantIndex: number; specIndex: number } | null>(null);
  const [tempSpecLabel, setTempSpecLabel] = useState('');
  const [tempSpecValue, setTempSpecValue] = useState('');

  // Auto slug
  useEffect(() => {
    if (name.trim()) setSlug(slugify(name));
  }, [name]);

  // Generate variants
  const generatedVariants = useMemo(() => {
    if (!attributes.length) return [];
    const keys = attributes.map(a => a.key);
    const lists = attributes.map(a => a.values.filter(Boolean));
    if (lists.some(l => !l.length)) return [];

    const combos: Record<string, string>[] = [{}];
    for (let i = 0; i < keys.length; i++) {
      const newCombos: Record<string, string>[] = [];
      for (const c of combos) {
        for (const v of lists[i]) {
          newCombos.push({ ...c, [keys[i]]: v });
        }
      }
      combos.length = 0;
      combos.push(...newCombos);
    }

    return combos.map(attrs => ({
      slug: `${slug}-${Object.values(attrs).map(slugify).join('-').replace(/-+/g, '-')}`,
      attributes: attrs,
      originalPrice: 0,
      discountedPrice: 0,
      specifications: [] as Specification[],
    }));
  }, [attributes, slug]);

  useEffect(() => {
    setVariants(prev => {
      if (!generatedVariants.length) return [];
      return generatedVariants.map((gen, i) => {
        const old = prev[i];
        return old && old.slug === gen.slug ? { ...old, specifications: old.specifications } : gen;
      });
    });
  }, [generatedVariants]);

  // Edit attribute
  const startEditAttribute = (key: string) => {
    setEditingAttrKey(key);
    const attr = attributes.find(a => a.key === key);
    if (attr && attrValueRef.current) {
      attrValueRef.current.value = attr.values.join(', ');
      attrValueRef.current.focus();
    }
  };

  const saveAttributeValues = () => {
    if (!editingAttrKey || !attrValueRef.current) return;
    const values = attrValueRef.current.value.split(',').map(v => v.trim()).filter(Boolean);
    if (!values.length) return;

    setAttributes(attrs =>
      attrs.map(a => a.key === editingAttrKey ? { ...a, values } : a)
    );
    setEditingAttrKey(null);
    attrValueRef.current.value = '';
  };

  const addNewAttribute = () => {
    const key = attrKeyRef.current?.value.trim();
    const valueInput = attrValueRef.current?.value || '';
    const values = valueInput.split(',').map(v => v.trim()).filter(Boolean);
    if (!key || !values.length) return notify.warning('Nhập tên và giá trị');

    setAttributes(attrs => {
      const existing = attrs.find(a => a.key === key);
      if (existing) {
        const newVals = [...existing.values, ...values.filter(v => !existing.values.includes(v))];
        return attrs.map(a => a.key === key ? { ...a, values: newVals } : a);
      }
      return [...attrs, { key, values }];
    });

    if (attrKeyRef.current) attrKeyRef.current.value = '';
    if (attrValueRef.current) attrValueRef.current.value = '';
  };

  const removeAttribute = (key: string) => {
    setAttributes(a => a.filter(x => x.key !== key));
  };

  const removeValue = (key: string, value: string) => {
    setAttributes(a => a.map(x =>
      x.key === key ? { ...x, values: x.values.filter(v => v !== value) } : x
    ).filter(x => x.values.length > 0));
  };

  const updatePrice = (i: number, field: 'originalPrice' | 'discountedPrice', val: number) => {
    setVariants(v => v.map((x, idx) => idx === i ? { ...x, [field]: val } : x));
  };

  // Specification handlers
  const startEditSpec = (variantIndex: number, specIndex: number) => {
    const spec = variants[variantIndex].specifications[specIndex];
    setEditingSpec({ variantIndex, specIndex });
    setTempSpecLabel(spec.label);
    setTempSpecValue(spec.value);
  };

  const saveSpec = () => {
    if (!editingSpec || !tempSpecLabel.trim() || !tempSpecValue.trim()) return;

    setVariants(vs =>
      vs.map((v, i) =>
        i === editingSpec.variantIndex
          ? {
              ...v,
              specifications: v.specifications.map((s, j) =>
                j === editingSpec.specIndex
                  ? { label: tempSpecLabel.trim(), value: tempSpecValue.trim() }
                  : s
              ),
            }
          : v
      )
    );
    setEditingSpec(null);
    setTempSpecLabel('');
    setTempSpecValue('');
  };

  const cancelEditSpec = () => {
    setEditingSpec(null);
    setTempSpecLabel('');
    setTempSpecValue('');
  };

  const addSpec = (variantIndex: number, label: string, value: string) => {
    if (!label.trim() || !value.trim()) return;

    setVariants(vs =>
      vs.map((v, i) =>
        i === variantIndex
          ? { ...v, specifications: [...v.specifications, { label: label.trim(), value: value.trim() }] }
          : v
      )
    );
  };

  const removeSpec = (variantIndex: number, specIndex: number) => {
    setVariants(vs =>
      vs.map((v, i) =>
        i === variantIndex
          ? { ...v, specifications: v.specifications.filter((_, j) => j !== specIndex) }
          : v
      )
    );
  };

  const handleSubmit = async () => {
    setErrors([]);

    if (!subCategoryId) return setErrors(['Chưa chọn danh mục con']);
    if (!name || !brand || !description || !attributes.length || !variants.length) {
      return setErrors(['Thiếu thông tin bắt buộc']);
    }

    const specErrors = variants.flatMap((v, i) => {
      if (!v.specifications.length) return [`Biến thể ${i + 1}: Phải có ít nhất 1 thông số`];
      return [];
    });

    const priceErrors = variants.flatMap((v, i) => {
      const errs: string[] = [];
      if (v.originalPrice <= 0) errs.push(`Biến thể ${i + 1}: Giá gốc > 0`);
      if (v.discountedPrice <= 0) errs.push(`Biến thể ${i + 1}: Giá giảm > 0`);
      if (v.discountedPrice > v.originalPrice) errs.push(`Biến thể ${i + 1}: Giá giảm ≤ giá gốc`);
      return errs;
    });

    if (specErrors.length || priceErrors.length) {
      return setErrors([...specErrors, ...priceErrors]);
    }

    const payload = {
      id: Date.now(),
      name: name.trim(),
      slug,
      brand: brand.trim(),
      description: description.trim(),
      subCategoryId,
      attributeOptions: Object.fromEntries(attributes.map(a => [a.key, a.values])),
      variants: variants.map(v => ({
        ...v,
        specifications: v.specifications,
      })),
    };

    try {
      setLoading(true);
      await api.createProduct(payload);
      notify.success('Tạo sản phẩm thành công!');
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

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Tạo sản phẩm" size="xl">
      <div className="space-y-5 text-sm max-h-[80vh] overflow-y-auto">
        {/* Thông tin cơ bản */}
        <div className="grid grid-cols-2 gap-3">
          <input placeholder="Tên *" value={name} onChange={e => setName(e.target.value)}
            className="px-3 py-2 border rounded-lg" />
          <input placeholder="Thương hiệu *" value={brand} onChange={e => setBrand(e.target.value)}
            className="px-3 py-2 border rounded-lg" />
          <input placeholder="Slug" value={slug} onChange={e => setSlug(slugify(e.target.value))}
            className="col-span-2 px-3 py-2 border rounded-lg" />
          <textarea placeholder="Mô tả *" value={description} onChange={e => setDescription(e.target.value)}
            rows={2} className="col-span-2 px-3 py-2 border rounded-lg" />
        </div>

        {/* Thuộc tính */}
        <div className="space-y-2">
          <div className="flex gap-2">
            <input ref={attrKeyRef} placeholder="Tên thuộc tính" className="flex-1 px-3 py-2 text-xs border rounded-lg"
              onKeyDown={e => e.key === 'Enter' && attrValueRef.current?.focus()} />
            <input ref={attrValueRef} placeholder={editingAttrKey ? 'Chỉnh giá trị (cách nhau ,)' : 'Giá trị'}
              className="flex-1 px-3 py-2 text-xs border rounded-lg"
              onKeyDown={e => e.key === 'Enter' && (editingAttrKey ? saveAttributeValues() : addNewAttribute())} />
            <button onClick={editingAttrKey ? saveAttributeValues : addNewAttribute}
              className="px-3 py-2 text-xs text-white rounded-lg bg-violet-600">
              {editingAttrKey ? 'Lưu' : 'Thêm'}
            </button>
          </div>

          <div className="flex flex-wrap gap-2">
            {attributes.map(attr => (
              <div key={attr.key} className="flex items-center gap-1 p-2 text-xs bg-gray-100 rounded-lg">
                <button onClick={() => startEditAttribute(attr.key)}
                  className="font-medium hover:text-violet-600">{attr.key}:</button>
                <div className="flex gap-1">
                  {attr.values.map(v => (
                    <span key={v} className="px-2 py-0.5 bg-white rounded border flex items-center gap-1">
                      {v}
                      <X className="w-3 h-3 text-red-500 cursor-pointer" onClick={() => removeValue(attr.key, v)} />
                    </span>
                  ))}
                </div>
                <Trash2 className="w-3 h-3 text-red-500 cursor-pointer" onClick={() => removeAttribute(attr.key)} />
              </div>
            ))}
          </div>
        </div>

        {/* Biến thể + Thông số */}
        {variants.length > 0 && (
          <div className="overflow-hidden border rounded-lg">
            <table className="w-full text-xs">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-2 py-1 text-left">Slug</th>
                  {attributes.map(a => <th key={a.key} className="px-2 py-1 text-left">{a.key}</th>)}
                  <th className="w-20 px-2 py-1 text-right">Giá gốc</th>
                  <th className="w-20 px-2 py-1 text-right">Giá giảm</th>
                  <th className="px-2 py-1 text-left">Thông số</th>
                </tr>
              </thead>
              <tbody>
                {variants.map((v, i) => (
                  <VariantRow
                    key={i}
                    variant={v}
                    variantIndex={i}
                    attributes={attributes}
                    updatePrice={updatePrice}
                    editingSpec={editingSpec}
                    tempSpecLabel={tempSpecLabel}
                    tempSpecValue={tempSpecValue}
                    setTempSpecLabel={setTempSpecLabel}
                    setTempSpecValue={setTempSpecValue}
                    startEditSpec={startEditSpec}
                    saveSpec={saveSpec}
                    cancelEditSpec={cancelEditSpec}
                    addSpec={addSpec}
                    removeSpec={removeSpec}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Lỗi */}
        {errors.length > 0 && (
          <div className="p-3 text-xs text-red-600 border border-red-200 rounded-lg bg-red-50">
            {errors.map((e, i) => <div key={i}>• {e}</div>)}
          </div>
        )}

        {/* Nút */}
        <div className="flex gap-2">
          <button onClick={onClose} disabled={loading}
            className="flex-1 px-3 py-2 text-xs border rounded-lg">Hủy</button>
          <button onClick={handleSubmit} disabled={loading || !variants.length}
            className="flex-1 px-3 py-2 text-xs text-white rounded-lg bg-violet-600 disabled:opacity-50">
            {loading ? 'Đang tạo...' : 'Tạo'}
          </button>
        </div>
      </div>
    </Modal>
  );
};

// Component riêng cho mỗi variant row
interface VariantRowProps {
  variant: VariantDraft;
  variantIndex: number;
  attributes: Attribute[];
  updatePrice: (i: number, field: 'originalPrice' | 'discountedPrice', val: number) => void;
  editingSpec: { variantIndex: number; specIndex: number } | null;
  tempSpecLabel: string;
  tempSpecValue: string;
  setTempSpecLabel: (val: string) => void;
  setTempSpecValue: (val: string) => void;
  startEditSpec: (variantIndex: number, specIndex: number) => void;
  saveSpec: () => void;
  cancelEditSpec: () => void;
  addSpec: (variantIndex: number, label: string, value: string) => void;
  removeSpec: (variantIndex: number, specIndex: number) => void;
}

const VariantRow = ({
  variant,
  variantIndex,
  attributes,
  updatePrice,
  editingSpec,
  tempSpecLabel,
  tempSpecValue,
  setTempSpecLabel,
  setTempSpecValue,
  startEditSpec,
  saveSpec,
  cancelEditSpec,
  addSpec,
  removeSpec,
}: VariantRowProps) => {
  const [newSpecLabel, setNewSpecLabel] = useState('');
  const [newSpecValue, setNewSpecValue] = useState('');

  const handleAddSpec = () => {
    if (newSpecLabel.trim() && newSpecValue.trim()) {
      addSpec(variantIndex, newSpecLabel, newSpecValue);
      setNewSpecLabel('');
      setNewSpecValue('');
    }
  };

  const isEditing = (specIndex: number) =>
    editingSpec?.variantIndex === variantIndex && editingSpec?.specIndex === specIndex;

  return (
    <tr className="border-t">
      <td className="px-2 py-1 font-mono text-xs truncate max-w-32" title={variant.slug}>
        {variant.slug}
      </td>
      {attributes.map(a => (
        <td key={a.key} className="px-2 py-1">{variant.attributes[a.key]}</td>
      ))}
      <td className="px-2 py-1">
        <input
          type="number"
          value={variant.originalPrice || ''}
          onChange={e => updatePrice(variantIndex, 'originalPrice', +e.target.value)}
          className="w-full px-1 py-0.5 text-right border rounded"
        />
      </td>
      <td className="px-2 py-1">
        <input
          type="number"
          value={variant.discountedPrice || ''}
          onChange={e => updatePrice(variantIndex, 'discountedPrice', +e.target.value)}
          className="w-full px-1 py-0.5 text-right border rounded"
        />
      </td>
      <td className="px-2 py-1">
        <div className="space-y-1">
          {variant.specifications.map((spec, j) => (
            <div key={j} className="flex items-center gap-1">
              {isEditing(j) ? (
                <>
                  <input
                    value={tempSpecLabel}
                    onChange={e => setTempSpecLabel(e.target.value)}
                    className="w-16 px-1 py-0.5 text-xs border rounded"
                    autoFocus
                  />
                  <input
                    value={tempSpecValue}
                    onChange={e => setTempSpecValue(e.target.value)}
                    className="flex-1 px-1 py-0.5 text-xs border rounded"
                    onKeyDown={e => e.key === 'Enter' && saveSpec()}
                  />
                  <button onClick={saveSpec} className="text-green-600">
                    <Check className="w-3 h-3" />
                  </button>
                  <button onClick={cancelEditSpec} className="text-red-600">
                    <X className="w-3 h-3" />
                  </button>
                </>
              ) : (
                <>
                  <span className="font-medium">{spec.label}:</span>
                  <span>{spec.value}</span>
                  <button onClick={() => startEditSpec(variantIndex, j)} className="text-blue-600">
                    <Edit2 className="w-3 h-3" />
                  </button>
                  <button onClick={() => removeSpec(variantIndex, j)} className="text-red-600">
                    <X className="w-3 h-3" />
                  </button>
                </>
              )}
            </div>
          ))}
          <div className="flex gap-1 mt-1">
            <input
              placeholder="Nhãn"
              value={newSpecLabel}
              onChange={e => setNewSpecLabel(e.target.value)}
              className="w-16 px-1 py-0.5 text-xs border rounded"
            />
            <input
              placeholder="Giá trị"
              value={newSpecValue}
              onChange={e => setNewSpecValue(e.target.value)}
              className="flex-1 px-1 py-0.5 text-xs border rounded"
              onKeyDown={e => e.key === 'Enter' && handleAddSpec()}
            />
            <button onClick={handleAddSpec} className="text-violet-600">
              <Plus className="w-3 h-3" />
            </button>
          </div>
        </div>
      </td>
    </tr>
  );
};