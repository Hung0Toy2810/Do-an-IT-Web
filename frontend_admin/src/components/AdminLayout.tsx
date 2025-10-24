import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation, Outlet } from 'react-router-dom';
import {
  User, 
  Package, 
  ShoppingCart, 
  Users, 
  BarChart3, 
  FileText, 
  LogOut,
  Monitor,
  ChevronLeft,
  ChevronRight,
  Menu,
  Shield,
  X
} from 'lucide-react';
import { deleteCookie } from '../utils/cookies';
import { isTokenValid } from '../utils/auth';
import { notify } from '../utils/notify';
import { api } from '../utils/apiHandler';

interface AdminInfo {
  username: string;
}

interface ApiResponse<T> {
  data?: T;
  message?: string;
}

interface MenuItem {
  id: string;
  label: string;
  icon: React.ReactNode;
  path: string;
}

export default function AdminLayout() {
  const [adminInfo, setAdminInfo] = useState<AdminInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  const menuItems: MenuItem[] = [
    { id: 'products', label: 'Quản lý sản phẩm', icon: <Package className="w-5 h-5" />, path: '/admin/products' },
    { id: 'orders', label: 'Quản lý đơn hàng', icon: <ShoppingCart className="w-5 h-5" />, path: '/admin/orders' },
    { id: 'customers', label: 'Quản lý khách hàng', icon: <Users className="w-5 h-5" />, path: '/admin/customers' },
    { id: 'reports', label: 'Báo cáo và phân tích', icon: <BarChart3 className="w-5 h-5" />, path: '/admin/reports' },
    { id: 'content', label: 'Quản lý nội dung', icon: <FileText className="w-5 h-5" />, path: '/admin/content' },
    { id: 'admins', label: 'Quản lý quản trị viên', icon: <Shield className="w-5 h-5" />, path: '/admin/admins' },
  ];

  useEffect(() => {
    checkAuthAndFetchAdmin();
  }, []);

  const checkAuthAndFetchAdmin = async () => {
    if (!isTokenValid()) {
      deleteCookie('auth_token');
      notify.error('Phiên đăng nhập đã hết hạn');
      navigate('/admin-login');
      return;
    }

    try {
      const { ok, data } = await api.get('http://localhost:5067/api/administrators/me') as {
        ok: boolean;
        data: ApiResponse<AdminInfo>;
      };

      if (ok && data.data) {
        setAdminInfo({ username: data.data.username || 'Admin' });
      }
    } catch (error) {
      console.error('Error fetching admin info:', error);
      notify.error('Không thể kết nối đến server');
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      const { ok, data } = await api.post('http://localhost:5067/api/administrators/logout') as {
        ok: boolean;
        data: ApiResponse<never>;
      };

      if (ok) {
        deleteCookie('auth_token');
        notify.success(data.message || 'Đăng xuất thành công');
        setTimeout(() => navigate('/admin-login'), 500);
      } else {
        notify.error(data.message || 'Đăng xuất thất bại');
      }
    } catch (error) {
      console.error('Logout error:', error);
      notify.error('Không thể kết nối đến server');
    }
  };

  const handleLogoutOtherDevices = async () => {
    try {
      const { ok, data } = await api.post('http://localhost:5067/api/administrators/logout/other-devices') as {
        ok: boolean;
        data: ApiResponse<never>;
      };

      if (ok) {
        notify.success(data.message || 'Đã đăng xuất tất cả thiết bị khác');
      } else {
        notify.error(data.message || 'Không thể đăng xuất các thiết bị khác');
      }
    } catch (error) {
      console.error('Logout other devices error:', error);
      notify.error('Không thể kết nối đến server');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
        <div className="flex flex-col items-center gap-4">
          <svg className="w-12 h-12 text-violet-700 animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <p className="text-sm font-medium text-violet-700">Đang tải...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
      {/* Mobile Menu Button */}
      <button
        onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
        className="fixed z-50 p-3 text-white transition-all shadow-lg top-4 left-4 lg:hidden bg-gradient-to-br from-violet-600 to-violet-800 hover:shadow-xl"
        style={{ borderRadius: '12px' }}
      >
        {mobileMenuOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
      </button>

      {/* Sidebar */}
      <aside
        className={`
          fixed lg:static inset-y-0 left-0 z-40
          ${sidebarCollapsed ? 'lg:w-20' : 'lg:w-72'}
          ${mobileMenuOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'}
          w-72 bg-white border-r border-violet-100 shadow-xl
          transition-all duration-300 ease-in-out
        `}
      >
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="flex items-center justify-between p-4 border-b border-violet-100">
            {!sidebarCollapsed && (
              <div className="flex items-center gap-3">
                <div 
                  className="flex items-center justify-center w-10 h-10 shadow-md bg-gradient-to-br from-violet-600 to-violet-800"
                  style={{ borderRadius: '12px' }}
                >
                  <User className="w-5 h-5 text-white" />
                </div>
                <div>
                  <h2 className="text-sm font-bold text-gray-900">Quản trị viên</h2>
                  <p className="text-xs font-medium text-violet-600">{adminInfo?.username}</p>
                </div>
              </div>
            )}
            <button
              onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
              className="hidden p-2 transition-colors rounded-lg lg:block hover:bg-violet-50"
            >
              {sidebarCollapsed ? (
                <ChevronRight className="w-5 h-5 text-violet-700" />
              ) : (
                <ChevronLeft className="w-5 h-5 text-violet-700" />
              )}
            </button>
          </div>

          {/* Menu Items */}
          <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
            {menuItems.map((item) => (
              <button
                key={item.id}
                onClick={() => {
                  navigate(item.path);
                  setMobileMenuOpen(false);
                }}
                className={`
                  w-full flex items-center gap-3 px-3 py-3 rounded-xl
                  transition-all font-medium text-sm
                  ${location.pathname === item.path
                    ? 'bg-gradient-to-r from-violet-600 to-violet-700 text-white shadow-lg'
                    : 'text-gray-700 hover:bg-violet-50 hover:text-violet-700'
                  }
                  ${sidebarCollapsed ? 'justify-center' : ''}
                `}
              >
                {item.icon}
                {!sidebarCollapsed && <span>{item.label}</span>}
              </button>
            ))}
          </nav>

          {/* Logout Buttons */}
          <div className="p-3 space-y-2 border-t border-violet-100">
            <button
              onClick={handleLogoutOtherDevices}
              className={`
                w-full flex items-center gap-3 px-3 py-2.5 rounded-xl
                text-sm font-medium text-orange-700 hover:bg-orange-50
                transition-all
                ${sidebarCollapsed ? 'justify-center' : ''}
              `}
            >
              <Monitor className="w-5 h-5" />
              {!sidebarCollapsed && <span>Đăng xuất thiết bị khác</span>}
            </button>
            <button
              onClick={handleLogout}
              className={`
                w-full flex items-center gap-3 px-3 py-2.5 rounded-xl
                text-sm font-medium text-red-700 hover:bg-red-50
                transition-all
                ${sidebarCollapsed ? 'justify-center' : ''}
              `}
            >
              <LogOut className="w-5 h-5" />
              {!sidebarCollapsed && <span>Đăng xuất</span>}
            </button>
          </div>
        </div>
      </aside>

      {/* Mobile Overlay */}
      {mobileMenuOpen && (
        <div
          onClick={() => setMobileMenuOpen(false)}
          className="fixed inset-0 z-30 bg-black/30 lg:hidden"
        />
      )}

      {/* Main Content */}
      <main className="flex-1 p-4 lg:p-8">
        <div className="mx-auto max-w-7xl">
          <Outlet />
        </div>
      </main>
    </div>
  );
}