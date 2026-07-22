'use client';

import { useTranslations } from 'next-intl';
import { AppModal, SubmitButton, CancelButton, toast } from '@/shared/ui';
import { useAppForm, Form, FormField } from '@/shared/forms';
import {
  stockInSchema,
  stockOutSchema,
  adjustStockSchema,
  type StockInFormValues,
  type StockOutFormValues,
  type AdjustStockFormValues,
} from '../inventory.schema';
import {
  useStockInMutation,
  useStockOutMutation,
  useAdjustStockMutation,
} from '../api/inventory.queries';

export type StockActionMode = 'in' | 'out' | 'adjust';

export interface StockActionDialogProps {
  mode: StockActionMode | null;
  inventoryId: string;
  onOpenChange: (open: boolean) => void;
}

/**
 * One dialog for all three write actions (StockIn/StockOut/Adjust) since
 * they share the same {quantity-ish, reason} shape and mutation pattern —
 * three near-identical dialog files would just be drift-prone duplication.
 */
export function StockActionDialog({ mode, inventoryId, onOpenChange }: StockActionDialogProps) {
  const t = useTranslations('inventory.stockDetail.actions');
  const tCommon = useTranslations('common.actions');
  const stockInMutation = useStockInMutation();
  const stockOutMutation = useStockOutMutation();
  const adjustMutation = useAdjustStockMutation();

  const stockInForm = useAppForm<StockInFormValues>({
    schema: stockInSchema,
    defaultValues: { quantity: '', reason: '' },
  });
  const stockOutForm = useAppForm<StockOutFormValues>({
    schema: stockOutSchema,
    defaultValues: { quantity: '', reason: '' },
  });
  const adjustForm = useAppForm<AdjustStockFormValues>({
    schema: adjustStockSchema,
    defaultValues: { newQuantity: '', reason: '' },
  });

  async function handleStockIn(values: StockInFormValues) {
    try {
      await stockInMutation.mutateAsync({ inventoryId, values });
      toast.success(t('stockInSuccess'));
      onOpenChange(false);
    } catch {
      toast.error(t('actionError'));
    }
  }

  async function handleStockOut(values: StockOutFormValues) {
    try {
      await stockOutMutation.mutateAsync({ inventoryId, values });
      toast.success(t('stockOutSuccess'));
      onOpenChange(false);
    } catch {
      toast.error(t('actionError'));
    }
  }

  async function handleAdjust(values: AdjustStockFormValues) {
    try {
      await adjustMutation.mutateAsync({ inventoryId, values });
      toast.success(t('adjustSuccess'));
      onOpenChange(false);
    } catch {
      toast.error(t('actionError'));
    }
  }

  return (
    <AppModal
      open={mode !== null}
      onOpenChange={onOpenChange}
      title={
        mode === 'in' ? t('stockInTitle') : mode === 'out' ? t('stockOutTitle') : t('adjustTitle')
      }
    >
      {mode === 'in' ? (
        <Form form={stockInForm} onSubmit={handleStockIn} className="space-y-4">
          <FormField<StockInFormValues> name="quantity" type="number" label={t('quantity')} />
          <FormField<StockInFormValues> name="reason" label={t('reason')} />
          <div className="flex justify-end gap-2 pt-2">
            <CancelButton type="button" onClick={() => onOpenChange(false)}>
              {tCommon('cancel')}
            </CancelButton>
            <SubmitButton>{tCommon('save')}</SubmitButton>
          </div>
        </Form>
      ) : mode === 'out' ? (
        <Form form={stockOutForm} onSubmit={handleStockOut} className="space-y-4">
          <FormField<StockOutFormValues> name="quantity" type="number" label={t('quantity')} />
          <FormField<StockOutFormValues> name="reason" label={t('reason')} />
          <div className="flex justify-end gap-2 pt-2">
            <CancelButton type="button" onClick={() => onOpenChange(false)}>
              {tCommon('cancel')}
            </CancelButton>
            <SubmitButton>{tCommon('save')}</SubmitButton>
          </div>
        </Form>
      ) : mode === 'adjust' ? (
        <Form form={adjustForm} onSubmit={handleAdjust} className="space-y-4">
          <FormField<AdjustStockFormValues>
            name="newQuantity"
            type="number"
            label={t('newQuantity')}
          />
          <FormField<AdjustStockFormValues> name="reason" label={t('reason')} />
          <div className="flex justify-end gap-2 pt-2">
            <CancelButton type="button" onClick={() => onOpenChange(false)}>
              {tCommon('cancel')}
            </CancelButton>
            <SubmitButton>{tCommon('save')}</SubmitButton>
          </div>
        </Form>
      ) : null}
    </AppModal>
  );
}
