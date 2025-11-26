// src/components/HeaderAndNavbar.tsx
import React, { useState, useRef, useCallback, useEffect } from 'react';
import {
  ChevronDown,
  Menu,
  X,
  ShoppingCart,
  User,
  Search as SearchIcon,
  Loader2,
} from 'lucide-react';
import { useNavigate, useLocation } from 'react-router-dom';
import axios from 'axios';
import { isTokenValid } from '../utils/auth';
import { mockMenuData } from '../data/menuData';

interface SubCategory {
  id: number;
  name: string;
  slug: string;
}

interface Category {
  id: number;
  name: string;
  slug: string;
  subCategories: SubCategory[];
}

export default function HeaderAndNavbar() {
  const [activeMenu, setActiveMenu] = useState<string | null>(null);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [expandedMobileMenu, setExpandedMobileMenu] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [cartCount] = useState(3);
  const [apiCategories, setApiCategories] = useState<Category[]>([]);
  const [loadingApi, setLoadingApi] = useState(true);
  const [apiError, setApiError] = useState(false);

  const leaveTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const navigate = useNavigate();
  const location = useLocation();

  /* ==================== FETCH API ==================== */
  useEffect(() => {
    const fetch = async () => {
      try {
        setLoadingApi(true);
        const { data } = await axios.get('http://localhost:5067/api/categories/with-subcategories');
        setApiCategories(data?.data || []);
        setApiError(false);
      } catch (err) {
        console.error('Lỗi tải danh mục:', err);
        setApiError(true);
      } finally {
        setLoadingApi(false);
      }
    };
    fetch();
  }, []);

  /* ==================== EFFECTS ==================== */
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const q = params.get('q');
    if (location.pathname === '/search' && q) setSearchQuery(q);
    else setSearchQuery('');
  }, [location]);

  useEffect(() => {
    document.body.style.overflow = isMobileMenuOpen ? 'hidden' : '';
    return () => { document.body.style.overflow = ''; };
  }, [isMobileMenuOpen]);

  /* ==================== HANDLERS ==================== */
  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = searchQuery.trim();
    if (!trimmed) return;
    setIsMobileMenuOpen(false);
    navigate(`/search?q=${encodeURIComponent(trimmed)}`);
  };

  const clearSearch = () => {
    setSearchQuery('');
    if (location.pathname === '/search') navigate('/search');
  };

  const handleMouseEnter = useCallback((title: string) => {
    if (leaveTimeoutRef.current) clearTimeout(leaveTimeoutRef.current);
    setActiveMenu(title);
  }, []);

  const handleMouseLeave = useCallback(() => {
    leaveTimeoutRef.current = setTimeout(() => setActiveMenu(null), 250);
  }, []);

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(p => !p);
    setExpandedMobileMenu(null);
    setActiveMenu(null);
  };

  const toggleMobileSubmenu = (title: string) => {
    setExpandedMobileMenu(p => (p === title ? null : title));
  };

  const handleSubCatClick = (slug: string) => {
    setIsMobileMenuOpen(false);
    setActiveMenu(null);
    navigate(`/subcategory/${slug}`);
  };

  const handleUserClick = () => navigate(isTokenValid() ? '/profile' : '/login');

  const handleMovetoCart = () => navigate('/cart');

  /* ==================== MENU DATA ==================== */
  const shoppingMenu = {
    title: 'Mua sắm',
    mega: true,
    categories: apiCategories.map(cat => ({
      title: cat.name,
      items: cat.subCategories.map(sub => ({
        name: sub.name,
        slug: sub.slug,
      })),
    })),
  };

  const staticMenus = mockMenuData.filter(m => m.title !== 'Mua sắm');
  const orderedMenus = [
    staticMenus.find(m => m.title === 'Trang chủ')!,
    shoppingMenu,
    staticMenus.find(m => m.title === 'Blogs')!,
    staticMenus.find(m => m.title === 'Flash Sales')!,
    staticMenus.find(m => m.title === 'Hỗ trợ')!,
    staticMenus.find(m => m.title === 'Cá nhân')!,
  ].filter(Boolean);

  /* ==================== RENDER ==================== */
  return (
    <>
      {/* ==================== HEADER ==================== */}
      <header className="sticky top-0 z-50 bg-white shadow-sm">
        <div className="border-b border-gray-200">
          <div className="max-w-screen-xl px-4 mx-auto sm:px-6 lg:px-8">
            <div className="flex items-center justify-between h-16">
              <a href="/" className="text-2xl font-bold text-violet-900">PhoneCare</a>

              {/* Desktop Search & Icons */}
              <div className="items-center justify-end flex-1 hidden gap-3 ml-6 md:flex">
                <form onSubmit={handleSearch} className="relative flex-1 max-w-xl">
                  <SearchIcon className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 left-3 top-1/2" />
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={e => setSearchQuery(e.target.value)}
                    placeholder="Tìm kiếm sản phẩm..."
                    className="w-full pl-10 pr-10 py-2.5 text-sm bg-gray-100 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-violet-600/20 focus:border-violet-600 transition-all"
                  />
                  {searchQuery && (
                    <button type="button" onClick={clearSearch} className="absolute p-1 text-gray-400 -translate-y-1/2 right-9 top-1/2 hover:text-gray-600">
                      <X className="w-4 h-4" />
                    </button>
                  )}
                  <button type="submit" className="absolute right-2 top-1/2 -translate-y-1/2 p-1.5 text-violet-700 hover:text-violet-900">
                    <SearchIcon className="w-5 h-5" />
                  </button>
                </form>
                <button onClick={handleMovetoCart} className="relative p-2.5 text-gray-700 hover:text-violet-700 hover:bg-violet-50 rounded-xl transition-all">
                  <ShoppingCart className="w-5 h-5" />
                </button>
                <button onClick={handleUserClick} className="p-2.5 text-gray-700 hover:text-violet-700 hover:bg-violet-50 rounded-xl transition-all">
                  <User className="w-5 h-5" />
                </button>
              </div>

              {/* Mobile Toggle */}
              <button onClick={toggleMobileMenu} className="md:hidden p-2.5 text-gray-700">
                {isMobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
              </button>
            </div>
          </div>
        </div>

        {/* ==================== NAVBAR – CHỈ HIỆN TRÊN DESKTOP (md+) ==================== */}
        <nav className="hidden text-white md:block bg-gradient-to-r from-violet-700 to-violet-800">
          <div className="max-w-screen-xl px-4 mx-auto sm:px-6 lg:px-8">
            <ul className="flex items-center justify-center gap-1">
              {loadingApi ? (
                <li className="flex items-center h-12 px-6"><Loader2 className="w-5 h-5 animate-spin" /></li>
              ) : apiError ? (
                <li className="h-12 px-6 text-sm">Lỗi tải danh mục</li>
              ) : (
                orderedMenus.map((menu, idx) => (
                  <li
                    key={idx}
                    className="relative"
                    onMouseEnter={() => handleMouseEnter(menu.title)}
                    onMouseLeave={handleMouseLeave}
                  >
                    {menu.noDropdown ? (
                      <a href={menu.link} className="flex items-center h-12 px-5 text-sm font-medium transition-all hover:bg-white/10 rounded-xl">
                        {menu.title}
                      </a>
                    ) : (
                      <button className="flex items-center h-12 gap-1.5 px-5 text-sm font-medium hover:bg-white/10 rounded-xl transition-all">
                        {menu.title}
                        <ChevronDown className={`w-3.5 h-3.5 transition-transform ${activeMenu === menu.title ? 'rotate-180' : ''}`} />
                      </button>
                    )}

                    {/* NORMAL DROPDOWN */}
                    {activeMenu === menu.title && !menu.mega && (
                      <div
                        className="absolute left-0 top-full mt-1 w-auto min-w-[180px] bg-white text-gray-800 shadow-lg border border-gray-100 rounded-xl overflow-hidden z-50"
                        onMouseEnter={() => handleMouseEnter(menu.title)}
                        onMouseLeave={handleMouseLeave}
                      >
                        <div className="py-1">
                          {menu.items?.map((it: any, iIdx: number) => {
                            const Icon = it.icon;
                            return (
                              <a
                                key={iIdx}
                                href={it.link}
                                className="flex items-center gap-2 px-4 py-2 text-xs font-medium text-gray-700 transition-all hover:text-violet-700 hover:bg-violet-50"
                              >
                                {Icon && <Icon className="w-4 h-4" />}
                                <span>{it.name}</span>
                              </a>
                            );
                          })}
                        </div>
                      </div>
                    )}
                  </li>
                ))
              )}
            </ul>
          </div>
        </nav>
      </header>

      {/* ==================== MEGA DROPDOWN – CHỈ HIỆN TRÊN DESKTOP ==================== */}
      {activeMenu === 'Mua sắm' && (
        <div className="hidden md:block fixed inset-x-0 top-32 left-1/2 -translate-x-1/2 w-full max-w-7xl mx-auto bg-white shadow-2xl z-[60] border border-violet-100 rounded-2xl overflow-y-auto px-6 py-8" style={{ maxHeight: 'calc(100vh - 9rem)' }}
          onMouseEnter={() => handleMouseEnter('Mua sắm')}
          onMouseLeave={handleMouseLeave}
        >
          <div className="grid gap-6 mb-6" style={{ gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))' }}>
            {shoppingMenu.categories?.map((cat: any, cIdx: number) => (
              <div key={cIdx}>
                <h3 className="mb-3 text-sm font-bold text-violet-900 border-b border-violet-200 pb-1.5">
                  {cat.title}
                </h3>
                <ul className="space-y-0.5">
                  {cat.items.map((it: any, iIdx: number) => (
                    <li key={iIdx}>
                      <button
                        onClick={() => handleSubCatClick(it.slug)}
                        className="w-full text-left px-2 py-1.5 text-xs font-medium text-gray-700 hover:text-violet-700 hover:bg-violet-50 rounded-md transition-all"
                      >
                        {it.name}
                      </button>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
          <div className="pt-5 border-t border-violet-100">
            <div className="flex flex-col items-center justify-between gap-4 p-4 text-white sm:flex-row bg-gradient-to-r from-violet-600 to-violet-800 rounded-xl">
              <div className="text-center sm:text-left">
                <h4 className="text-sm font-semibold">Khuyến mãi đặc biệt</h4>
                <p className="text-xs opacity-90">Giảm đến 50% sản phẩm mới</p>
              </div>
              <button className="px-4 py-2 text-xs font-medium transition-all bg-white rounded-lg text-violet-800 hover:scale-105 active:scale-95">
                Xem ngay
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ==================== MOBILE MENU – FULL MÀN HÌNH ==================== */}
      {isMobileMenuOpen && (
        <div className="fixed inset-0 z-50 overflow-y-auto bg-white top-16">
          <div className="px-4 py-5 space-y-4">
            {/* Mobile Search */}
            <form onSubmit={handleSearch} className="relative">
              <SearchIcon className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 left-3 top-1/2" />
              <input
                type="text"
                value={searchQuery}
                onChange={e => setSearchQuery(e.target.value)}
                placeholder="Tìm kiếm..."
                className="w-full pl-10 pr-10 py-2.5 text-sm bg-gray-100 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-violet-600/20"
              />
              {searchQuery && (
                <button type="button" onClick={clearSearch} className="absolute p-1 text-gray-400 -translate-y-1/2 right-9 top-1/2">
                  <X className="w-4 h-4" />
                </button>
              )}
              <button type="submit" className="absolute right-2 top-1/2 -translate-y-1/2 p-1.5 text-violet-700">
                <SearchIcon className="w-5 h-5" />
              </button>
            </form>

            {/* Mobile Menu Items */}
            <div className="space-y-1">
              {orderedMenus.map((menu, idx) => (
                <div key={idx}>
                  {menu.noDropdown ? (
                    <a
                      href={menu.link}
                      onClick={() => setIsMobileMenuOpen(false)}
                      className="block px-4 py-3 text-sm font-medium text-gray-900 hover:bg-violet-50 hover:text-violet-700 rounded-xl"
                    >
                      {menu.title}
                    </a>
                  ) : (
                    <div>
                      <button
                        onClick={() => toggleMobileSubmenu(menu.title)}
                        className="flex items-center justify-between w-full px-4 py-3 text-sm font-medium text-left text-gray-900 hover:bg-violet-50 hover:text-violet-700 rounded-xl"
                      >
                        {menu.title}
                        <ChevronDown className={`w-4 h-4 transition-transform ${expandedMobileMenu === menu.title ? 'rotate-180' : ''}`} />
                      </button>
                      {expandedMobileMenu === menu.title && (
                        <div className="mt-1 ml-4 space-y-1">
                          {menu.mega ? (
                            menu.categories?.map((cat: any, cIdx: number) => (
                              <div key={cIdx} className="mb-3">
                                <div className="px-3 py-2 text-xs font-bold text-violet-900">{cat.title}</div>
                                <div className="ml-4 space-y-1">
                                  {cat.items.map((it: any, iIdx: number) => (
                                    <button
                                      key={iIdx}
                                      onClick={() => handleSubCatClick(it.slug)}
                                      className="block w-full text-left px-4 py-2.5 text-sm text-gray-700 hover:bg-violet-50 hover:text-violet-700 rounded-lg"
                                    >
                                      {it.name}
                                    </button>
                                  ))}
                                </div>
                              </div>
                            ))
                          ) : (
                            menu.items?.map((it: any, iIdx: number) => (
                              <a
                                key={iIdx}
                                href={it.link}
                                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 rounded-lg hover:bg-violet-50 hover:text-violet-700"
                              >
                                {it.icon && <it.icon className="w-4 h-4" />}
                                {it.name}
                              </a>
                            ))
                          )}
                        </div>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>

            {/* Mobile Actions */}
            <div className="flex gap-3 pt-4 border-t">
              <button className="flex items-center justify-center flex-1 gap-2 py-3 font-medium bg-violet-50 text-violet-700 rounded-xl hover:bg-violet-100">
                <ShoppingCart className="w-5 h-5" />
                <span className="text-sm">Giỏ hàng</span>
              </button>
              <button
                onClick={() => { toggleMobileMenu(); handleUserClick(); }}
                className="flex items-center justify-center flex-1 gap-2 py-3 font-medium text-white bg-violet-700 rounded-xl hover:bg-violet-800"
              >
                <User className="w-5 h-5" />
                <span className="text-sm">Tài khoản</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}