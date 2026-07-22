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
import { RegisterForm } from './RegisterForm';

export function RegisterPage() {
  const t = useTranslations('auth.register');

  return (
    <AppCard className="w-full max-w-sm">
      <AppCardHeader>
        <AppCardTitle>{t('title')}</AppCardTitle>
        <AppCardDescription>{t('subtitle')}</AppCardDescription>
      </AppCardHeader>
      <AppCardContent className="space-y-4">
        <RegisterForm />
        <p className="text-muted-foreground text-center text-sm">
          {t('hasAccount')}{' '}
          <Link
            href="/login"
            className="text-primary font-medium underline-offset-4 hover:underline"
          >
            {t('loginLink')}
          </Link>
        </p>
      </AppCardContent>
    </AppCard>
  );
}
