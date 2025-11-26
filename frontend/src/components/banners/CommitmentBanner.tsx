// src/components/banners/CommitmentBanner.tsx
import React from "react";
import { CheckCircle } from "lucide-react";

export function CommitmentBanner() {
  const promises = [
    "Hàng chính hãng 100% – Có tem bảo hành",
    "Đổi trả miễn phí trong 30 ngày",
    "Hỗ trợ kỹ thuật trọn đời",
    "Giao hàng nhanh 2h nội thành HCM/HN",
  ];

  return (
    <section className="px-4 py-12 text-white bg-violet-900 sm:px-6 lg:px-8 md:py-16">
      <div className="max-w-6xl mx-auto text-center">
        <h2 className="mb-8 text-3xl font-black md:mb-12 md:text-5xl">Cam kết từ chúng tôi</h2>
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4 md:gap-8">
          {promises.map((text, i) => (
            <div
              key={i}
              className="flex flex-col items-center p-6 transition bg-violet-800 md:p-8 rounded-2xl hover:bg-violet-700"
            >
              <CheckCircle className="w-10 h-10 mb-3 md:w-12 md:h-12 md:mb-4 text-violet-300" />
              <p className="text-base font-medium md:text-lg">{text}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}