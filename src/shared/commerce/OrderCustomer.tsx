'use client';

import { useTranslations } from 'next-intl';
import { AppCard, AppCardHeader, AppCardTitle, AppCardContent } from '@/shared/ui';

export interface OrderCustomerProps {
  customerId: string;
}

/**
 * `GetOrderResponse` only has a raw `customerId` — no name/email/phone.
 * There's no cross-service lookup available either: the User service has no
 * "get user by arbitrary id" endpoint, only `GetUserDetail` for the *current*
 * session's own profile (see docs/backend/user/README.md and
 * docs/modules/user-management.md). So this shows the id plainly rather than
 * inventing a name/avatar. See docs/modules/order-management.md.
 */
export function OrderCustomer({ customerId }: OrderCustomerProps) {
  const t = useTranslations('commerce.orderCustomer');
  return (
    <AppCard>
      <AppCardHeader>
        <AppCardTitle>{t('title')}</AppCardTitle>
      </AppCardHeader>
      <AppCardContent className="space-y-1">
        <p className="text-muted-foreground text-sm">{t('idOnlyNote')}</p>
        <p className="font-mono text-sm">{customerId}</p>
      </AppCardContent>
    </AppCard>
  );
}
