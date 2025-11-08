// src/components/product/ProductVariants.tsx
interface Props {
  attributeOptions: { [key: string]: string[] };
  selected: { [key: string]: string };
  onChange: (key: string, value: string) => void;
}

export default function ProductVariants({ attributeOptions, selected, onChange }: Props) {
  return (
    <div className="space-y-5">
      {Object.entries(attributeOptions).map(([key, values]) => (
        <div key={key}>
          <p className="mb-2 text-sm font-semibold text-gray-700">
            {key}: <span className="text-violet-700">{selected[key]}</span>
          </p>
          <div className="flex flex-wrap gap-2">
            {values.map((val) => (
              <button
                key={val}
                onClick={() => onChange(key, val)}
                className={`px-3 py-2 text-sm font-medium rounded-xl border-2 transition-all ${
                  selected[key] === val
                    ? 'border-violet-800 bg-violet-50 text-violet-900'
                    : 'border-gray-300 bg-white text-gray-700 hover:border-violet-500'
                }`}
              >
                {val}
              </button>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
}