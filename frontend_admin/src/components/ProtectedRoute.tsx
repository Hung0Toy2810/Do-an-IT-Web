import React, { useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { getCookie, deleteCookie } from '../utils/cookies';
import { isTokenValid } from '../utils/auth';

interface ProtectedRouteProps {
  children: React.ReactNode;
}

export default function ProtectedRoute({ children }: ProtectedRouteProps) {
  const [isChecking, setIsChecking] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    checkAuthentication();
  }, []);

  const checkAuthentication = () => {
    const token = getCookie('auth_token');
    
    // Không có token hoặc token không hợp lệ
    if (!token || !isTokenValid()) {
      deleteCookie('auth_token');
      setIsAuthenticated(false);
      setIsChecking(false);
      return;
    }

    setIsAuthenticated(true);
    setIsChecking(false);
  };

  if (isChecking) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
        <div className="flex flex-col items-center gap-4">
          <svg className="w-12 h-12 text-violet-700 animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <p className="text-sm font-medium text-violet-700">Đang kiểm tra xác thực...</p>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/admin-login" replace />;
  }

  return <>{children}</>;
}