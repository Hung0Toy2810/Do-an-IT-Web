// src/components/banners/AudioBanner.tsx
import React from "react";
import { Headphones } from "lucide-react";

export function AudioBanner() {
  return (
    <section className="py-10 bg-violet-50">
      <div className="px-4 mx-auto max-w-7xl sm:px-6 lg:px-8">
        <div className="p-8 text-center text-white shadow-2xl bg-violet-600 rounded-3xl md:p-16 md:text-left">
          <div className="max-w-3xl mx-auto md:mx-0">
            <Headphones className="w-16 h-16 mx-auto mb-6 md:w-20 md:h-20 md:mx-0" />
            <h2 className="mb-4 text-3xl font-black md:mb-6 md:text-5xl">
              Tai nghe Bluetooth<br />Âm thanh sống động
            </h2>
            <p className="mb-6 text-base opacity-95 md:mb-8 md:text-lg">
              AirPods, Sony, JBL, Samsung – chống ồn, pin siêu lâu
            </p>
            <div className="flex flex-col justify-center gap-3 sm:flex-row md:justify-start md:gap-4">
              <a
                href="/collections/tai-nghe"
                className="px-8 py-3 text-base font-bold transition bg-white text-violet-700 md:px-10 md:py-4 md:text-lg rounded-xl hover:bg-gray-100"
              >
                Xem tất cả
              </a>
              <a
                href="/collections/tai-nghe-duoi-2-trieu"
                className="px-8 py-3 text-base font-bold text-white transition border-2 border-white md:px-10 md:py-4 md:text-lg rounded-xl hover:bg-white hover:text-violet-700"
              >
                Dưới 2 triệu
              </a>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
