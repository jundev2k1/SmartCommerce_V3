'use client';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { AppModal, SubmitButton, CancelButton, toast } from '@/shared/ui';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { createWarehouseSchema, type CreateWarehouseFormValues } from '../inventory.schema';
import { useCreateWarehouseMutation } from '../api/inventory.queries';

export interface WarehouseCreateDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function WarehouseCreateDialog({ open, onOpenChange }: WarehouseCreateDialogProps) {
  const t = useTranslations('inventory.warehouses');
  const tCommon = useTranslations('common.actions');
  const router = useRouter();
  const createMutation = useCreateWarehouseMutation();

  const form = useAppForm<CreateWarehouseFormValues>({
    schema: createWarehouseSchema,
    defaultValues: { code: '', name: '', address: '' },
  });

  async function handleSubmit(values: CreateWarehouseFormValues) {
    try {
      const result = await createMutation.mutateAsync(values);
      toast.success(t('toast.createSuccess'));
      onOpenChange(false);
      router.push(`/warehouses/${result.warehouseId}`);
    } catch {
      toast.error(t('toast.createError'));
    }
  }

  return (
    <AppModal open={open} onOpenChange={onOpenChange} title={t('createDialog.title')}>
      <Form form={form} onSubmit={handleSubmit} className="space-y-4">
        <FormField<CreateWarehouseFormValues> name="code" label={t('createDialog.fields.code')} />
        <FormField<CreateWarehouseFormValues> name="name" label={t('createDialog.fields.name')} />
        <FormField<CreateWarehouseFormValues>
          name="address"
          type="textarea"
          label={t('createDialog.fields.address')}
        />
        <div className="flex justify-end gap-2 pt-2">
          <CancelButton type="button" onClick={() => onOpenChange(false)}>
            {tCommon('cancel')}
          </CancelButton>
          <SubmitButton>{tCommon('create')}</SubmitButton>
        </div>
      </Form>
    </AppModal>
  );
}
