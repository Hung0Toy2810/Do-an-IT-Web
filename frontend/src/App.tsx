// src/App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import NotificationProvider from './components/NotificationProvider';
import MainLayout from './layouts/MainLayout';

import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ProfilePage from './pages/ProfilePage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import ChangePasswordPage from './pages/ChangePasswordPage';
import ProductDetail from './pages/ProductDetail';
import SearchPage from './pages/SearchPage';
import SubCategoryPage from './pages/SubCategoryPage';
import ShoppingCart from './pages/ShoppingCart';
// src/pages/OrderSuccess.tsx
import OrderSuccess from './pages/OrderSuccess';
export default function App() {
  return (
    <BrowserRouter>
      <NotificationProvider>
        <Routes>
          {/* Các trang dùng chung Header + Footer */}
          <Route element={<MainLayout />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/product/:slug" element={<ProductDetail />} />
            <Route path="/search" element={<SearchPage />} />
            <Route path="/subcategory/:slug" element={<SubCategoryPage />} />
            <Route path="/cart" element={<ShoppingCart />} />
            <Route path="/order-success" element={<OrderSuccess />} />
          </Route>

          {/* Trang riêng (không có Header/Footer) */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/change-password" element={<ChangePasswordPage />} />
        </Routes>
      </NotificationProvider>
    </BrowserRouter>
  );
}