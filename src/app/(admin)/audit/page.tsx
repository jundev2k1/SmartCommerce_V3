import { getTranslations } from 'next-intl/server';
import { PlaceholderModulePage } from '@/shared/layout';

export default async function AuditPage() {
  const t = await getTranslations('modules');
  return <PlaceholderModulePage title={t('audit.title')} description={t('audit.description')} />;
}
