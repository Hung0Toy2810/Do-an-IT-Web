import React, { useState, useEffect, useRef, useCallback } from "react";

interface DualSliderProps {
  min?: number;
  max?: number;
  step?: number;
  width?: string;
  height?: string;
  sliderHeight?: number;
  defaultMinValue?: number;
  defaultMaxValue?: number;
  currency?: string;
  onPriceChange?: (minPrice: number, maxPrice: number) => void;
}

const DualSlider: React.FC<DualSliderProps> = ({
  min = 0,
  max = 5000,
  step = 50,
  width = "100%",
  height = "auto",
  sliderHeight = 6,
  defaultMinValue = 420,
  defaultMaxValue = 3750,
  currency = "đ",
  onPriceChange,
}) => {
  const [minValue, setMinValue] = useState(defaultMinValue);
  const [maxValue, setMaxValue] = useState(defaultMaxValue);
  const [isDragging, setIsDragging] = useState<'min' | 'max' | null>(null);
  const sliderRef = useRef<HTMLDivElement>(null);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);
  const rafRef = useRef<number | null>(null);

  // Gọi callback khi kéo xong
  useEffect(() => {
    if (onPriceChange && !isDragging) {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
      timeoutRef.current = setTimeout(() => {
        onPriceChange(minValue, maxValue);
      }, 300);
    }
    
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, [minValue, maxValue, onPriceChange, isDragging]);

  const getValueFromPosition = useCallback((clientX: number) => {
    if (!sliderRef.current) return min;
    const rect = sliderRef.current.getBoundingClientRect();
    const percentage = Math.max(0, Math.min(1, (clientX - rect.left) / rect.width));
    const rawValue = min + percentage * (max - min);
    return Math.round(rawValue / step) * step;
  }, [min, max, step]);

  const updateValue = useCallback((clientX: number) => {
    if (rafRef.current) {
      cancelAnimationFrame(rafRef.current);
    }

    rafRef.current = requestAnimationFrame(() => {
      const newValue = getValueFromPosition(clientX);
      
      if (isDragging === 'min') {
        const clampedValue = Math.min(newValue, maxValue - step);
        if (clampedValue >= min) {
          setMinValue(clampedValue);
        }
      } else if (isDragging === 'max') {
        const clampedValue = Math.max(newValue, minValue + step);
        if (clampedValue <= max) {
          setMaxValue(clampedValue);
        }
      }
    });
  }, [isDragging, minValue, maxValue, min, max, step, getValueFromPosition]);

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!isDragging) return;
      e.preventDefault();
      updateValue(e.clientX);
    };

    const handleTouchMove = (e: TouchEvent) => {
      if (!isDragging) return;
      e.preventDefault();
      const touch = e.touches[0];
      updateValue(touch.clientX);
    };

    const handleEnd = () => {
      setIsDragging(null);
      if (rafRef.current) {
        cancelAnimationFrame(rafRef.current);
      }
    };

    if (isDragging) {
      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleEnd);
      document.addEventListener('touchmove', handleTouchMove, { passive: false });
      document.addEventListener('touchend', handleEnd);
      document.addEventListener('touchcancel', handleEnd);
    }

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleEnd);
      document.removeEventListener('touchmove', handleTouchMove);
      document.removeEventListener('touchend', handleEnd);
      document.removeEventListener('touchcancel', handleEnd);
    };
  }, [isDragging, updateValue]);

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat("vi-VN").format(value);
  };

  const handleReset = () => {
    setMinValue(defaultMinValue);
    setMaxValue(defaultMaxValue);
  };

  const leftPercent = ((minValue - min) / (max - min)) * 100;
  const rightPercent = ((maxValue - min) / (max - min)) * 100;

  return (
    <div
      className="relative bg-white border shadow-sm border-gray-200/80"
      style={{ 
        width,
        height,
        padding: '20px',
        borderRadius: '18px'
      }}
    >
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <h3 className="text-base font-semibold tracking-tight text-gray-900">
          Lọc theo giá
        </h3>
        <button
          onClick={handleReset}
          className="text-sm font-medium transition-all duration-200 text-violet-700 hover:text-violet-800 active:scale-95"
          style={{ borderRadius: '8px' }}
        >
          Đặt lại
        </button>
      </div>

      {/* Slider Container */}
      <div 
        ref={sliderRef}
        className="relative mb-10 select-none touch-none" 
        style={{ 
          height: `${sliderHeight * 6}px`, 
          cursor: isDragging ? 'grabbing' : 'default',
          willChange: isDragging ? 'transform' : 'auto'
        }}
      >
        {/* Track nền */}
        <div
          className="absolute -translate-y-1/2 bg-gray-200 top-1/2"
          style={{ 
            height: `${sliderHeight}px`, 
            left: 0, 
            right: 0, 
            zIndex: 1,
            borderRadius: `${sliderHeight / 2}px`
          }}
        />

        {/* Track active - Gradient tím đậm */}
        <div
          className="absolute -translate-y-1/2 shadow-lg top-1/2 bg-gradient-to-r from-violet-700 via-violet-800 to-violet-700 shadow-violet-800/40"
          style={{
            height: `${sliderHeight}px`,
            left: `${leftPercent}%`,
            right: `${100 - rightPercent}%`,
            zIndex: 2,
            borderRadius: `${sliderHeight / 2}px`,
            willChange: isDragging ? 'left, right' : 'auto'
          }}
        />

        {/* Min Thumb */}
        <div
          className={`absolute top-1/2 w-6 h-6 bg-white border-[3px] border-violet-800 cursor-grab active:cursor-grabbing shadow-md hover:shadow-lg hover:scale-105 ${
            isDragging === 'min' ? 'scale-110 !shadow-xl !shadow-violet-800/50 ring-[3px] ring-violet-800/25' : ''
          }`}
          style={{
            left: `${leftPercent}%`,
            transform: 'translate(-50%, -50%)',
            zIndex: isDragging === 'min' ? 10 : 5,
            transition: isDragging === 'min' ? 'none' : 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
            willChange: isDragging === 'min' ? 'transform' : 'auto',
            borderRadius: '50%'
          }}
          onMouseDown={(e) => {
            e.preventDefault();
            setIsDragging('min');
          }}
          onTouchStart={(e) => {
            e.preventDefault();
            setIsDragging('min');
          }}
        >
          {/* Inner dot */}
          <div 
            className="absolute w-2 h-2 -translate-x-1/2 -translate-y-1/2 top-1/2 left-1/2 bg-violet-800" 
            style={{ borderRadius: '50%' }}
          />
        </div>

        {/* Max Thumb */}
        <div
          className={`absolute top-1/2 w-6 h-6 bg-white border-[3px] border-violet-800 cursor-grab active:cursor-grabbing shadow-md hover:shadow-lg hover:scale-105 ${
            isDragging === 'max' ? 'scale-110 !shadow-xl !shadow-violet-800/50 ring-[3px] ring-violet-800/25' : ''
          }`}
          style={{
            left: `${rightPercent}%`,
            transform: 'translate(-50%, -50%)',
            zIndex: isDragging === 'max' ? 10 : 5,
            transition: isDragging === 'max' ? 'none' : 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)',
            willChange: isDragging === 'max' ? 'transform' : 'auto',
            borderRadius: '50%'
          }}
          onMouseDown={(e) => {
            e.preventDefault();
            setIsDragging('max');
          }}
          onTouchStart={(e) => {
            e.preventDefault();
            setIsDragging('max');
          }}
        >
          {/* Inner dot */}
          <div 
            className="absolute w-2 h-2 -translate-x-1/2 -translate-y-1/2 top-1/2 left-1/2 bg-violet-800" 
            style={{ borderRadius: '50%' }}
          />
        </div>
      </div>

      {/* Price Display */}
      <div className="flex items-center justify-center gap-3">
        <div 
          className="flex-1 px-4 py-3 text-center border bg-gradient-to-br from-violet-50 to-violet-100/60 border-violet-200/60"
          style={{ borderRadius: '14px' }}
        >
          <div className="mb-1 text-xs font-medium text-violet-700">Tối thiểu</div>
          <div className="text-lg font-semibold text-violet-900">
            {formatCurrency(minValue)}{currency}
          </div>
        </div>

        <div 
          className="flex items-center justify-center w-10 h-10 bg-violet-100"
          style={{ borderRadius: '10px' }}
        >
          <svg className="w-5 h-5 text-violet-800" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
          </svg>
        </div>

        <div 
          className="flex-1 px-4 py-3 text-center border bg-gradient-to-br from-violet-50 to-violet-100/60 border-violet-200/60"
          style={{ borderRadius: '14px' }}
        >
          <div className="mb-1 text-xs font-medium text-violet-700">Tối đa</div>
          <div className="text-lg font-semibold text-violet-900">
            {formatCurrency(maxValue)}{currency}
          </div>
        </div>
      </div>
    </div>
  );
};

export default DualSlider;