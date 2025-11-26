// src/components/banners/FastChargeBanner.tsx
import React from "react";
import { BatteryCharging } from "lucide-react";

export function FastChargeBanner() {
  return (
    <section className="py-10 bg-violet-50">
      <div className="px-4 mx-auto max-w-7xl sm:px-6 lg:px-8">
        <div className="p-8 text-center text-white bg-purple-600 shadow-2xl rounded-3xl md:p-16">
          <BatteryCharging className="w-16 h-16 mx-auto mb-6 md:w-20 md:h-20" />
          <h2 className="mb-4 text-3xl font-black md:mb-6 md:text-5xl">
            Sạc đầy pin chỉ 30 phút
          </h2>
          <p className="mb-6 text-base opacity-95 md:mb-8 md:text-lg">
            Sạc GaN 65W/100W • Pin dự phòng 20000mAh • Cáp C-to-C chính hãng
          </p>
          <a
            href="/collections/sac-nhanh"
            className="inline-block px-10 py-4 text-lg font-bold text-purple-600 transition bg-white shadow-lg md:px-12 md:py-5 md:text-xl rounded-xl hover:bg-gray-100"
          >
            Mua sạc nhanh ngay
          </a>
        </div>
      </div>
    </section>
  );
}