// src/layouts/MainLayout.tsx
import { Outlet } from 'react-router-dom';
import HeaderAndNavbar from '../components/HeaderAndNavbar';
import Footer from '../components/Footer';

export default function MainLayout() {
  return (
    <div className="flex flex-col min-h-screen">
      <HeaderAndNavbar />
      <main className="flex-grow">
        <Outlet /> {/* Nội dung trang sẽ render ở đây */}
      </main>
      <Footer />
    </div>
  );
}