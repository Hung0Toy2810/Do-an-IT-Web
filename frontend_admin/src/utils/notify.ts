// utils/notify.ts
import type { NotificationType } from '../types';

type NotifyCallback = (type: NotificationType, message: string) => void;

interface WindowWithNotify extends Window {
  __globalNotify?: NotifyCallback;
  __notifyQueue?: Array<{ type: NotificationType; message: string }>;
}

declare const window: WindowWithNotify;

// Hàm gọi nội bộ
const _notify = (type: NotificationType, message: string) => {
  if (window.__globalNotify) {
    window.__globalNotify(type, message);
    return;
  }

  if (!window.__notifyQueue) {
    window.__notifyQueue = [];
  }
  window.__notifyQueue.push({ type, message });
};

// === EXPORT OBJECT notify ===
export const notify = {
  success: (message: string) => _notify('success', message),
  error: (message: string) => _notify('error', message),
  warning: (message: string) => _notify('warning', message),
  info: (message: string) => _notify('info', message),
};