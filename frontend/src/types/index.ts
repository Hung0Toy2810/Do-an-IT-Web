// ==================== types/index.ts ====================
export type NotificationType = 'success' | 'error' | 'warning' | 'info';
export type PageType = 'home' | 'login' | 'register' | 'profile';

export interface Notification {
  id: number;
  type: NotificationType;
  message: string;
}

export interface NavItem {
  name: string;
  link: string;
  icon?: React.ElementType;
}

export interface Category {
  title: string;
  icon: React.ElementType;
  items: NavItem[];
}

export interface MenuItem {
  title: string;
  link?: string;
  noDropdown?: boolean;
  mega?: boolean;
  categories?: Category[];
  items?: NavItem[];
  icon?: React.ElementType;
}