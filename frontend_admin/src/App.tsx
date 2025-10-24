import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'; 
import RegisterAdminPage from './pages/RegisterAdminPage';
import AdminLoginPage from './pages/AdminLoginPage';
import AdminLayout from './components/AdminLayout';
import ProductManagement from './pages/ProductManagement';
import OrderManagement from './pages/OrderManagement';
import CustomerManagement from './pages/CustomerManagement';
import ReportsAnalytics from './pages/ReportsAnalytics';
import ContentManagement from './pages/ContentManagement';
import AdminUserManagement from './pages/AdminUserManagement';
import NotificationProvider from './components/NotificationProvider';
import ProtectedRoute from './components/ProtectedRoute';

export default function App() {
  return (
    <BrowserRouter>
      <NotificationProvider>
        <Routes>
          {/* Public Routes */}
          <Route path="/register-admin" element={<RegisterAdminPage />} />
          <Route path="/admin-login" element={<AdminLoginPage />} />
          
          {/* Protected Admin Routes */}
          <Route 
            path="/admin" 
            element={
              <ProtectedRoute>
                <AdminLayout />
              </ProtectedRoute>
            }
          >
            {/* Nested Routes */}
            <Route index element={<Navigate to="/admin/products" replace />} />
            <Route path="products" element={<ProductManagement />} />
            <Route path="orders" element={<OrderManagement />} />
            <Route path="customers" element={<CustomerManagement />} />
            <Route path="reports" element={<ReportsAnalytics />} />
            <Route path="content" element={<ContentManagement />} />
            <Route path="admins" element={<AdminUserManagement />} />
          </Route>
          
          {/* Redirect root to admin */}
          <Route path="/" element={<Navigate to="/admin" replace />} />
          
          {/* 404 - Redirect to admin */}
          <Route path="*" element={<Navigate to="/admin" replace />} />
        </Routes>
      </NotificationProvider>
    </BrowserRouter>
  );
}