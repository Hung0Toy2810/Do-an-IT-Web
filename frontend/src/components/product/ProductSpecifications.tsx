// src/components/product/ProductSpecifications.tsx
interface Spec { label: string; value: string }

export default function ProductSpecifications({ specs }: { specs: Spec[] }) {
  return (
    <div className="p-5 bg-white border shadow-xl sm:p-6 rounded-2xl border-violet-100/50">
      <h3 className="mb-4 text-lg font-bold text-gray-900">Thông số kỹ thuật</h3>
      <div className="space-y-3">
        {specs.map((s, i) => (
          <div key={i} className="flex justify-between py-2 border-b border-gray-100 last:border-0">
            <span className="text-sm text-gray-600">{s.label}</span>
            <span className="text-sm font-medium text-gray-900">{s.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
}