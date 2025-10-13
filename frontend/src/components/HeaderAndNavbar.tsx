// ==================== components/HeaderAndNavbar.tsx ====================
import React, { useState, useRef, useCallback, useEffect } from 'react';
import { 
  ChevronDown, 
  Menu,
  X,
  ShoppingCart,
  User,
  Search
} from 'lucide-react';
import { PageType } from '../types';
import { mockMenuData } from '../data/menuData';
import { isTokenValid } from '../utils/auth';

interface HeaderAndNavbarProps {
  onNavigate: (page: PageType) => void;
}

export default function HeaderAndNavbar({ onNavigate }: HeaderAndNavbarProps) {
  const [activeMenu, setActiveMenu] = useState<string | null>(null);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [expandedMobileMenu, setExpandedMobileMenu] = useState<string | null>(null);
  const [cartCount] = useState(3);
  const leaveTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    if (activeMenu || isMobileMenuOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [activeMenu, isMobileMenuOpen]);

  const handleMouseEnter = useCallback((menuTitle: string) => {
    if (leaveTimeoutRef.current) {
      clearTimeout(leaveTimeoutRef.current);
      leaveTimeoutRef.current = null;
    }
    setActiveMenu(menuTitle);
  }, []);

  const handleMouseLeave = useCallback(() => {
    leaveTimeoutRef.current = setTimeout(() => {
      setActiveMenu(null);
    }, 300);
  }, []);

  const toggleMobileMenu = () => {
    setIsMobileMenuOpen(!isMobileMenuOpen);
    setExpandedMobileMenu(null);
  };

  const toggleMobileSubmenu = (title: string) => {
    setExpandedMobileMenu(expandedMobileMenu === title ? null : title);
  };

  const handleWheel = (e: React.WheelEvent<HTMLDivElement>) => {
    e.stopPropagation();
  };

  const handleUserClick = () => {
    onNavigate(isTokenValid() ? 'profile' : 'login');
  };

  return (
    <div className="sticky top-0 z-50 w-full font-sans bg-white shadow" style={{ fontFamily: 'Roboto, -apple-system, BlinkMacSystemFont, sans-serif' }}>
      {/* Header */}
      <div className="sticky top-0 z-50 bg-white border-b border-gray-200/80">
        <div className="max-w-screen-xl px-4 mx-auto sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center">
              <a href="#" className="text-2xl font-bold text-violet-900">
                PhoneCare
              </a>
            </div>

            <div className="items-center justify-end flex-1 hidden gap-4 ml-8 md:flex">
              <div className="relative flex-1 max-w-2xl">
                <Search className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 pointer-events-none left-4 top-1/2" />
                <input
                  type="text"
                  placeholder="Tìm kiếm sản phẩm..."
                  className="w-full pl-12 pr-4 py-2.5 text-sm font-medium bg-gray-100 border border-gray-200/80 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 transition-all"
                  style={{ borderRadius: '12px' }}
                />
              </div>
              <button 
                className="relative p-2.5 text-gray-900 hover:text-violet-800 hover:bg-violet-50 transition-all"
                style={{ borderRadius: '10px' }}
              >
                <ShoppingCart className="w-5 h-5" />
                {cartCount > 0 && (
                  <span 
                    className="absolute -top-1 -right-1 bg-violet-800 text-white text-xs font-bold min-w-[20px] h-5 flex items-center justify-center px-1.5"
                    style={{ borderRadius: '10px' }}
                  >
                    {cartCount}
                  </span>
                )}
              </button>
              <button 
                onClick={handleUserClick}
                className="p-2.5 text-gray-900 hover:text-violet-800 hover:bg-violet-50 transition-all"
                style={{ borderRadius: '10px' }}
              >
                <User className="w-5 h-5" />
              </button>
            </div>

            <button
              onClick={toggleMobileMenu}
              className="md:hidden p-2.5 text-gray-900 hover:text-violet-800 hover:bg-violet-50 transition-all"
              style={{ borderRadius: '10px' }}
            >
              {isMobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
            </button>
          </div>
        </div>
      </div>

      {/* Navigation */}
      <nav className="relative z-40 hidden text-white shadow-lg md:block bg-gradient-to-r from-violet-700 to-violet-800">
        <div className="px-4 mx-auto max-w-7xl sm:px-6 lg:px-8">
          <ul className="flex items-center justify-center gap-1">
            {mockMenuData.map((menu, index) => (
              <li key={index} className="relative">
                {menu.noDropdown ? (
                  <a 
                    href={menu.link}
                    className="flex items-center h-12 px-6 text-base font-medium transition-all duration-200 hover:text-violet-100 hover:bg-white/10"
                    style={{ borderRadius: '10px' }}
                  >
                    {menu.title}
                  </a>
                ) : (
                  <button 
                    className="flex items-center h-12 gap-2 px-6 text-base font-medium transition-all duration-200 hover:text-violet-100 hover:bg-white/10"
                    style={{ borderRadius: '10px' }}
                    onMouseEnter={() => handleMouseEnter(menu.title)}
                    onMouseLeave={handleMouseLeave}
                  >
                    {menu.title}
                    <ChevronDown className={`w-4 h-4 transition-transform duration-200 ${activeMenu === menu.title ? 'rotate-180' : ''}`} />
                  </button>
                )}

                {activeMenu === menu.title && menu.mega && (
                  <div 
                    className="fixed left-1/2 -translate-x-1/2 bg-white text-gray-800 shadow-2xl py-6 px-6 z-[100] border border-violet-100/50 overflow-y-auto"
                    style={{ 
                      marginTop: '4px', 
                      borderRadius: '18px',
                      top: 'calc(4rem + 3rem)',
                      width: 'fit-content',
                      maxWidth: '95vw',
                      maxHeight: '70vh',
                    }}
                    onMouseEnter={() => handleMouseEnter(menu.title)}
                    onMouseLeave={handleMouseLeave}
                    onWheel={handleWheel}
                  >
                    <div className="flex gap-6 mb-5">
                      {menu.categories?.map((category, idx) => {
                        return (
                          <div key={idx} className="flex flex-col">
                            <div className="pb-2.5 mb-2.5 border-b border-violet-100">
                              <h3 className="text-sm font-bold text-violet-900 whitespace-nowrap">{category.title}</h3>
                            </div>
                            <div className="flex flex-col gap-0.5">
                              {category.items.map((item, itemIdx) => (
                                <a
                                  key={itemIdx}
                                  href={item.link}
                                  className="flex items-center px-3 py-2 text-xs font-medium text-gray-700 transition-all duration-200 hover:text-violet-800 hover:bg-violet-50 hover:translate-x-1 hover:shadow-sm whitespace-nowrap"
                                  style={{ borderRadius: '8px' }}
                                >
                                  {item.name}
                                </a>
                              ))}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                    <div className="pt-5 border-t border-violet-100">
                      <div 
                        className="flex items-center justify-between gap-4 p-4 text-white transition-all duration-300 bg-gradient-to-r from-violet-600 to-violet-800 hover:from-violet-700 hover:to-violet-900"
                        style={{ borderRadius: '14px' }}
                      >
                        <div className="flex flex-col justify-center">
                          <h4 className="mb-1 text-sm font-semibold">Khuyến mãi đặc biệt</h4>
                          <p className="text-xs text-violet-100">Giảm giá lên đến 50% cho sản phẩm mới</p>
                        </div>
                        <button 
                          className="flex-shrink-0 px-5 py-2 text-xs font-medium transition-all bg-white text-violet-800 hover:bg-violet-50 hover:scale-105 active:scale-95 whitespace-nowrap"
                          style={{ borderRadius: '10px' }}
                        >
                          Xem ngay
                        </button>
                      </div>
                    </div>
                  </div>
                )}

                {activeMenu === menu.title && !menu.mega && (
                  <div 
                    className="absolute top-full left-0 bg-white text-gray-800 shadow-2xl py-2 px-2 z-[100] border border-violet-100/50 overflow-y-auto"
                    style={{ 
                      marginTop: '4px', 
                      borderRadius: '14px',
                      width: 'fit-content',
                      minWidth: '220px',
                      maxHeight: '50vh',
                    }}
                    onMouseEnter={() => handleMouseEnter(menu.title)}
                    onMouseLeave={handleMouseLeave}
                    onWheel={handleWheel}
                  >
                    <div className="flex flex-col gap-1">
                      {menu.items?.map((item, idx) => {
                        const ItemIcon = item.icon;
                        return (
                          <a
                            key={idx}
                            href={item.link}
                            className="px-3 py-2.5 flex items-center gap-2.5 hover:text-violet-800 hover:bg-violet-50 hover:shadow-sm transition-all duration-200 group"
                            style={{ borderRadius: '10px' }}
                          >
                            {ItemIcon && (
                              <div 
                                className="flex items-center justify-center flex-shrink-0 transition-all bg-gradient-to-br from-violet-100 to-violet-200 group-hover:scale-110 group-hover:from-violet-200 group-hover:to-violet-300 w-7 h-7"
                                style={{ borderRadius: '8px' }}
                              >
                                <ItemIcon className="w-4 h-4 text-violet-800" />
                              </div>
                            )}
                            <span className="text-xs font-medium whitespace-nowrap">{item.name}</span>
                          </a>
                        );
                      })}
                    </div>
                  </div>
                )}
              </li>
            ))}
          </ul>
        </div>
      </nav>

      {/* Mobile Menu */}
      {isMobileMenuOpen && (
        <div className="overflow-y-auto bg-white border-b border-gray-200 shadow-lg md:hidden" style={{ maxHeight: '80vh' }} onWheel={handleWheel}>
          <div className="px-4 py-4 space-y-3">
            <div className="relative">
              <Search className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 pointer-events-none left-4 top-1/2" />
              <input
                type="text"
                placeholder="Tìm kiếm sản phẩm..."
                className="w-full pl-12 pr-4 py-2.5 text-sm font-medium bg-gray-100 border border-gray-200/80 text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-violet-800/20 focus:border-violet-800 transition-all"
                style={{ borderRadius: '12px' }}
              />
            </div>
            <div className="space-y-1">
              {mockMenuData.map((menu, index) => (
                <div key={index}>
                  {menu.noDropdown ? (
                    <a
                      href={menu.link}
                      className="block px-4 py-3 text-sm font-medium text-gray-900 transition-all hover:bg-violet-50 hover:text-violet-800"
                      style={{ borderRadius: '10px' }}
                    >
                      {menu.title}
                    </a>
                  ) : (
                    <div>
                      <button
                        onClick={() => toggleMobileSubmenu(menu.title)}
                        className="flex items-center justify-between w-full px-4 py-3 text-sm font-medium text-left text-gray-900 transition-all hover:bg-violet-50 hover:text-violet-800"
                        style={{ borderRadius: '10px' }}
                      >
                        {menu.title}
                        <ChevronDown className={`w-4 h-4 transition-transform duration-200 ${expandedMobileMenu === menu.title ? 'rotate-180' : ''}`} />
                      </button>
                      {expandedMobileMenu === menu.title && (
                        <div className="mt-1 ml-4 space-y-1 overflow-y-auto" style={{ maxHeight: '50vh' }}>
                          {menu.mega ? (
                            menu.categories?.map((category, catIdx) => (
                              <div key={catIdx} className="mb-3">
                                <div className="flex items-center gap-2 px-3 py-2 text-xs font-semibold text-violet-900">
                                  <category.icon className="w-4 h-4" />
                                  {category.title}
                                </div>
                                {category.items.map((item, itemIdx) => (
                                  <a
                                    key={itemIdx}
                                    href={item.link}
                                    className="block px-4 py-2 text-sm font-medium text-gray-700 transition-all hover:bg-violet-50 hover:text-violet-800"
                                    style={{ borderRadius: '8px' }}
                                  >
                                    {item.name}
                                  </a>
                                ))}
                              </div>
                            ))
                          ) : (
                            menu.items?.map((item, itemIdx) => (
                              <a
                                key={itemIdx}
                                href={item.link}
                                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 transition-all hover:bg-violet-50 hover:text-violet-800"
                                style={{ borderRadius: '8px' }}
                              >
                                {item.icon && <item.icon className="w-4 h-4" />}
                                {item.name}
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
            <div className="flex gap-2 pt-3 border-t border-gray-200">
              <button 
                className="flex items-center justify-center flex-1 gap-2 p-3 font-medium transition-all bg-violet-50 text-violet-800 hover:bg-violet-100"
                style={{ borderRadius: '12px' }}
              >
                <ShoppingCart className="w-5 h-5" />
                <span className="text-sm">Giỏ hàng</span>
              </button>
              <button 
                onClick={() => {
                  toggleMobileMenu();
                  handleUserClick();
                }}
                className="flex items-center justify-center flex-1 gap-2 p-3 font-medium text-white transition-all bg-violet-800 hover:bg-violet-900"
                style={{ borderRadius: '12px' }}
              >
                <User className="w-5 h-5" />
                <span className="text-sm">Tài khoản</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}