// components/NotificationProvider.tsx
import React, { useState, useEffect } from 'react';
import NotificationItem from './NotificationItem';
import { Notification, NotificationType } from '../types';

let nextId = 1;

interface WindowWithNotify extends Window {
  __globalNotify?: (type: NotificationType, message: string) => void;
  __notifyQueue?: Array<{ type: NotificationType; message: string }>;
}

declare const window: WindowWithNotify;

export default function NotificationProvider({ children }: { children: React.ReactNode }) {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    // Gán callback toàn cục khi Provider mount
    window.__globalNotify = (type: NotificationType, message: string) => {
      const id = nextId++;
      const newNotification = { id, type, message };

      setNotifications((prev) => {
        const updated = [...prev, newNotification];
        return updated.length > 5 ? updated.slice(-5) : updated;
      });

      setTimeout(() => {
        setNotifications((prev) => prev.filter((n) => n.id !== id));
      }, 5000);
    };

    // Xử lý queue nếu có thông báo bị kẹt
    if (window.__notifyQueue && window.__notifyQueue.length > 0) {
      window.__notifyQueue.forEach(({ type, message }) => {
        window.__globalNotify!(type, message);
      });
      window.__notifyQueue = [];
    }

    return () => {
      window.__globalNotify = undefined;
    };
  }, []);

  const removeNotification = (id: number) => {
    setNotifications((prev) => prev.filter((n) => n.id !== id));
  };

  return (
    <>
      {children}

      {/* Desktop & Tablet */}
      <div className="fixed z-50 hidden w-full max-w-xs px-4 space-y-2 pointer-events-none sm:block top-4 right-4 sm:px-0">
        {notifications.map((n) => (
          <NotificationItem key={n.id} notification={n} onRemove={removeNotification} />
        ))}
      </div>

      {/* Mobile */}
      <div className="fixed left-0 right-0 z-50 block w-full px-3 space-y-2 pointer-events-none sm:hidden top-4">
        <div className="max-w-sm mx-auto space-y-2">
          {notifications.map((n) => (
            <NotificationItem key={n.id} notification={n} onRemove={removeNotification} />
          ))}
        </div>
      </div>
    </>
  );
}