'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { AppEmpty, AppLoading, PrimaryButton, SecondaryButton, toast } from '@/shared/ui';
import { EntityHeader } from '@/shared/entity';
import { CheckoutSummary } from '@/shared/commerce';
import { useCartQuery } from '@/features/cart';
import { useCurrentUserQuery, toSessionUser } from '@/features/auth';
import { isApiError } from '@/shared/lib/api/types';
import { useCreateOrderMutation } from '../api/checkout.queries';
import type { CreateOrderResponse } from '@/services/order';

export function CheckoutPage() {
  const t = useTranslations('checkout');
  const router = useRouter();
  const { data: cart, isLoading: cartLoading, isError: cartIsError } = useCartQuery();
  const { data: user, isLoading: userLoading } = useCurrentUserQuery();
  const createOrderMutation = useCreateOrderMutation();
  const [placedOrder, setPlacedOrder] = useState<CreateOrderResponse | null>(null);

  if (placedOrder) {
    return (
      <div className="space-y-4">
        <EntityHeader title={t('successTitle')} />
        <AppEmpty
          title={t('successTitle')}
          description={t('successDescription', {
            orderId: placedOrder.orderId,
            total: placedOrder.totalAmount.toFixed(2),
          })}
          action={
            <div className="flex gap-2">
              <PrimaryButton onClick={() => router.push(`/orders/${placedOrder.orderId}`)}>
                {t('viewOrder')}
              </PrimaryButton>
              <SecondaryButton onClick={() => router.push('/shop')}>
                {t('continueShopping')}
              </SecondaryButton>
            </div>
          }
        />
      </div>
    );
  }

  if (cartLoading || userLoading) {
    return <AppLoading />;
  }

  if (cartIsError) {
    return (
      <div className="space-y-4">
        <EntityHeader title={t('title')} />
        <AppEmpty title={t('errorTitle')} description={t('errorDescription')} />
      </div>
    );
  }

  const items = cart?.items ?? [];

  if (items.length === 0) {
    return (
      <div className="space-y-4">
        <EntityHeader title={t('title')} />
        <AppEmpty
          title={t('emptyTitle')}
          description={t('emptyDescription')}
          action={
            <PrimaryButton onClick={() => router.push('/shop')}>
              {t('browseProducts')}
            </PrimaryButton>
          }
        />
      </div>
    );
  }

  async function handlePlaceOrder() {
    if (!user || !cart) return;
    try {
      const result = await createOrderMutation.mutateAsync({
        customerName: toSessionUser(user).displayName,
        customerPhone: user.phoneNumber ?? '',
        items: cart.items.map((item) => ({
          productId: item.productId,
          variationId: item.variationId,
          quantity: item.quantity,
        })),
      });
      setPlacedOrder(result);
      toast.success(t('toast.success'));
    } catch (err) {
      if (isApiError(err) && err.status === 409) {
        toast.error(t('toast.conflict'));
      } else {
        toast.error(t('toast.error'));
      }
    }
  }

  return (
    <div className="space-y-4">
      <EntityHeader title={t('title')} />
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <CheckoutSummary items={items} />
        </div>
        <div>
          <PrimaryButton
            className="w-full"
            disabled={!user || createOrderMutation.isPending}
            onClick={handlePlaceOrder}
          >
            {t('placeOrder')}
          </PrimaryButton>
        </div>
      </div>
    </div>
  );
}
