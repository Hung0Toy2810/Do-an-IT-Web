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
    title: 'Khám phá thế giới tri thức "không giới hạn" với công nghệ AI tiên tiến',
    description:
      "Trải nghiệm nền tảng học tập thông minh với hàng ngàn khóa học chất lượng cao. Học mọi lúc, mọi nơi với phương pháp cá nhân hóa.",
    image: "https://images.unsplash.com/photo-1522202176988-66273c2fd55f?w=1200&h=600&fit=crop",
    primaryButtonText: "Bắt đầu ngay",
    secondaryButtonText: "Tìm hiểu thêm",
  },
  {
    id: 2,
    title: 'Nâng cao kỹ năng "chuyên nghiệp" cùng đội ngũ giảng viên hàng đầu',
    description:
      "Được hướng dẫn bởi các chuyên gia hàng đầu trong ngành. Chứng chỉ được công nhận toàn cầu và cơ hội việc làm hấp dẫn.",
    image: "https://images.unsplash.com/photo-1522202176988-66273c2fd55f?w=1200&h=600&fit=crop",
    primaryButtonText: "Xem khóa học",
    secondaryButtonText: "Liên hệ tư vấn",
  },
  {
    id: 3,
    title: 'Cộng đồng học tập "sôi động" và nhiệt huyết',
    description:
      "Kết nối với hàng nghìn học viên trên toàn quốc. Chia sẻ kiến thức, kinh nghiệm và cùng nhau phát triển mỗi ngày.",
    image: "https://images.unsplash.com/photo-1522202176988-66273c2fd55f?w=1200&h=600&fit=crop",
    primaryButtonText: "Tham gia ngay",
    secondaryButtonText: "Khám phá",
  },
  {
    id: 4,
    title: 'Học tập linh hoạt "hiệu quả" theo lộ trình cá nhân',
    description:
      "Tự do lựa chọn thời gian và tốc độ học phù hợp. Hệ thống theo dõi tiến độ thông minh giúp bạn đạt mục tiêu nhanh chóng.",
    image: "https://images.unsplash.com/photo-1522202176988-66273c2fd55f?w=1200&h=600&fit=crop",
    primaryButtonText: "Dùng thử miễn phí",
    secondaryButtonText: "Xem demo",
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