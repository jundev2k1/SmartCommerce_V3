'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { ChevronRight } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import {
  AppEmpty,
  AppLoading,
  PrimaryButton,
  SecondaryButton,
  SubmitButton,
  toast,
} from '@/shared/ui';
import { EntityHeader, EntityMetadata } from '@/shared/entity';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { CheckoutSummary } from '@/shared/commerce';
import { useCartQuery } from '@/features/cart';
import { useCurrentUserQuery, toSessionUser } from '@/features/auth';
import { isApiError } from '@/shared/lib/api/types';
import { useCreateOrderMutation } from '../api/checkout.queries';
import { checkoutOwnerSchema, type CheckoutOwnerFormValues } from '../checkout.schema';
import type { CartItemResponse, CreateOrderResponse } from '@/services/order';

type CheckoutStep = 'cart' | 'owner' | 'confirm' | 'complete';

const STEP_ORDER: CheckoutStep[] = ['cart', 'owner', 'confirm', 'complete'];

function CheckoutStepper({ current }: { current: CheckoutStep }) {
  const t = useTranslations('checkout.steps');
  const currentIndex = STEP_ORDER.indexOf(current);

  return (
    <ol className="flex items-center gap-1 text-sm">
      {STEP_ORDER.map((step, index) => (
        <li key={step} className="flex items-center gap-1">
          <span
            className={cn(
              'text-muted-foreground',
              index === currentIndex && 'text-foreground font-medium',
              index < currentIndex && 'text-foreground',
            )}
          >
            {t(step)}
          </span>
          {index < STEP_ORDER.length - 1 ? (
            <ChevronRight className="text-muted-foreground size-4" />
          ) : null}
        </li>
      ))}
    </ol>
  );
}

function CartReviewStep({
  items,
  onContinue,
  onEditCart,
}: {
  items: CartItemResponse[];
  onContinue: () => void;
  onEditCart: () => void;
}) {
  const t = useTranslations('checkout');

  return (
    <div className="grid gap-6 lg:grid-cols-3">
      <div className="lg:col-span-2">
        <CheckoutSummary items={items} />
      </div>
      <div className="space-y-2">
        <PrimaryButton className="w-full" onClick={onContinue}>
          {t('continueButton')}
        </PrimaryButton>
        <SecondaryButton className="w-full" onClick={onEditCart}>
          {t('cartStep.editCart')}
        </SecondaryButton>
      </div>
    </div>
  );
}

function OwnerInfoStep({
  defaultValues,
  onBack,
  onSubmit,
}: {
  defaultValues: CheckoutOwnerFormValues;
  onBack: () => void;
  onSubmit: (values: CheckoutOwnerFormValues) => void;
}) {
  const t = useTranslations('checkout');
  const form = useAppForm<CheckoutOwnerFormValues>({
    schema: checkoutOwnerSchema,
    defaultValues,
  });

  return (
    <div className="max-w-lg space-y-4">
      <div className="space-y-1">
        <h2 className="font-medium">{t('ownerStep.title')}</h2>
        <p className="text-muted-foreground text-sm">{t('ownerStep.description')}</p>
      </div>
      <Form form={form} onSubmit={onSubmit} className="space-y-4">
        <FormField<CheckoutOwnerFormValues>
          name="customerName"
          label={t('ownerStep.customerName')}
        />
        <FormField<CheckoutOwnerFormValues>
          name="customerPhone"
          label={t('ownerStep.customerPhone')}
        />
        <FormField<CheckoutOwnerFormValues>
          name="shippingAddress"
          type="textarea"
          label={t('ownerStep.shippingAddress')}
        />
        <div className="flex gap-2">
          <SecondaryButton type="button" onClick={onBack}>
            {t('back')}
          </SecondaryButton>
          <SubmitButton>{t('continueButton')}</SubmitButton>
        </div>
      </Form>
    </div>
  );
}

function ConfirmStep({
  owner,
  items,
  onBack,
  onPlaceOrder,
  placing,
}: {
  owner: CheckoutOwnerFormValues;
  items: CartItemResponse[];
  onBack: () => void;
  onPlaceOrder: () => void;
  placing: boolean;
}) {
  const t = useTranslations('checkout');

  return (
    <div className="grid gap-6 lg:grid-cols-3">
      <div className="space-y-4 lg:col-span-2">
        <div className="space-y-1">
          <h2 className="font-medium">{t('confirmStep.title')}</h2>
          <p className="text-muted-foreground text-sm">{t('confirmStep.description')}</p>
        </div>
        <div className="space-y-2">
          <p className="text-sm font-medium">{t('confirmStep.orderOwner')}</p>
          <EntityMetadata
            items={[
              { label: t('ownerStep.customerName'), value: owner.customerName },
              { label: t('ownerStep.customerPhone'), value: owner.customerPhone },
              { label: t('ownerStep.shippingAddress'), value: owner.shippingAddress },
            ]}
          />
        </div>
        <CheckoutSummary items={items} />
      </div>
      <div className="space-y-2">
        <PrimaryButton className="w-full" loading={placing} onClick={onPlaceOrder}>
          {t('placeOrder')}
        </PrimaryButton>
        <SecondaryButton className="w-full" disabled={placing} onClick={onBack}>
          {t('back')}
        </SecondaryButton>
      </div>
    </div>
  );
}

export function CheckoutPage() {
  const t = useTranslations('checkout');
  const router = useRouter();
  const { data: cart, isLoading: cartLoading, isError: cartIsError } = useCartQuery();
  const { data: user, isLoading: userLoading } = useCurrentUserQuery();
  const createOrderMutation = useCreateOrderMutation();
  const [step, setStep] = useState<Exclude<CheckoutStep, 'complete'>>('cart');
  const [owner, setOwner] = useState<CheckoutOwnerFormValues | null>(null);
  const [placedOrder, setPlacedOrder] = useState<CreateOrderResponse | null>(null);

  if (placedOrder) {
    return (
      <div className="space-y-4">
        <EntityHeader title={t('successTitle')} />
        <CheckoutStepper current="complete" />
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

  if (!user || !cart) {
    return <AppLoading />;
  }

  async function handlePlaceOrder(ownerInfo: CheckoutOwnerFormValues) {
    if (!user || !cart) return;
    try {
      const result = await createOrderMutation.mutateAsync({
        customerName: ownerInfo.customerName,
        customerPhone: ownerInfo.customerPhone,
        shippingAddress: ownerInfo.shippingAddress,
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
      <CheckoutStepper current={step} />
      {step === 'cart' ? (
        <CartReviewStep
          items={items}
          onContinue={() => setStep('owner')}
          onEditCart={() => router.push('/cart')}
        />
      ) : null}
      {step === 'owner' ? (
        <OwnerInfoStep
          defaultValues={
            owner ?? {
              customerName: toSessionUser(user).displayName,
              customerPhone: user.phoneNumber ?? '',
              shippingAddress: '',
            }
          }
          onBack={() => setStep('cart')}
          onSubmit={(values) => {
            setOwner(values);
            setStep('confirm');
          }}
        />
      ) : null}
      {step === 'confirm' && owner ? (
        <ConfirmStep
          owner={owner}
          items={items}
          onBack={() => setStep('owner')}
          onPlaceOrder={() => handlePlaceOrder(owner)}
          placing={createOrderMutation.isPending}
        />
      ) : null}
    </div>
  );
}
