'use client';

import { useTranslations } from 'next-intl';
import { AppModal } from '@/shared/ui';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, toast } from '@/shared/ui';
import { ImageUrlListField } from '@/shared/entity';
import { variationFormSchema, type VariationFormValues } from '../products.schema';
import { useAddVariationMutation, useUpdateVariationMutation } from '../api/products.queries';
import type { ProductVariationResponse } from '@/services/product';

export type VariantFormDialogState =
  | { mode: 'create'; productId: string }
  | { mode: 'edit'; productId: string; variation: ProductVariationResponse };

export interface VariantFormDialogProps {
  state: VariantFormDialogState | null;
  onOpenChange: (open: boolean) => void;
}

export function VariantFormDialog({ state, onOpenChange }: VariantFormDialogProps) {
  const t = useTranslations('products.variants.formDialog');
  const tCommon = useTranslations('common.actions');
  const tToast = useTranslations('products.toast');
  const addMutation = useAddVariationMutation();
  const updateMutation = useUpdateVariationMutation();

  const isEdit = state?.mode === 'edit';
  const defaultValues: VariationFormValues = isEdit
    ? {
        sku: state.variation.sku ?? '',
        price: String(state.variation.price),
        barcode: state.variation.barcode ?? '',
        cost: state.variation.cost != null ? String(state.variation.cost) : undefined,
        weight: state.variation.weight != null ? String(state.variation.weight) : undefined,
        status: state.variation.status ?? '',
        images: state.variation.images ?? [],
      }
    : { sku: '', price: '', barcode: '', images: [] };

  const form = useAppForm<VariationFormValues>({ schema: variationFormSchema, defaultValues });

  async function handleSubmit(values: VariationFormValues) {
    if (!state) return;
    try {
      if (state.mode === 'create') {
        await addMutation.mutateAsync({ productId: state.productId, values });
        toast.success(tToast('variationAdded'));
      } else {
        await updateMutation.mutateAsync({
          productId: state.productId,
          variationId: state.variation.id,
          values,
        });
        toast.success(tToast('variationUpdated'));
      }
      onOpenChange(false);
    } catch {
      toast.error(tToast(state.mode === 'create' ? 'createError' : 'updateError'));
    }
  }

  return (
    <AppModal
      open={state !== null}
      onOpenChange={onOpenChange}
      title={isEdit ? t('editTitle') : t('createTitle')}
    >
      {state ? (
        <Form
          form={form}
          onSubmit={handleSubmit}
          className="space-y-4"
          key={isEdit ? state.variation.id : 'create'}
        >
          <div className="grid grid-cols-2 gap-4">
            <FormField<VariationFormValues> name="sku" label={t('sku')} />
            <FormField<VariationFormValues> name="price" type="number" label={t('price')} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <FormField<VariationFormValues> name="barcode" label={t('barcode')} />
            <FormField<VariationFormValues> name="cost" type="number" label={t('cost')} />
          </div>
          {isEdit ? <FormField<VariationFormValues> name="status" label={t('status')} /> : null}
          <div className="space-y-1.5">
            <label className="text-sm font-medium">{t('images')}</label>
            <ImageUrlListField
              value={form.watch('images') ?? []}
              onChange={(urls) => form.setValue('images', urls)}
              addLabel={t('images')}
            />
          </div>
          <div className="flex justify-end gap-2 pt-2">
            <CancelButton type="button" onClick={() => onOpenChange(false)}>
              {tCommon('cancel')}
            </CancelButton>
            <SubmitButton>{isEdit ? tCommon('save') : tCommon('create')}</SubmitButton>
          </div>
        </Form>
      ) : null}
    </AppModal>
  );
}
