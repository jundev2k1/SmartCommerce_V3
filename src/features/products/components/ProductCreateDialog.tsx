'use client';

import { useState } from 'react';
import { Controller, useFieldArray } from 'react-hook-form';
import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { Plus, X } from 'lucide-react';
import { Label } from '@/components/ui/label';
import { AppModal } from '@/shared/ui';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, CreateButton, IconButton, toast, AppInput } from '@/shared/ui';
import { ImageUrlListField } from '@/shared/entity';
import { createProductSchema, type CreateProductFormValues } from '../products.schema';
import { useCreateProductMutation } from '../api/products.queries';

export interface ProductCreateDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ProductCreateDialog({ open, onOpenChange }: ProductCreateDialogProps) {
  const t = useTranslations('products');
  const tCommon = useTranslations('common.actions');
  const tVariant = useTranslations('products.variants.formDialog');
  const router = useRouter();
  const createMutation = useCreateProductMutation();
  const [defaultVariationIndex, setDefaultVariationIndex] = useState(0);

  const form = useAppForm<CreateProductFormValues>({
    schema: createProductSchema,
    defaultValues: {
      code: '',
      name: '',
      description: '',
      slug: '',
      variations: [{ sku: '', price: '', barcode: '', images: [] }],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: 'variations',
  });

  async function handleSubmit(values: CreateProductFormValues) {
    try {
      const result = await createMutation.mutateAsync({
        values,
        defaultIndex: defaultVariationIndex,
      });
      toast.success(t('toast.createSuccess'));
      onOpenChange(false);
      router.push(`/products/${result.productId}`);
    } catch {
      toast.error(t('toast.createError'));
    }
  }

  return (
    <AppModal
      open={open}
      onOpenChange={onOpenChange}
      title={t('createDialog.title')}
      description={t('createDialog.description')}
    >
      <Form form={form} onSubmit={handleSubmit} className="max-h-96 space-y-4 overflow-y-auto pr-2">
        <FormField<CreateProductFormValues> name="code" label={t('createDialog.fields.code')} />
        <FormField<CreateProductFormValues> name="name" label={t('createDialog.fields.name')} />
        <FormField<CreateProductFormValues> name="slug" label={t('createDialog.fields.slug')} />
        <FormField<CreateProductFormValues>
          name="description"
          type="textarea"
          label={t('createDialog.fields.description')}
        />

        <div className="space-y-3 border-t pt-4">
          <div className="flex items-center justify-between">
            <h3 className="font-medium">{tVariant('createTitle')} (required)</h3>
            <CreateButton
              size="sm"
              type="button"
              onClick={() => append({ sku: '', price: '', barcode: '', images: [] })}
            >
              <Plus className="size-4" />
            </CreateButton>
          </div>

          {fields.map((field, index) => (
            <div key={field.id} className="space-y-3 rounded-md border p-3">
              <div className="flex items-center justify-between">
                <label className="text-sm font-medium">
                  <input
                    type="radio"
                    checked={defaultVariationIndex === index}
                    onChange={() => setDefaultVariationIndex(index)}
                    className="mr-2"
                  />
                  {tVariant('createTitle')} {index + 1}
                </label>
                {fields.length > 1 && (
                  <IconButton
                    type="button"
                    aria-label={tCommon('delete')}
                    onClick={() => remove(index)}
                  >
                    <X className="size-4" />
                  </IconButton>
                )}
              </div>

              <div className="grid grid-cols-2 gap-3">
                <Controller
                  control={form.control}
                  name={`variations.${index}.sku` as const}
                  render={({ field, fieldState }) => (
                    <div className="space-y-1.5">
                      <Label>{tVariant('sku')}</Label>
                      <AppInput {...field} aria-invalid={!!fieldState.error} />
                      {fieldState.error && (
                        <p role="alert" className="text-destructive text-sm">
                          {fieldState.error.message}
                        </p>
                      )}
                    </div>
                  )}
                />
                <Controller
                  control={form.control}
                  name={`variations.${index}.price` as const}
                  render={({ field, fieldState }) => (
                    <div className="space-y-1.5">
                      <Label>{tVariant('price')}</Label>
                      <AppInput type="number" {...field} aria-invalid={!!fieldState.error} />
                      {fieldState.error && (
                        <p role="alert" className="text-destructive text-sm">
                          {fieldState.error.message}
                        </p>
                      )}
                    </div>
                  )}
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <Controller
                  control={form.control}
                  name={`variations.${index}.barcode` as const}
                  render={({ field }) => (
                    <div className="space-y-1.5">
                      <Label>{tVariant('barcode')}</Label>
                      <AppInput {...field} />
                    </div>
                  )}
                />
                <Controller
                  control={form.control}
                  name={`variations.${index}.cost` as const}
                  render={({ field }) => (
                    <div className="space-y-1.5">
                      <Label>{tVariant('cost')}</Label>
                      <AppInput type="number" {...field} />
                    </div>
                  )}
                />
              </div>

              <div className="space-y-1.5">
                <Label>{tVariant('images')}</Label>
                <ImageUrlListField
                  value={form.watch(`variations.${index}.images`) ?? []}
                  onChange={(urls) => form.setValue(`variations.${index}.images`, urls)}
                  addLabel={tVariant('images')}
                />
              </div>
            </div>
          ))}
        </div>

        <div className="flex justify-end gap-2 border-t pt-2">
          <CancelButton type="button" onClick={() => onOpenChange(false)}>
            {tCommon('cancel')}
          </CancelButton>
          <SubmitButton>{tCommon('create')}</SubmitButton>
        </div>
      </Form>
    </AppModal>
  );
}
