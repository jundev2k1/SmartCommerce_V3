'use client';

import { Trash2 } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import {
  AppCard,
  AppCardContent,
  AppEmpty,
  AppLoading,
  IconButton,
  PrimaryButton,
  SecondaryButton,
} from '@/shared/ui';
import { EntityHeader } from '@/shared/entity';
import { CartSummary, ProductPrice, QuantitySelector } from '@/shared/commerce';
import {
  useCartQuery,
  useUpdateCartItemQuantityMutation,
  useRemoveCartItemMutation,
  useClearCartMutation,
} from '../api/cart.queries';

export function CartPage() {
  const t = useTranslations('cart');
  const router = useRouter();
  const { data: cart, isLoading, isError } = useCartQuery();
  const updateQuantityMutation = useUpdateCartItemQuantityMutation();
  const removeItemMutation = useRemoveCartItemMutation();
  const clearMutation = useClearCartMutation();

  if (isLoading) {
    return <AppLoading />;
  }

  if (isError) {
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

  return (
    <div className="space-y-4">
      <EntityHeader
        title={t('title')}
        actions={
          <SecondaryButton onClick={() => clearMutation.mutate()}>{t('clearCart')}</SecondaryButton>
        }
      />

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-3 lg:col-span-2">
          {items.map((item) => (
            <AppCard key={item.variationId}>
              <AppCardContent className="flex flex-wrap items-center gap-4 py-4">
                <div className="min-w-0 flex-1">
                  <p className="font-medium">{item.productName}</p>
                  <p className="text-sm font-medium">
                    <ProductPrice value={item.unitPrice} />
                  </p>
                </div>
                <QuantitySelector
                  value={item.quantity}
                  onChange={(quantity) =>
                    updateQuantityMutation.mutate({ variationId: item.variationId, quantity })
                  }
                />
                <IconButton
                  aria-label={t('remove')}
                  onClick={() => removeItemMutation.mutate(item.variationId)}
                >
                  <Trash2 />
                </IconButton>
              </AppCardContent>
            </AppCard>
          ))}
        </div>

        <div>
          <CartSummary
            items={items}
            action={
              <PrimaryButton className="w-full" onClick={() => router.push('/checkout')}>
                {t('proceedToCheckout')}
              </PrimaryButton>
            }
          />
        </div>
      </div>
    </div>
  );
}
