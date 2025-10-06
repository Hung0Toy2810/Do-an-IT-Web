import { useState, useEffect } from 'react';
import { CheckCircle, AlertCircle, AlertTriangle, X } from 'lucide-react';

type NotificationType = 'success' | 'error' | 'warning';

interface Notification {
  id: number;
  type: NotificationType;
  message: string;
}

// Global notification state
let notificationId = 0;
let addNotificationCallback: ((type: NotificationType, message: string) => void) | null = null;

// Global function to trigger notifications
export const notify = {
  success: (message: string) => addNotificationCallback?.('success', message),
  error: (message: string) => addNotificationCallback?.('error', message),
  warning: (message: string) => addNotificationCallback?.('warning', message),
};

export function NotificationProvider({ children }: { children: React.ReactNode }) {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  useEffect(() => {
    addNotificationCallback = (type: NotificationType, message: string) => {
      const id = ++notificationId;
      setNotifications(prev => [...prev, { id, type, message }]);
      
      // Auto remove after 5 seconds
      setTimeout(() => {
        setNotifications(prev => prev.filter(n => n.id !== id));
      }, 5000);
    };

    return () => {
      addNotificationCallback = null;
    };
  }, []);

  const removeNotification = (id: number) => {
    setNotifications(prev => prev.filter(n => n.id !== id));
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

function NotificationItem({ 
  notification, 
  onRemove 
}: { 
  notification: Notification;
  onRemove: (id: number) => void;
}) {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    setTimeout(() => setIsVisible(true), 10);
  }, []);

  const handleRemove = () => {
    setIsVisible(false);
    setTimeout(() => onRemove(notification.id), 300);
  };

  const config = {
    success: {
      bg: 'bg-green-50',
      border: 'border-green-200',
      iconBg: 'bg-green-100',
      icon: CheckCircle,
      iconColor: 'text-green-600',
      textColor: 'text-green-900',
    },
    error: {
      bg: 'bg-red-50',
      border: 'border-red-200',
      iconBg: 'bg-red-100',
      icon: AlertCircle,
      iconColor: 'text-red-600',
      textColor: 'text-red-900',
    },
    warning: {
      bg: 'bg-yellow-50',
      border: 'border-yellow-200',
      iconBg: 'bg-yellow-100',
      icon: AlertTriangle,
      iconColor: 'text-yellow-600',
      textColor: 'text-yellow-900',
    },
  };

  const { bg, border, iconBg, icon: Icon, iconColor, textColor } = config[notification.type];

  return (
    <div
      className={`${bg} ${border} border-2 shadow-lg backdrop-blur-sm pointer-events-auto transition-all duration-300 transform ${
        isVisible ? 'translate-x-0 opacity-100' : 'translate-x-full opacity-0'
      }`}
      style={{ borderRadius: '12px' }}
    >
      <div className="flex items-start gap-2.5 p-3">
        <div 
          className={`flex-shrink-0 ${iconBg} flex items-center justify-center w-8 h-8`}
          style={{ borderRadius: '10px' }}
        >
          <Icon className={`w-4 h-4 ${iconColor}`} />
        </div>
        
        <div className="flex-1 min-w-0">
          <p className={`text-xs font-semibold ${textColor} break-words leading-relaxed`}>
            {notification.message}
          </p>
        </div>

        <button
          onClick={handleRemove}
          className={`flex-shrink-0 ${textColor} hover:opacity-70 transition-opacity`}
        >
          <X className="w-4 h-4" />
        </button>
      </div>

      {/* Progress bar */}
      <div className="h-1 overflow-hidden bg-black/5" style={{ borderRadius: '0 0 10px 10px' }}>
        <div 
          className={`h-full ${
            notification.type === 'success' ? 'bg-green-500' :
            notification.type === 'error' ? 'bg-red-500' :
            'bg-yellow-500'
          }`}
          style={{
            width: '100%',
            animation: 'progress 5s linear forwards',
          }}
        />
      </div>

      <style>{`
        @keyframes progress {
          from { width: 100%; }
          to { width: 0%; }
        }
      `}</style>
    </div>
  );
}

// Demo Component
export default function NotificationDemo() {
  return (
    <NotificationProvider>
      <div className="min-h-screen p-8 bg-gradient-to-br from-violet-50 via-purple-50 to-pink-50">
        <div className="max-w-2xl mx-auto">
          <div 
            className="p-8 bg-white shadow-xl"
            style={{ borderRadius: '20px' }}
          >
            <h1 className="mb-2 text-3xl font-bold text-transparent bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text">
              Hệ thống thông báo
            </h1>
            <p className="mb-8 text-gray-600">Click vào các nút để xem thông báo</p>

            <div className="space-y-4">
              <button
                onClick={() => notify.success('Thao tác thành công! Dữ liệu đã được lưu.')}
                className="w-full py-3.5 text-sm font-semibold text-white bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all"
                style={{ borderRadius: '14px' }}
              >
                Hiển thị thông báo thành công
              </button>

              <button
                onClick={() => notify.error('Đã xảy ra lỗi! Vui lòng thử lại sau.')}
                className="w-full py-3.5 text-sm font-semibold text-white bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all"
                style={{ borderRadius: '14px' }}
              >
                Hiển thị thông báo lỗi
              </button>

              <button
                onClick={() => notify.warning('Cảnh báo: Hành động này không thể hoàn tác!')}
                className="w-full py-3.5 text-sm font-semibold text-white bg-gradient-to-r from-yellow-600 to-yellow-700 hover:from-yellow-700 hover:to-yellow-800 shadow-lg hover:shadow-xl hover:scale-[1.02] active:scale-[0.98] transition-all"
                style={{ borderRadius: '14px' }}
              >
                Hiển thị thông báo cảnh báo
              </button>

              <button
                onClick={() => {
                  notify.success('Đã lưu thành công!');
                  setTimeout(() => notify.warning('Vui lòng kiểm tra lại thông tin'), 500);
                  setTimeout(() => notify.error('Kết nối bị gián đoạn'), 1000);
                }}
                className="w-full py-3.5 text-sm font-semibold text-violet-700 bg-white border-2 border-violet-200 hover:border-violet-400 hover:bg-violet-50 shadow-sm hover:shadow-md hover:scale-[1.02] active:scale-[0.98] transition-all"
                style={{ borderRadius: '14px' }}
              >
                Hiển thị nhiều thông báo
              </button>
            </div>

            <div className="p-4 mt-8 border-2 bg-violet-50 border-violet-200" style={{ borderRadius: '14px' }}>
              <h3 className="mb-2 text-sm font-bold text-violet-900">Cách sử dụng:</h3>
              <code className="block p-3 text-xs bg-white rounded-lg text-violet-800">
                {`notify.success('Thành công!')
notify.error('Có lỗi!')
notify.warning('Cảnh báo!')`}
              </code>
            </div>
          </div>
        </div>
      </div>
    </NotificationProvider>
  );
}