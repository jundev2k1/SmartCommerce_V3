'use client';

import { useTranslations } from 'next-intl';
import { DeleteButton } from '@/shared/ui';

export interface OrderActionsProps {
  onCancel: () => void;
  cancelling?: boolean;
}

/**
 * Extracted so the action row isn't re-inlined per order screen. Only Cancel
 * is exposed here — `CompleteOrder` (admin-only "Approve") lives in
 * `OrderApproveListPage` instead, not this customer-facing detail action bar.
 * Not disabled based on a client-side terminal-status check — the server's
 * own 400 for an already-cancelled/completed order surfaces as a normal
 * error toast instead.
 */
export function OrderActions({ onCancel, cancelling }: OrderActionsProps) {
  const t = useTranslations('commerce.orderActions');
  return (
    <div className="flex flex-wrap gap-2">
      <DeleteButton onClick={onCancel} loading={cancelling}>
        {t('cancelOrder')}
      </DeleteButton>
    </div>
  );
}
