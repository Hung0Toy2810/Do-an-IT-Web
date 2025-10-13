// ==================== pages/ProfilePage.tsx ====================
import React from 'react';
import { ArrowLeft } from 'lucide-react';
import { PageType } from '../types';

interface ProfilePageProps {
  onNavigate: (page: PageType) => void;
}

export default function ProfilePage({ onNavigate }: ProfilePageProps) {
  return (
    <div className="min-h-screen px-4 py-6 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
      <div className="container max-w-4xl mx-auto">
        <button
          onClick={() => onNavigate('home')}
          className="flex items-center gap-2 px-4 py-2.5 mb-8 text-sm font-medium text-violet-800 transition-all bg-white hover:bg-violet-50 shadow-sm hover:shadow-md"
          style={{ borderRadius: '12px' }}
        >
          <ArrowLeft className="w-4 h-4" />
          Quay lại
        </button>
        
        <div className="p-8 bg-white shadow-xl" style={{ borderRadius: '20px' }}>
          <h1 className="mb-4 text-3xl font-bold text-violet-900">Trang cá nhân</h1>
          <p className="text-gray-600">Đây là trang profile của bạn. (Chưa hoàn thiện)</p>
        </div>
      </div>
    </div>
  );
}