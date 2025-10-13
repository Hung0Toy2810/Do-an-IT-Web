// ==================== utils/notify.ts ====================
import { Notification, NotificationType } from '../types';

let notificationCallbacks: ((notification: Notification) => void)[] = [];
let notificationId = 0;

export const notify = {
  success: (message: string) => {
    const id = ++notificationId;
    notificationCallbacks.forEach(cb => cb({ id, type: 'success', message }));
  },
  error: (message: string) => {
    const id = ++notificationId;
    notificationCallbacks.forEach(cb => cb({ id, type: 'error', message }));
  },
  warning: (message: string) => {
    const id = ++notificationId;
    notificationCallbacks.forEach(cb => cb({ id, type: 'warning', message }));
  }
};

export const registerNotificationCallback = (callback: (notification: Notification) => void) => {
  notificationCallbacks.push(callback);
  return () => {
    notificationCallbacks = notificationCallbacks.filter(cb => cb !== callback);
  };
};