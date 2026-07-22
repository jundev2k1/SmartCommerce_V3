'use client';

import { useTranslations } from 'next-intl';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, toast } from '@/shared/ui';
import { updateProductSchema, type UpdateProductFormValues } from '../products.schema';
import { useUpdateProductMutation } from '../api/products.queries';
import type { GetProductResponse } from '@/services/product';

export function ProductGeneralTab({ product }: { product: GetProductResponse }) {
  const t = useTranslations('products.detail.generalForm');
  const tToast = useTranslations('products.toast');
  const updateMutation = useUpdateProductMutation();

  const form = useAppForm<UpdateProductFormValues>({
    schema: updateProductSchema,
    defaultValues: {
      name: product.name ?? '',
      description: product.description ?? '',
      slug: product.slug ?? '',
    },
  });

  async function handleSubmit(values: UpdateProductFormValues) {
    try {
      await updateMutation.mutateAsync({ productId: product.id, values });
      toast.success(tToast('updateSuccess'));
    } catch {
      toast.error(tToast('updateError'));
    }
  }

  return (
    <Form form={form} onSubmit={handleSubmit} className="max-w-lg space-y-4">
      <FormField<UpdateProductFormValues> name="name" label={t('name')} />
      <FormField<UpdateProductFormValues> name="slug" label={t('slug')} />
      <FormField<UpdateProductFormValues>
        name="description"
        type="textarea"
        label={t('description')}
      />
      <SubmitButton>{t('save')}</SubmitButton>
    </Form>
  );
}
