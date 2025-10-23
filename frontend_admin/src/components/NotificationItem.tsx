// ==================== components/NotificationItem.tsx ====================
import React, { useState, useEffect } from 'react';
import { CheckCircle, AlertCircle, AlertTriangle, X, Info } from 'lucide-react';
import { Notification } from '../types';

interface NotificationItemProps {
  notification: Notification;
  onRemove: (id: number) => void;
}

export default function NotificationItem({ notification, onRemove }: NotificationItemProps) {
  const [isVisible, setIsVisible] = useState(false);
  const [isPaused, setIsPaused] = useState(false);

  useEffect(() => {
    setTimeout(() => setIsVisible(true), 10);
  }, []);

  const handleRemove = () => {
    setIsVisible(false);
    setTimeout(() => onRemove(notification.id), 300);
  };

  const config = {
    success: {
      bg: 'bg-gradient-to-r from-green-50 to-emerald-50',
      border: 'border-green-300',
      iconBg: 'bg-green-500',
      icon: CheckCircle,
      iconColor: 'text-white',
      textColor: 'text-green-900',
      progressBg: 'bg-green-500',
    },
    error: {
      bg: 'bg-gradient-to-r from-red-50 to-rose-50',
      border: 'border-red-300',
      iconBg: 'bg-red-500',
      icon: AlertCircle,
      iconColor: 'text-white',
      textColor: 'text-red-900',
      progressBg: 'bg-red-500',
    },
    warning: {
      bg: 'bg-gradient-to-r from-yellow-50 to-amber-50',
      border: 'border-yellow-300',
      iconBg: 'bg-yellow-500',
      icon: AlertTriangle,
      iconColor: 'text-white',
      textColor: 'text-yellow-900',
      progressBg: 'bg-yellow-500',
    },
    info: {
      bg: 'bg-gradient-to-r from-blue-50 to-cyan-50',
      border: 'border-blue-300',
      iconBg: 'bg-blue-500',
      icon: Info,
      iconColor: 'text-white',
      textColor: 'text-blue-900',
      progressBg: 'bg-blue-500',
    },
  };

  const { bg, border, iconBg, icon: Icon, iconColor, textColor, progressBg } = config[notification.type];

  return (
    <div
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
      className={`${bg} ${border} border shadow-lg backdrop-blur-sm pointer-events-auto transition-all duration-300 transform overflow-hidden ${
        isVisible ? 'translate-x-0 opacity-100 scale-100' : 'translate-x-full opacity-0 scale-95'
      } hover:scale-[1.02] hover:shadow-xl`}
      style={{ borderRadius: '12px' }}
    >
      <div className="flex items-start gap-2 p-2.5 sm:gap-2.5 sm:p-3">
        <div 
          className={`flex-shrink-0 ${iconBg} flex items-center justify-center w-7 h-7 sm:w-8 sm:h-8 shadow-md`}
          style={{ borderRadius: '8px' }}
        >
          <Icon className={`w-3.5 h-3.5 sm:w-4 sm:h-4 ${iconColor}`} />
        </div>
        
        <div className="flex-1 min-w-0 pt-0.5">
          <p className={`text-xs sm:text-sm font-semibold ${textColor} break-words leading-snug`}>
            {notification.message}
          </p>
        </div>

        <button
          onClick={handleRemove}
          className={`flex-shrink-0 ${textColor} hover:opacity-70 transition-all hover:rotate-90 duration-200 p-0.5 -mt-0.5 -mr-0.5`}
          aria-label="Đóng thông báo"
        >
          <X className="w-3.5 h-3.5 sm:w-4 sm:h-4" />
        </button>
      </div>

      <div className="relative h-0.5 bg-black/10">
        <div 
          className={`absolute top-0 left-0 h-full ${progressBg}`}
          style={{
            width: '100%',
            animation: isPaused ? 'none' : 'progress 5s linear forwards',
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