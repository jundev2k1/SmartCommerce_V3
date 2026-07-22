'use client';

import Link from 'next/link';
import { useTranslations } from 'next-intl';
import {
  AppCard,
  AppCardHeader,
  AppCardTitle,
  AppCardDescription,
  AppCardContent,
} from '@/shared/ui';
import { LoginForm } from './LoginForm';

export function LoginPage() {
  const t = useTranslations('auth.login');

  return (
    <AppCard className="w-full max-w-sm">
      <AppCardHeader>
        <AppCardTitle>{t('title')}</AppCardTitle>
        <AppCardDescription>{t('subtitle')}</AppCardDescription>
      </AppCardHeader>
      <AppCardContent className="space-y-4">
        <LoginForm />
        <p className="text-muted-foreground text-center text-sm">
          {t('noAccount')}{' '}
          <Link
            href="/register"
            className="text-primary font-medium underline-offset-4 hover:underline"
          >
            {t('registerLink')}
          </Link>
        </p>
      </AppCardContent>
    </AppCard>
  );
}
