'use client';

import { getRequiredRolesForPath, hasRequiredRole } from '@/shared/config/nav';
import { connect } from '@/shared/lib/realtime/signalr-client';
import { useSessionStore } from '@/shared/stores/session.store';
import { AppLoading } from '@/shared/ui';
import { useTranslations } from 'next-intl';
import { usePathname, useRouter } from 'next/navigation';
import { useEffect, type ReactNode } from 'react';
import { toSessionUser, useCurrentUserQuery } from '../api/auth.queries';

/**
 * Wraps protected (admin) routes. Redirects to /login if the session check
 * fails — any query error is treated as "not authenticated" for guard
 * purposes, since we can't distinguish a real 401 from a transient failure
 * without the backend documenting more error detail (see
 * docs/backend/user/README.md). Also enforces route-level role access
 * (redirecting to /403) using the same nav.ts role data the sidebar filters
 * on, so hiding a nav entry and blocking direct navigation to its route stay
 * in sync — see docs/modules/order-management.md.
 */
export function AuthGuard({ children }: { children: ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const t = useTranslations('auth.session');
  const { data, isLoading, isError } = useCurrentUserQuery();
  const setSession = useSessionStore((s) => s.setSession);

  useEffect(() => {
    if (data) {
      setSession(toSessionUser(data));
      // Connect the shared SignalR hub once a session is confirmed — see
      // docs/realtime/signalr-strategy.md. No real event catalog is published
      // yet, so a failed/no-op connection here is expected without a live
      // backend; features subscribe to events independently via useHubEvent.
      connect().catch(() => undefined);
    }
  }, [data, setSession]);

  useEffect(() => {
    if (!isLoading && (isError || !data)) {
      router.replace('/login');
    }
  }, [isLoading, isError, data, router]);

  const requiredRoles = data ? getRequiredRolesForPath(pathname) : undefined;
  const roleAllowed = hasRequiredRole(data?.roles ?? [], requiredRoles);

  useEffect(() => {
    if (data && !roleAllowed) {
      router.replace('/403');
    }
  }, [data, roleAllowed, router]);

  if (isLoading || !data || !roleAllowed) {
    return <AppLoading label={t('checking')} className="min-h-svh" />;
  }

  return <>{children}</>;
}
