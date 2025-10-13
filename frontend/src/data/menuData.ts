// ==================== data/menuData.ts ====================
import { 
  Smartphone, 
  Headphones, 
  Battery, 
  Cable, 
  Sparkles, 
  TrendingUp, 
  Gift
} from 'lucide-react';
import { MenuItem } from '../types';

export const mockMenuData: MenuItem[] = [
  {
    title: 'Trang chủ',
    link: '#',
    noDropdown: true,
  },
  {
    title: 'Mua sắm',
    mega: true,
    categories: [
      {
        title: 'Bảo vệ',
        icon: Smartphone,
        items: [
          { name: 'Ốp lưng', link: '#' },
          { name: 'Bao da', link: '#' },
          { name: 'Miếng dán camera', link: '#' },
          { name: 'Túi chống nước', link: '#' },
          { name: 'Kính cường lực', link: '#' },
          { name: 'Dán PPF', link: '#' },
        ],
      },
      {
        title: 'Âm thanh',
        icon: Headphones,
        items: [
          { name: 'Loa Bluetooth', link: '#' },
          { name: 'Loa karaoke', link: '#' },
          { name: 'Tai nghe không dây', link: '#' },
          { name: 'Tai nghe có dây', link: '#' },
          { name: 'Mic cài áo', link: '#' },
          { name: 'Phụ kiện tai nghe', link: '#' },
          { name: 'Eartips & foam', link: '#' },
        ],
      },
      {
        title: 'Sạc & Pin',
        icon: Battery,
        items: [
          { name: 'Sạc nhanh', link: '#' },
          { name: 'Sạc không dây', link: '#' },
          { name: 'Pin dự phòng', link: '#' },
          { name: 'Cáp sạc', link: '#' },
          { name: 'Pin dự phòng magsafe', link: '#' },
          { name: 'Adapter đa năng', link: '#' },
        ],
      },
      {
        title: 'Phụ kiện khác',
        icon: Cable,
        items: [
          { name: 'Giá đỡ điện thoại', link: '#' },
          { name: 'Ví magsafe', link: '#' },
          { name: 'Gimbal chống rung', link: '#' },
          { name: 'Đồng hồ thông minh', link: '#' },
          { name: 'Thiết bị mạng', link: '#' },
          { name: 'Camera hành trình', link: '#' },
        ],
      },
      {
        title: 'Kết nối',
        icon: Sparkles,
        items: [
            { name: 'Cáp Lightning', link: '#' },
            { name: 'Cáp Type-C', link: '#' },
            { name: 'Cáp HDMI', link: '#' },
            { name: 'Đầu chuyển Type-C', link: '#' },
            { name: 'USB OTG', link: '#' },
            { name: 'Đầu đọc thẻ nhớ', link: '#' },
        ],
        },
    ],
  },
  {
    title: 'Blogs',
    link: '#',
    noDropdown: true,
  },
  {
    title: 'Flash Sales',
    icon: TrendingUp,
    items: [
      { name: 'Giảm giá hôm nay', link: '#', icon: Gift },
      { name: 'Deal hot trong tuần', link: '#', icon: TrendingUp },
      { name: 'Sản phẩm mới', link: '#', icon: Sparkles },
    ],
  },
  {
    title: 'Hỗ trợ',
    items: [
      { name: 'Về chúng tôi', link: '#' },
      { name: 'Liên hệ', link: '#' },
      { name: 'Hệ thống cửa hàng', link: '#' },
      { name: 'Chính sách bảo hành', link: '#' },
      { name: 'Hướng dẫn mua hàng', link: '#' },
    ],
  },
  {
    title: 'Cá nhân',
    items: [
      { name: 'Yêu thích', link: '#' },
      { name: 'Giỏ hàng', link: '#' },
      { name: 'Đơn hàng của bạn', link: '#' },
      { name: 'Profile cá nhân', link: '#' },
    ],
  },
];