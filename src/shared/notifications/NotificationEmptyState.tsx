'use client';

import { useTranslations } from 'next-intl';
import { AppEmpty } from '@/shared/ui';

export interface NotificationEmptyStateProps {
  variant?: 'empty' | 'error';
  onRetry?: () => void;
}

/** Thin default-copy wrapper over AppEmpty for the notification dropdown/bell. */
export function NotificationEmptyState({
  variant = 'empty',
  onRetry,
}: NotificationEmptyStateProps) {
  const t = useTranslations('notificationsUi.emptyState');

  if (variant === 'error') {
    return (
      <AppEmpty
        title={t('errorTitle')}
        description={t('errorDescription')}
        action={
          onRetry ? (
            <button
              type="button"
              onClick={onRetry}
              className="text-primary text-sm font-medium underline"
            >
              {t('retry')}
            </button>
          ) : undefined
        }
      />
    );
  }

  return <AppEmpty title={t('emptyTitle')} description={t('emptyDescription')} />;
}
