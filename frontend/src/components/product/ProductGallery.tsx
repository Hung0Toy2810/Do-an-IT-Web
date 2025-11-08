// src/components/product/ProductGallery.tsx
import { Heart } from 'lucide-react';
import { useState } from 'react';

interface Props {
  images: string[];
  productName: string;
  discountPercentage: number;
  isFavorite: boolean;
  onToggleFavorite: () => void;
}

export default function ProductGallery({ images, productName, discountPercentage, isFavorite, onToggleFavorite }: Props) {
  const [selectedIndex, setSelectedIndex] = useState(0);

  return (
    <div className="space-y-4">
      {/* Ảnh lớn */}
      <div className="relative overflow-hidden bg-gray-100 aspect-square sm:aspect-auto sm:h-96 lg:h-full rounded-2xl">
        <img
          src={images[selectedIndex] || '/placeholder.svg'}
          alt={productName}
          className="object-cover w-full h-full"
        />

        {discountPercentage > 0 && (
          <div className="absolute top-3 left-3 px-2.5 py-1 text-xs sm:text-sm font-bold text-white bg-violet-800 rounded-lg">
            -{Math.round(discountPercentage)}%
          </div>
        )}

        <button
          onClick={onToggleFavorite}
          className="absolute p-2 transition-all bg-white shadow-md top-3 right-3 rounded-xl hover:shadow-lg"
        >
          <Heart className={`w-5 h-5 ${isFavorite ? 'fill-violet-800 text-violet-800' : 'text-gray-500'}`} />
        </button>
      </div>

      {/* Thumbnail */}
      {images.length > 1 && (
        <div className="grid grid-cols-4 gap-2 sm:gap-3">
          {images.map((img, i) => (
            <button
              key={i}
              onClick={() => setSelectedIndex(i)}
              className={`aspect-square rounded-xl overflow-hidden border-2 transition-all ${
                i === selectedIndex ? 'border-violet-800' : 'border-gray-200'
              }`}
            >
              <img src={img} alt="" className="object-cover w-full h-full" />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}