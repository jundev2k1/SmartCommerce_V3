'use client';

import { useEffect, useState } from 'react';
import { useTranslations } from 'next-intl';

export interface NotificationTimestampProps {
  value: string;
  className?: string;
}

function relativeUnits(diffMs: number): {
  amount: number;
  unit: 'justNow' | 'minutes' | 'hours' | 'days';
} {
  const minutes = Math.floor(diffMs / 60_000);
  if (minutes < 1) return { amount: 0, unit: 'justNow' };
  if (minutes < 60) return { amount: minutes, unit: 'minutes' };
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return { amount: hours, unit: 'hours' };
  const days = Math.floor(hours / 24);
  return { amount: days, unit: 'days' };
}

/**
 * Relative time ("5m ago", "3h ago", "2d ago") computed from the real
 * `createdAt` timestamp. `now` is state (not `Date.now()` called directly
 * during render, which React Compiler flags as an impure read) — lazily
 * initialized once and refreshed every 30s via an effect, which also means
 * the label updates live while a dropdown stays open.
 */
export function NotificationTimestamp({ value, className }: NotificationTimestampProps) {
  const t = useTranslations('notificationsUi.timestamp');
  const [now, setNow] = useState(() => Date.now());

  useEffect(() => {
    const interval = setInterval(() => setNow(Date.now()), 30_000);
    return () => clearInterval(interval);
  }, []);

  const { amount, unit } = relativeUnits(now - new Date(value).getTime());

  return (
    <time dateTime={value} className={className} title={new Date(value).toLocaleString()}>
      {unit === 'justNow' ? t('justNow') : t(unit, { count: amount })}
    </time>
  );
}
