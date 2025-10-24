import { FileText } from 'lucide-react';

export default function ContentManagement() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div 
        className="p-6 bg-white border shadow-lg border-violet-100/50"
        style={{ borderRadius: '20px' }}
      >
        <h1 className="mb-2 text-2xl font-bold text-transparent lg:text-3xl bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text">
          Quản lý nội dung
        </h1>
        <p className="text-sm font-medium text-gray-600">
          Quản lý bài viết, banner, thông báo và nội dung website
        </p>
      </div>

      {/* Content */}
      <div 
        className="p-8 bg-white border shadow-lg lg:p-12 border-violet-100/50"
        style={{ borderRadius: '20px' }}
      >
        <div className="flex flex-col items-center justify-center py-12 text-center lg:py-20">
          <div 
            className="inline-flex items-center justify-center w-16 h-16 mb-4 shadow-lg lg:w-20 lg:h-20 bg-gradient-to-br from-violet-600 to-violet-800"
            style={{ borderRadius: '16px' }}
          >
            <FileText className="w-8 h-8 text-white lg:w-10 lg:h-10" />
          </div>
          <h2 className="mb-2 text-xl font-bold text-gray-900 lg:text-2xl">
            Trang đang được phát triển
          </h2>
          <p className="max-w-md text-sm font-medium text-gray-600 lg:text-base">
            Chức năng quản lý nội dung sẽ sớm được cập nhật. Vui lòng quay lại sau!
          </p>
        </div>
      </div>
    </div>
  );
}