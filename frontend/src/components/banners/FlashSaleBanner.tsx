// src/components/banners/FlashSaleBanner.tsx
import React from "react";
import { Zap, Shield, Truck, ChevronRight } from "lucide-react";

export function FlashSaleBanner() {
  return (
    <section className="py-10 bg-violet-50">
      <div className="grid grid-cols-1 gap-4 px-4 mx-auto max-w-7xl sm:px-6 lg:px-8 sm:gap-6 md:grid-cols-3">
        {/* Flash Sale */}
        <a
          href="/flash-sale"
          className="flex items-center gap-4 p-5 text-white transition-all transform shadow-lg group bg-violet-600 md:gap-5 md:p-6 rounded-2xl hover:shadow-xl hover:-translate-y-1"
        >
          <Zap className="w-12 h-12 md:w-14 md:h-14" />
          <div>
            <h3 className="text-lg font-black md:text-xl">FLASH SALE</h3>
            <p className="text-xs opacity-90 md:text-sm">Giảm tới 70% hôm nay</p>
            <span className="flex items-center mt-1 text-xs font-bold md:mt-2 md:text-sm">
              Mua ngay <ChevronRight className="w-3 h-3 ml-1 md:w-4 md:h-4" />
            </span>
          </div>
        </a>

        {/* Bảo hành */}
        <a
          href="/chinh-sach-bao-hanh"
          className="flex items-center gap-4 p-5 text-white transition-all transform shadow-lg group bg-purple-600 md:gap-5 md:p-6 rounded-2xl hover:shadow-xl hover:-translate-y-1"
        >
          <Shield className="w-12 h-12 md:w-14 md:h-14" />
          <div>
            <h3 className="text-lg font-black md:text-xl">BẢO HÀNH VÀNG</h3>
            <p className="text-xs opacity-90 md:text-sm">12 tháng 1 đổi 1</p>
          </div>
        </a>

        {/* Freeship */}
        <a
          href="/giao-hang"
          className="flex items-center gap-4 p-5 text-white transition-all transform shadow-lg group bg-violet-700 md:gap-5 md:p-6 rounded-2xl hover:shadow-xl hover:-translate-y-1"
        >
          <Truck className="w-12 h-12 md:w-14 md:h-14" />
          <div>
            <h3 className="text-lg font-black md:text-xl">MIỄN PHÍ VẬN CHUYỂN</h3>
            <p className="text-xs opacity-90 md:text-sm">Đơn từ 300k</p>
          </div>
        </a>
      </div>
    </section>
  );
}