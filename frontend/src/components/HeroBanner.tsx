import React, { useState, useEffect } from "react";

// Banner Data Type
interface BannerData {
  id: number;
  title: string;
  description: string;
  image: string;
  primaryButtonText: string;
  secondaryButtonText: string;
}

// Mock Data
const bannerData: BannerData[] = [
  {
    id: 1,
    title: 'Khám phá thế giới phụ kiện "chính hãng", chất lượng "vượt trội"',
    description:
      "Từ ốp lưng, kính cường lực đến sạc nhanh - tất cả đều có tại cửa hàng chúng tôi. Hàng mới cập nhật mỗi ngày, mẫu mã đa dạng.",
    image: "",
    primaryButtonText: "Mua ngay",
    secondaryButtonText: "Xem bộ sưu tập",
  },
  {
    id: 2,
    title: 'Phụ kiện "cao cấp" - bảo vệ "toàn diện" cho điện thoại của bạn',
    description:
      "Chọn ốp chống sốc, bao da sang trọng hay dán PPF chống trầy. Giúp điện thoại của bạn luôn bền đẹp như mới.",
    image: "",
    primaryButtonText: "Khám phá ngay",
    secondaryButtonText: "Xem chi tiết",
  },
  {
    id: 3,
    title: 'Âm thanh sống động - trải nghiệm "không giới hạn"',
    description:
      "Tận hưởng âm nhạc với tai nghe Bluetooth, loa di động và micro chất lượng cao. Kết nối dễ dàng, âm thanh chân thực.",
    image: "",
    primaryButtonText: "Mua tai nghe",
    secondaryButtonText: "Xem loa",
  },
  {
    id: 4,
    title: 'Sạc nhanh - năng lượng "luôn đầy" mọi lúc mọi nơi',
    description:
      "Sở hữu bộ sạc, pin dự phòng và cáp chính hãng. Tương thích mọi thiết bị, an toàn, bền bỉ, và sạc cực nhanh.",
    image: "",
    primaryButtonText: "Xem ngay",
    secondaryButtonText: "Khuyến mãi hôm nay",
  },
];


// Hero Banner Props
interface HeroBannerCarouselProps {
  width?: string | number;
  height?: string | number;
}

const HeroBannerCarousel: React.FC<HeroBannerCarouselProps> = ({
  width = "100%",
  height = "600px",
}) => {
  const [currentSlide, setCurrentSlide] = useState(0);
  const [isAutoPlaying, setIsAutoPlaying] = useState(true);
  const [touchStart, setTouchStart] = useState(0);
  const [touchEnd, setTouchEnd] = useState(0);

  const totalSlides = bannerData.length;

  // Chuẩn hóa giá trị width và height
  const normalizedWidth = typeof width === "number" ? `${width}px` : width;
  const normalizedHeight = typeof height === "number" ? `${height}px` : height;

  // Auto play
  useEffect(() => {
    if (!isAutoPlaying) return;

    const interval = setInterval(() => {
      setCurrentSlide((prev) => (prev + 1) % totalSlides);
    }, 5000);

    return () => clearInterval(interval);
  }, [isAutoPlaying, totalSlides]);

  const goToSlide = (index: number) => {
    setCurrentSlide(index);
    setIsAutoPlaying(false);
    setTimeout(() => setIsAutoPlaying(true), 10000);
  };

  // Handle touch events for mobile swipe
  const handleTouchStart = (e: React.TouchEvent) => {
    setTouchStart(e.targetTouches[0].clientX);
  };

  const handleTouchMove = (e: React.TouchEvent) => {
    setTouchEnd(e.targetTouches[0].clientX);
  };

  const handleTouchEnd = () => {
    if (!touchStart || !touchEnd) return;
    
    const distance = touchStart - touchEnd;
    const isLeftSwipe = distance > 50;
    const isRightSwipe = distance < -50;

    if (isLeftSwipe) {
      setCurrentSlide((prev) => (prev + 1) % totalSlides);
      setIsAutoPlaying(false);
      setTimeout(() => setIsAutoPlaying(true), 10000);
    }
    
    if (isRightSwipe) {
      setCurrentSlide((prev) => (prev - 1 + totalSlides) % totalSlides);
      setIsAutoPlaying(false);
      setTimeout(() => setIsAutoPlaying(true), 10000);
    }

    setTouchStart(0);
    setTouchEnd(0);
  };

  const currentBanner = bannerData[currentSlide];

  // Function to parse and render title with highlighted text in quotes
  const renderTitle = (title: string) => {
    const parts = title.split(/(".*?")/g);
    return parts.map((part, index) => {
      if (part.startsWith('"') && part.endsWith('"')) {
        return (
          <strong key={index} className="text-violet-800">
            {part.slice(1, -1)}
          </strong>
        );
      }
      return <span key={index}>{part}</span>;
    });
  };

  return (
    <section className="relative overflow-hidden bg-white shadow-lg" style={{ width: normalizedWidth }}>
      <div 
        className="relative w-full bg-white" 
        style={{ height: normalizedHeight }}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
      >
        {/* Background - White */}
        <div className="absolute inset-0 bg-white" />

        {/* Content */}
        <div className="relative z-10 flex items-center justify-center h-full max-w-screen-xl px-4 py-16 mx-auto sm:px-6 lg:px-8">
          <div className="max-w-3xl mx-auto text-center">
            <h1 className="text-3xl font-bold text-gray-900 sm:text-4xl lg:text-5xl" style={{ fontFamily: 'Roboto, sans-serif' }}>
              {renderTitle(currentBanner.title)}
            </h1>
            
            <p className="mt-6 text-base text-gray-700 sm:text-lg lg:text-xl" style={{ fontFamily: 'Roboto, sans-serif' }}>
              {currentBanner.description}
            </p>

            <div className="flex flex-col justify-center gap-4 mt-8 sm:flex-row sm:gap-6">
              <a
                href="#"
                className="inline-block px-8 py-3 font-medium text-white transition-all border shadow-lg bg-violet-800 border-violet-800 rounded-xl hover:bg-violet-900 focus:outline-none focus:ring-4 focus:ring-violet-300"
                style={{ fontFamily: 'Roboto, sans-serif' }}
              >
                {currentBanner.primaryButtonText}
              </a>
              <a
                href="#"
                className="inline-block px-8 py-3 font-medium transition-all bg-white border border-gray-300 text-violet-800 rounded-xl hover:bg-gray-50 hover:border-violet-800 focus:outline-none focus:ring-4 focus:ring-violet-300"
                style={{ fontFamily: 'Roboto, sans-serif' }}
              >
                {currentBanner.secondaryButtonText}
              </a>
            </div>
          </div>
        </div>

        {/* Dot Pagination */}
        <div className="absolute z-20 flex gap-4 transform -translate-x-1/2 bottom-8 left-1/2">
          {bannerData.map((_, index) => (
            <button
              key={index}
              onClick={() => goToSlide(index)}
              className={`h-3 rounded-full transition-all focus:outline-none focus:ring-2 focus:ring-violet-800/50 ${
                index === currentSlide
                  ? "bg-violet-800 w-12"
                  : "bg-gray-300 hover:bg-violet-600 w-3"
              }`}
              aria-label={`Go to slide ${index + 1}`}
            />
          ))}
        </div>
      </div>
    </section>
  );
};

export default HeroBannerCarousel;