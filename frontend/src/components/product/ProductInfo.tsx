// src/components/product/ProductInfo.tsx
import { Star } from 'lucide-react';

interface Props {
  name: string;
  brand: string;
  description: string;
  rating: number;
  totalRatings: number;
}

export default function ProductInfo({ name, brand, description, rating, totalRatings }: Props) {
  return (
    <div>
      <p className="text-sm font-semibold uppercase text-violet-700">{brand}</p>
      <h1 className="mt-1 text-xl font-bold text-gray-900 sm:text-2xl lg:text-3xl line-clamp-2">{name}</h1>

      <div className="flex items-center gap-2 mt-2">
        <div className="flex gap-0.5">
          {[...Array(5)].map((_, i) => (
            <Star
              key={i}
              className={`w-4 h-4 ${i < Math.floor(rating) ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}`}
            />
          ))}
        </div>
        <span className="text-sm font-medium text-gray-700">{rating.toFixed(1)}</span>
        <span className="text-sm text-gray-500">({totalRatings})</span>
      </div>

      <p className="mt-3 text-sm leading-relaxed text-gray-600 sm:text-base">{description}</p>
    </div>
  );
}