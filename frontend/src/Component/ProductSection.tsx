import React, { useState, useEffect } from "react";
import { Heart, ShoppingCart, Star, ChevronRight } from "lucide-react";

// Product Type
interface Product {
  id: number;
  name: string;
  image: string;
  originalPrice: number;
  discountedPrice?: number;
  discountPercentage?: number;
  rating: number;
  totalReviews: number;
  category: string;
}

// Product Card Props
interface ProductCardProps {
  product: Product;
  onAddToCart?: (product: Product) => void;
  onToggleFavorite?: (productId: number) => void;
  isFavorite?: boolean;
}

const ProductCard: React.FC<ProductCardProps> = ({
  product,
  onAddToCart,
  onToggleFavorite,
  isFavorite = false,
}) => {
  const [isLiked, setIsLiked] = useState(isFavorite);

  const handleToggleFavorite = () => {
    setIsLiked(!isLiked);
    onToggleFavorite?.(product.id);
  };

  const handleAddToCart = () => {
    onAddToCart?.(product);
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(price);
  };

  return (
    <div
      className="relative flex flex-col overflow-hidden transition-all bg-white border border-gray-200 shadow-sm rounded-2xl group hover:shadow-lg"
      style={{ fontFamily: "Roboto, sans-serif" }}
    >
      {/* Image Container */}
      <div className="relative overflow-hidden bg-gray-100 aspect-square">
        <img
          src={product.image}
          alt={product.name}
          className="object-cover w-full h-full transition-transform duration-300 group-hover:scale-110"
        />

        {/* Discount Label */}
        {product.discountPercentage && (
          <div className="absolute px-1.5 py-0.5 text-xs font-bold text-white rounded-xl top-1.5 left-1.5 bg-violet-800">
            -{product.discountPercentage}%
          </div>
        )}

        {/* Favorite Button */}
        <button
          onClick={handleToggleFavorite}
          className="absolute p-1 transition-all bg-white border border-gray-200 shadow-sm top-1.5 right-1.5 rounded-md hover:bg-violet-50 focus:outline-none focus:ring-1 focus:ring-violet-800/20"
          aria-label={isLiked ? "Remove from favorites" : "Add to favorites"}
        >
          <Heart
            className={`w-3.5 h-3.5 transition-colors ${
              isLiked
                ? "fill-violet-800 text-violet-800"
                : "text-gray-400 hover:text-violet-800"
            }`}
          />
        </button>
      </div>

      {/* Content - Flex Layout */}
      <div className="flex flex-col flex-1 p-2.5">
        {/* Top Content */}
        <div className="flex-shrink-0">
          {/* Category */}
          <p className="text-xs font-medium text-gray-500 uppercase">
            {product.category}
          </p>

          {/* Product Name */}
          <h3 className="mt-1 text-xs font-semibold leading-tight text-gray-900 line-clamp-2">
            {product.name}
          </h3>

          {/* Rating */}
          <div className="flex items-center gap-1 mt-1.5">
            <div className="flex items-center gap-0.5">
              {[...Array(5)].map((_, index) => (
                <Star
                  key={index}
                  className={`w-2.5 h-2.5 ${
                    index < Math.floor(product.rating)
                      ? "fill-yellow-400 text-yellow-400"
                      : "text-gray-300"
                  }`}
                />
              ))}
            </div>
            <span className="text-xs text-gray-600">
              {product.rating}
            </span>
          </div>
        </div>

        {/* Spacer */}
        <div className="flex-grow"></div>

        {/* Bottom Content */}
        <div className="flex-shrink-0">
          {/* Price */}
          <div className="flex items-center gap-1.5 mt-2">
            {product.discountedPrice ? (
              <>
                <span className="text-xs font-bold text-violet-800">
                  {formatPrice(product.discountedPrice)}
                </span>
                <span className="text-[10px] text-gray-500 line-through">
                  {formatPrice(product.originalPrice)}
                </span>
              </>
            ) : (
              <span className="text-xs font-bold text-gray-900">
                {formatPrice(product.originalPrice)}
              </span>
            )}
          </div>


          {/* Add to Cart Button */}
          <button
            onClick={handleAddToCart}
            className="flex items-center justify-center w-full gap-1 px-2 py-1 mt-2 text-sm font-medium text-white transition-all border shadow-sm bg-violet-800 border-violet-800 rounded-xl hover:bg-violet-900 focus:outline-none focus:ring-2 focus:ring-violet-300"
          >
            <ShoppingCart className="w-3 h-3" />
            Thêm vào giỏ
          </button>
        </div>
      </div>
    </div>
  );
};

// Product Section Container Props
interface ProductSectionProps {
  title: string;
  products: Product[];
  onAddToCart?: (product: Product) => void;
  onToggleFavorite?: (productId: number) => void;
  favorites?: Set<number>;
  onViewMore?: () => void;
  itemsPerPage?: number;
}

