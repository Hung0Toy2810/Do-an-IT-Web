// ==================== components/NotificationProvider.tsx ====================
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
    setNotifications((prev) => [...prev, { id, type, message }]);
    
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
      <div className="fixed z-50 w-full max-w-xs space-y-2 pointer-events-none top-4 right-4">
        {notifications.map((notification) => (
          <NotificationItem
            key={notification.id}
            notification={notification}
            onRemove={removeNotification}
          />
        ))}
      </div>
    </>
  );
}