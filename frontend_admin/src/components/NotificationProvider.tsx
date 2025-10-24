// ==================== components/NotificationProvider.tsx ====================
/* eslint-disable react-refresh/only-export-components */
import React, { useState } from 'react';
import NotificationItem from './NotificationItem';
import { Notification, NotificationType } from '../types';

// Hàm notify toàn cục
let notifyCallback: ((type: NotificationType, message: string) => void) | null = null;

export function notify(type: NotificationType, message: string) {
  if (notifyCallback) {
    notifyCallback(type, message);
  }
}

interface NotificationProviderProps {
  children: React.ReactNode;
}

let nextId = 1;

export default function NotificationProvider({ children }: NotificationProviderProps) {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  // Cập nhật notifyCallback khi component mount
  notifyCallback = (type: NotificationType, message: string) => {
    const id = nextId++;
    const newNotification = { id, type, message };
    
    setNotifications((prev) => {
      // Giới hạn tối đa 5 thông báo cùng lúc
      const updated = [...prev, newNotification];
      if (updated.length > 5) {
        return updated.slice(-5);
      }
      return updated;
    });
    
    // Tự động xóa sau 5 giây
    setTimeout(() => {
      setNotifications((prev) => prev.filter((n) => n.id !== id));
    }, 5000);
  };

  const removeNotification = (id: number) => {
    setNotifications((prev) => prev.filter((n) => n.id !== id));
  };

  return (
    <>
      {children}
      {/* Desktop & Tablet - Top Right */}
      <div className="fixed z-50 hidden w-full max-w-xs px-4 space-y-2 pointer-events-none sm:block top-4 right-4 sm:px-0">
        {notifications.map((notification) => (
          <NotificationItem
            key={notification.id}
            notification={notification}
            onRemove={removeNotification}
          />
        ))}
      </div>
      
      {/* Mobile - Top Center */}
      <div className="fixed left-0 right-0 z-50 block w-full px-3 space-y-2 pointer-events-none sm:hidden top-4">
        <div className="max-w-sm mx-auto space-y-2">
          {notifications.map((notification) => (
            <NotificationItem
              key={notification.id}
              notification={notification}
              onRemove={removeNotification}
            />
          ))}
        </div>
      </div>
    </>
  );
}