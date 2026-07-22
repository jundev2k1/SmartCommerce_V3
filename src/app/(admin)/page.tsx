'use client';

import { useQuery } from '@tanstack/react-query';
import { useTranslations } from 'next-intl';
import { apiClient } from '@/shared/lib/api/client';
import { AppCard, AppCardHeader, AppCardTitle, AppCardContent, AppSkeleton } from '@/shared/ui';

interface DashboardSummary {
  totalOrders: number;
  pendingOrders: number;
  lowStockItems: number;
}

/**
 * Proves the Axios -> interceptor -> TanStack Query round trip against the
 * dev-only mock adapter (see docs/roadmap.md Phase 1 completion criteria).
 * Replaced by a real dashboard feature once there's business data to show.
 */
function useDashboardSummary() {
  return useQuery({
    queryKey: ['dashboard', 'summary'],
    queryFn: () => apiClient.get<DashboardSummary>('/dashboard/summary').then((res) => res.data),
  });
}

export default function DashboardHomePage() {
  const t = useTranslations('dashboard');
  const { data, isLoading } = useDashboardSummary();

  const stats = [
    { key: 'totalOrders', label: t('stats.totalOrders'), value: data?.totalOrders },
    { key: 'pendingOrders', label: t('stats.pendingOrders'), value: data?.pendingOrders },
    { key: 'lowStockItems', label: t('stats.lowStockItems'), value: data?.lowStockItems },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">{t('welcome')}</h1>
        <p className="text-muted-foreground">{t('subtitle')}</p>
      </div>
      <div className="grid gap-4 sm:grid-cols-3">
        {stats.map((stat) => (
          <AppCard key={stat.key}>
            <AppCardHeader>
              <AppCardTitle className="text-muted-foreground text-sm font-medium">
                {stat.label}
              </AppCardTitle>
            </AppCardHeader>
            <AppCardContent>
              {isLoading ? (
                <AppSkeleton className="h-8 w-16" />
              ) : (
                <p className="text-3xl font-semibold">{stat.value}</p>
              )}
            </AppCardContent>
          </AppCard>
        ))}
      </div>
    </div>
  );
}
