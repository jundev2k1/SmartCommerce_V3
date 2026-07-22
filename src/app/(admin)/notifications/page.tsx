import { getTranslations } from 'next-intl/server';
import { PlaceholderModulePage } from '@/shared/layout';

export default async function NotificationsPage() {
  const t = await getTranslations('modules');
  return (
    <PlaceholderModulePage
      title={t('notifications.title')}
      description={t('notifications.description')}
    />
  );
}
