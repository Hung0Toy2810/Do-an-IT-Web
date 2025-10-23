import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom'; 
import RegisterAdminPage from './pages/RegisterAdminPage';
import AdminLoginPage from './pages/AdminLoginPage';
export default function App() {
  return (
    <BrowserRouter>
      <NotificationProvider>
        <Routes>
          <Route path="/register-admin" element={<RegisterAdminPage />} />
          <Route path="/admin-login" element={<AdminLoginPage />} />
        </Routes>
      </NotificationProvider>
    </BrowserRouter>
  );
}