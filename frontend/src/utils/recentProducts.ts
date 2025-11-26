// src/utils/recentProducts.ts
export interface RecentProduct {
  id: number;
  name: string;
  image: string;
  originalPrice: number;
  discountedPrice?: number;
  slug: string;
  rating: number;
  totalReviews: number;
  category: string;
  viewedAt?: number;
}

const KEY = 'recent_products';
const MAX_ITEMS = 30;

export const addToRecentProducts = (product: RecentProduct) => {
  try {
    let list: RecentProduct[] = [];
    const raw = localStorage.getItem(KEY);
    if (raw) {
      list = JSON.parse(raw);
    }

    // Xóa nếu đã tồn tại
    list = list.filter(p => p.id !== product.id);

    // Thêm vào đầu
    list.unshift({ ...product, viewedAt: Date.now() });

    // Giữ tối đa 30
    if (list.length > MAX_ITEMS) {
      list = list.slice(0, MAX_ITEMS);
    }

    localStorage.setItem(KEY, JSON.stringify(list));
  } catch (err) {
    console.error('Lỗi lưu recent products:', err);
  }
};

export const getRecentProducts = (): RecentProduct[] => {
  try {
    const raw = localStorage.getItem(KEY);
    return raw ? JSON.parse(raw) : [];
  } catch (err) {
    console.error('Lỗi đọc recent products:', err);
    return [];
  }
};

export const clearRecentProducts = () => {
  localStorage.removeItem(KEY);
};