const ProductSection: React.FC<ProductSectionProps> = ({
  title,
  products,
  onAddToCart,
  onToggleFavorite,
  favorites = new Set(),
  onViewMore,
}) => {
  const [currentPage, setCurrentPage] = useState(0);
  const [cardsPerView, setCardsPerView] = useState(5);

  // Responsive cards per view
  useEffect(() => {
    const updateCardsPerView = () => {
      const width = window.innerWidth;
      if (width < 768) {
        // Mobile
        setCardsPerView(2);
      } else if (width < 1024) {
        // Tablet
        setCardsPerView(3);
      } else if (width < 1280) {
        // Small Desktop
        setCardsPerView(4);
      } else {
        // Large Desktop
        setCardsPerView(5);
      }
    };

    updateCardsPerView();
    window.addEventListener('resize', updateCardsPerView);
    return () => window.removeEventListener('resize', updateCardsPerView);
  }, []);

  const totalPages = Math.ceil(products.length / cardsPerView);
  const startIndex = currentPage * cardsPerView;
  const endIndex = startIndex + cardsPerView;
  const currentProducts = products.slice(startIndex, endIndex);

  const goToPage = (pageIndex: number) => {
    setCurrentPage(pageIndex);
  };

  // Reset to first page when cards per view changes
  useEffect(() => {
    setCurrentPage(0);
  }, [cardsPerView]);

  return (
    <div
      className="w-full p-4 bg-white shadow-lg rounded-2xl"
      style={{ fontFamily: "Roboto, sans-serif" }}
    >
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-bold text-gray-900">{title}</h2>
        {onViewMore && (
          <button
            onClick={onViewMore}
            className="flex items-center gap-1 px-3 py-1.5 text-xs font-medium transition-all border text-violet-800 border-violet-800 rounded-lg hover:bg-violet-50 focus:outline-none focus:ring-2 focus:ring-violet-800/20"
          >
            Xem thêm
            <ChevronRight className="w-3.5 h-3.5" />
          </button>
        )}
      </div>

      {/* Product Row - Single Row Only */}
      <div className={`grid gap-3 ${
        cardsPerView === 2 ? 'grid-cols-2' :
        cardsPerView === 3 ? 'grid-cols-3' :
        cardsPerView === 4 ? 'grid-cols-4' :
        'grid-cols-5'
      }`}>
        {currentProducts.map((product) => (
          <ProductCard
            key={product.id}
            product={product}
            onAddToCart={onAddToCart}
            onToggleFavorite={onToggleFavorite}
            isFavorite={favorites.has(product.id)}
          />
        ))}
      </div>

      {/* Dot Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-4 mt-4">
          {[...Array(totalPages)].map((_, index) => (
            <button
              key={index}
              onClick={() => goToPage(index)}
              className={`h-3 rounded-full transition-all focus:outline-none focus:ring-2 focus:ring-violet-800/50 ${
                index === currentPage
                  ? "bg-violet-800 w-12"
                  : "bg-gray-300 hover:bg-violet-600 w-3"
              }`}
              aria-label={`Go to page ${index + 1}`}
            />
          ))}
        </div>
      )}
    </div>
  );
};

// Demo Component with Mock Data
const ProductSectionDemo: React.FC = () => {
  const [allProducts, setAllProducts] = useState<Product[]>([]);
  const [favorites, setFavorites] = useState<Set<number>>(new Set());
  const [loading, setLoading] = useState(true);

  // Mock API fetch
  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      
      // Simulate API delay
      await new Promise((resolve) => setTimeout(resolve, 1000));

      // Mock data - 12 products
      const mockProducts: Product[] = [
        {
          id: 1,
          name: "Laptop Dell XPS 13 - Intel Core i7 Gen 12, RAM 16GB, SSD 512GB",
          image: "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=400&h=400&fit=crop",
          originalPrice: 35000000,
          discountedPrice: 29000000,
          discountPercentage: 17,
          rating: 4.8,
          totalReviews: 256,
          category: "Laptop",
        },
        {
          id: 2,
          name: "iPhone 15 Pro Max 256GB - Titan Tự Nhiên",
          image: "https://images.unsplash.com/photo-1592286927505-67dff0a6b368?w=400&h=400&fit=crop",
          originalPrice: 34990000,
          discountedPrice: 31990000,
          discountPercentage: 9,
          rating: 4.9,
          totalReviews: 512,
          category: "Điện thoại",
        },
        {
          id: 3,
          name: "Tai nghe Sony WH-1000XM5 - Chống ồn chủ động",
          image: "https://images.unsplash.com/photo-1618366712010-f4ae9c647dcf?w=400&h=400&fit=crop",
          originalPrice: 8990000,
          discountedPrice: 7490000,
          discountPercentage: 17,
          rating: 4.7,
          totalReviews: 189,
          category: "Phụ kiện",
        },
        {
          id: 4,
          name: "Samsung Galaxy Watch 6 Classic - 43mm, Bluetooth",
          image: "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&h=400&fit=crop",
          originalPrice: 9990000,
          discountedPrice: 7990000,
          discountPercentage: 20,
          rating: 4.6,
          totalReviews: 143,
          category: "Đồng hồ thông minh",
        },
        {
          id: 5,
          name: "iPad Air M2 - 11 inch, Wi-Fi, 128GB",
          image: "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=400&h=400&fit=crop",
          originalPrice: 16990000,
          discountedPrice: 14990000,
          discountPercentage: 12,
          rating: 4.8,
          totalReviews: 321,
          category: "Máy tính bảng",
        },
        {
          id: 6,
          name: "Bàn phím cơ Keychron K8 Pro - RGB, Hot-swap",
          image: "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400&h=400&fit=crop",
          originalPrice: 3290000,
          discountedPrice: 2790000,
          discountPercentage: 15,
          rating: 4.5,
          totalReviews: 87,
          category: "Phụ kiện",
        },
        {
          id: 7,
          name: "MacBook Pro 14 M3 Pro - 18GB RAM, 512GB SSD",
          image: "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400&h=400&fit=crop",
          originalPrice: 52990000,
          discountedPrice: 49990000,
          discountPercentage: 6,
          rating: 4.9,
          totalReviews: 423,
          category: "Laptop",
        },
        {
          id: 8,
          name: "Samsung Galaxy S24 Ultra 512GB - Titan Gray",
          image: "https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?w=400&h=400&fit=crop",
          originalPrice: 33990000,
          discountedPrice: 29990000,
          discountPercentage: 12,
          rating: 4.8,
          totalReviews: 367,
          category: "Điện thoại",
        },
        {
          id: 9,
          name: "AirPods Pro 2 - USB-C, Chống ồn chủ động",
          image: "https://images.unsplash.com/photo-1606841837239-c5a1a4a07af7?w=400&h=400&fit=crop",
          originalPrice: 6490000,
          rating: 4.7,
          totalReviews: 234,
          category: "Phụ kiện",
        },
        {
          id: 10,
          name: "Chuột Logitech MX Master 3S - Wireless, Bluetooth",
          image: "https://images.unsplash.com/photo-1527814050087-3793815479db?w=400&h=400&fit=crop",
          originalPrice: 2690000,
          discountedPrice: 2290000,
          discountPercentage: 15,
          rating: 4.6,
          totalReviews: 156,
          category: "Phụ kiện",
        },
        {
          id: 11,
          name: "Monitor LG UltraGear 27 4K - 144Hz, HDR400",
          image: "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400&h=400&fit=crop",
          originalPrice: 12990000,
          discountedPrice: 10990000,
          discountPercentage: 15,
          rating: 4.7,
          totalReviews: 198,
          category: "Màn hình",
        },
        {
          id: 12,
          name: "Nintendo Switch OLED - Neon Blue/Red",
          image: "https://images.unsplash.com/photo-1578303512597-81e6cc155b3e?w=400&h=400&fit=crop",
          originalPrice: 8990000,
          discountedPrice: 7990000,
          discountPercentage: 11,
          rating: 4.8,
          totalReviews: 289,
          category: "Gaming",
        },
      ];

      setAllProducts(mockProducts);
      setLoading(false);
    };

    fetchProducts();
  }, []);

  const handleAddToCart = (product: Product) => {
    alert(`Đã thêm "${product.name}" vào giỏ hàng!`);
  };

  const handleToggleFavorite = (productId: number) => {
    setFavorites((prev) => {
      const newFavorites = new Set(prev);
      if (newFavorites.has(productId)) {
        newFavorites.delete(productId);
      } else {
        newFavorites.add(productId);
      }
      return newFavorites;
    });
  };

  const handleViewMore = (section: string) => {
    alert(`Xem thêm ${section}!`);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-lg font-medium text-gray-600" style={{ fontFamily: "Roboto, sans-serif" }}>
          Đang tải sản phẩm...
        </div>
      </div>
    );
  }

  // Split products into different sections
  const flashSaleProducts = allProducts.slice(0, 8);
  const featuredProducts = allProducts.slice(4, 12);
  const recommendedProducts = allProducts.slice(0, 6);

  return (
    <div className="min-h-screen p-3 bg-gray-50">
      <div className="mx-auto space-y-4 max-w-7xl">
        {/* Flash Sale Section */}
        <ProductSection
          title="Flash Sale"
          products={flashSaleProducts}
          onAddToCart={handleAddToCart}
          onToggleFavorite={handleToggleFavorite}
          favorites={favorites}
          onViewMore={() => handleViewMore("Flash Sale")}
          itemsPerPage={4}
        />

        {/* Featured Products Section */}
        <ProductSection
          title="Sản phẩm nổi bật"
          products={featuredProducts}
          onAddToCart={handleAddToCart}
          onToggleFavorite={handleToggleFavorite}
          favorites={favorites}
          onViewMore={() => handleViewMore("Sản phẩm nổi bật")}
          itemsPerPage={4}
        />

        {/* Recommended Section */}
        <ProductSection
          title="Gợi ý cho bạn"
          products={recommendedProducts}
          onAddToCart={handleAddToCart}
          onToggleFavorite={handleToggleFavorite}
          favorites={favorites}
          onViewMore={() => handleViewMore("Gợi ý cho bạn")}
          itemsPerPage={4}
        />
      </div>
    </div>
  );
};

export default ProductSectionDemo;