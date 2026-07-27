'use client';

import { useTranslations } from 'next-intl';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, toast } from '@/shared/ui';
import { updateOrderOwnerInfoSchema, type UpdateOrderOwnerInfoFormValues } from '../orders.schema';
import { useUpdateOrderOwnerInfoMutation } from '../api/orders.queries';

export interface EditOrderOwnerInfoFormProps {
  orderId: string;
  defaultValues: UpdateOrderOwnerInfoFormValues;
  onDone: () => void;
}

export function EditOrderOwnerInfoForm({
  orderId,
  defaultValues,
  onDone,
}: EditOrderOwnerInfoFormProps) {
  const t = useTranslations('orders.detail.editOwnerInfo');
  const tCommon = useTranslations('common.actions');
  const updateMutation = useUpdateOrderOwnerInfoMutation();

  const form = useAppForm<UpdateOrderOwnerInfoFormValues>({
    schema: updateOrderOwnerInfoSchema,
    defaultValues,
  });

  async function handleSubmit(values: UpdateOrderOwnerInfoFormValues) {
    try {
      await updateMutation.mutateAsync({ orderId, values });
      toast.success(t('success'));
      onDone();
    } catch {
      // 400 (order no longer Pending/Confirmed) or 403 (not owner/admin) — shown as a normal error.
      toast.error(t('error'));
    }
  }

  return (
    <Form form={form} onSubmit={handleSubmit} className="space-y-4">
      <FormField<UpdateOrderOwnerInfoFormValues> name="customerPhone" label={t('phone')} />
      <FormField<UpdateOrderOwnerInfoFormValues>
        name="shippingAddress"
        type="textarea"
        label={t('shippingAddress')}
      />
      <div className="flex justify-end gap-2 pt-2">
        <CancelButton type="button" onClick={onDone}>
          {tCommon('cancel')}
        </CancelButton>
        <SubmitButton>{tCommon('save')}</SubmitButton>
      </div>
    </Form>
  );
}
