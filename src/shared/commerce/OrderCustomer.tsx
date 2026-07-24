'use client';

import { useTranslations } from 'next-intl';
import { AppCard, AppCardHeader, AppCardTitle, AppCardContent } from '@/shared/ui';

export interface OrderCustomerProps {
  customerId: string;
  customerName: string;
  customerPhone: string;
}

export function OrderCustomer({ customerId, customerName, customerPhone }: OrderCustomerProps) {
  const t = useTranslations('commerce.orderCustomer');
  return (
    <AppCard>
      <AppCardHeader>
        <AppCardTitle>{t('title')}</AppCardTitle>
      </AppCardHeader>
      <AppCardContent className="space-y-2">
        <div>
          <p className="text-muted-foreground text-xs">{t('name')}</p>
          <p className="text-sm font-medium">{customerName}</p>
        </div>
        <div>
          <p className="text-muted-foreground text-xs">{t('phone')}</p>
          <p className="text-sm font-medium">{customerPhone}</p>
        </div>
        <div>
          <p className="text-muted-foreground text-xs">{t('id')}</p>
          <p className="font-mono text-xs">{customerId}</p>
        </div>
      </AppCardContent>
    </AppCard>
  );
}
