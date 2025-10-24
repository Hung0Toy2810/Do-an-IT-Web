// ==================== utils/auth.ts ====================
import { getCookie, deleteCookie } from './cookies';

export const isTokenValid = (): boolean => {
  const token = getCookie('auth_token');
  if (!token) return false;

  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const exp = payload.exp * 1000;
    
    if (Date.now() >= exp) {
      deleteCookie('auth_token');
      return false;
    }
    
    return true;
  } catch (error) {
    console.error('Lỗi khi kiểm tra token:', error);
    deleteCookie('auth_token');
    return false;
  }
};