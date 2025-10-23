import React from 'react';
import NotificationProvider from './components/NotificationProvider';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import ProfilePage from './pages/ProfilePage';
import ForgotPasswordPage from './pages/ForgotPasswordPage';
import { BrowserRouter, Routes, Route } from 'react-router-dom'; 
import ChangePasswordPage from './pages/ChangePasswordPage';
import RegisterAdminPage from './pages/RegisterAdminPage';
import AdminLoginPage from './pages/AdminLoginPage';
export default function App() {
  return (
    <BrowserRouter>
      <NotificationProvider>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/FogotPassword" element={<ForgotPasswordPage />} />
          <Route path="/change-password" element={<ChangePasswordPage />} />
          <Route path="/register-admin" element={<RegisterAdminPage />} />
          <Route path="/admin-login" element={<AdminLoginPage />} />
        </Routes>
      </NotificationProvider>
    </BrowserRouter>
  );
}