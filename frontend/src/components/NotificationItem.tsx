// ==================== components/NotificationItem.tsx ====================
import React, { useState, useEffect } from 'react';
import { CheckCircle, AlertCircle, AlertTriangle, X } from 'lucide-react';
import { Notification } from '../types';

interface NotificationItemProps {
  notification: Notification;
  onRemove: (id: number) => void;
}

export default function NotificationItem({ notification, onRemove }: NotificationItemProps) {
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