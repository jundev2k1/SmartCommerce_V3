'use client';

import { useEffect, type ReactNode } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { AppLoading } from '@/shared/ui';
import { useCurrentUserQuery } from '../api/auth.queries';

/** Wraps guest-only (auth) routes (login/register) — redirects to / if already authenticated. */
export function GuestGuard({ children }: { children: ReactNode }) {
  const router = useRouter();
  const t = useTranslations('auth.session');
  const { data, isLoading } = useCurrentUserQuery();

  useEffect(() => {
    if (!isLoading && data) {
      router.replace('/');
    }
  }, [isLoading, data, router]);

  if (isLoading) {
    return <AppLoading label={t('checking')} className="min-h-svh" />;
  }

  if (data) {
    return <AppLoading label={t('checking')} className="min-h-svh" />;
  }

  return <>{children}</>;
}
