import { Facebook, Instagram, Youtube, Mail, Phone, MapPin } from 'lucide-react';

export default function FooterDemo() {
  return (
    <div className="flex flex-col bg-gray-50">

      {/* Footer */}
      <footer className="text-gray-300 bg-gradient-to-br from-violet-800 to-violet-900" style={{ fontFamily: 'Roboto, sans-serif' }}>
        {/* Main Footer */}
        <div className="container max-w-screen-xl px-4 py-12 mx-auto sm:px-6 lg:px-8 lg:py-16">
          <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 xl:grid-cols-4 xl:gap-12">
            {/* Cột 1: PhoneCare */}
            <div className="min-w-0">
              <h3 className="mb-6 text-xl font-medium text-white">PhoneCare</h3>
              <p className="mb-6 text-sm leading-relaxed">
                Cung cấp phụ kiện điện thoại chính hãng: sạc nhanh, tai nghe, ốp lưng, kính cường lực…
              </p>
              <div className="space-y-3 text-sm">
                <div className="flex items-center gap-2">
                  <Mail className="w-4 h-4 text-violet-300" />
                  <span>support@phonecare.vn</span>
                </div>
                <div className="flex items-center gap-2">
                  <Phone className="w-4 h-4 text-violet-300" />
                  <span>Hotline: 0909 123 456</span>
                </div>
                <div className="flex items-center gap-2">
                  <MapPin className="w-4 h-4 text-violet-300" />
                  <span>123 Nguyễn Trãi, Q.5, TP.HCM</span>
                </div>
              </div>
              <div className="flex gap-4 mt-6">
                <a href="#" className="transition-colors duration-200 hover:text-white">
                  <Facebook className="w-5 h-5" />
                </a>
                <a href="#" className="transition-colors duration-200 hover:text-white">
                  <Instagram className="w-5 h-5" />
                </a>
                <a href="#" className="transition-colors duration-200 hover:text-white">
                  <Youtube className="w-5 h-5" />
                </a>
              </div>
            </div>

            {/* Cột 2: Hỗ trợ & Hướng dẫn */}
            <div className="min-w-0">
              <h4 className="mb-6 font-medium text-white">Hỗ trợ & Hướng dẫn</h4>
              <ul className="space-y-3 text-sm">
                <li><a href="#" className="transition-colors duration-200 hover:text-white">Hướng dẫn mua hàng</a></li>
                <li><a href="#" className="transition-colors duration-200 hover:text-white">Chính sách đổi trả</a></li>
                <li><a href="#" className="transition-colors duration-200 hover:text-white">Chính sách bảo mật</a></li>
                <li><a href="#" className="transition-colors duration-200 hover:text-white">Giao hàng & Thanh toán</a></li>
              </ul>
            </div>

            {/* Cột 3: About */}
            <div className="min-w-0">
              <h4 className="mb-6 font-medium text-white">About</h4>
              <ul className="space-y-3 text-sm">
                <li><a href="#" className="transition-colors duration-200 hover:text-white">Về chúng tôi</a></li>
                <li><a href="#" className="transition-colors duration-200 hover:text-white">Liên hệ</a></li>
                <li><a href="#" className="transition-colors duration-200 hover:text-white">Hệ thống cửa hàng</a></li>
                <li><a href="#" className="transition-colors duration-200 hover:text-white">Bảo hành sản phẩm</a></li>
              </ul>
            </div>

            {/* Cột 4: Newsletter */}
            <div className="min-w-0">
              <h4 className="mb-6 font-medium text-white">Newsletter</h4>
              <p className="mb-6 text-sm leading-relaxed">
                Nhận thông tin khuyến mãi & phụ kiện mới nhất.
              </p>
              <div className="flex flex-col gap-2 sm:flex-row">
                <input
                  type="email"
                  placeholder="Email của bạn"
                  className="w-full sm:flex-1 bg-violet-900/30 border border-violet-600/50 text-white rounded-lg px-4 py-2.5 text-sm placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-violet-400 focus:border-transparent"
                />
                <button className="bg-violet-600 text-white hover:bg-violet-500 rounded-lg font-medium px-6 py-2.5 text-sm transition-colors duration-200 whitespace-nowrap">
                  Đăng ký
                </button>
              </div>
            </div>
          </div>

          {/* Payment & Shipping */}
          <div className="pt-12 mt-16 border-t border-violet-600/40">
            <div className="grid grid-cols-1 gap-12 xl:grid-cols-2">
              <div>
                <h5 className="mb-4 font-medium text-white">Thanh toán</h5>
                <div className="flex flex-wrap gap-3">
                  {['Momo', 'ZaloPay', 'VNPay', 'Visa', 'Master', 'COD'].map((method) => (
                    <div key={method} className="px-4 py-2 text-sm border rounded-lg bg-violet-900/30 border-violet-600/40">
                      {method}
                    </div>
                  ))}
                </div>
              </div>
              <div>
                <h5 className="mb-4 font-medium text-white">Đơn vị vận chuyển</h5>
                <div className="flex flex-wrap gap-3">
                  {['GHN', 'GHTK', 'Viettel Post', 'J&T Express'].map((shipper) => (
                    <div key={shipper} className="px-4 py-2 text-sm border rounded-lg bg-violet-900/30 border-violet-600/40">
                      {shipper}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Copyright */}
        <div className="border-t border-violet-600/40">
          <div className="container px-4 py-6 mx-auto text-sm text-center sm:px-6 lg:px-8">
            <p>© 2025 PhoneCare. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  );
}