'use client';

import { useTranslations } from 'next-intl';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, toast } from '@/shared/ui';
import type { AppSelectOption } from '@/shared/ui';
import { updateCategorySchema, type UpdateCategoryFormValues } from '../categories.schema';
import { useUpdateCategoryMutation } from '../api/categories.queries';

export interface EditCategoryFormProps {
  categoryId: string;
  defaultValues: UpdateCategoryFormValues;
  /** Excludes the category itself and its own descendants — see categories.utils.ts. */
  categoryOptions: AppSelectOption[];
  onDone: () => void;
}

export function EditCategoryForm({
  categoryId,
  defaultValues,
  categoryOptions,
  onDone,
}: EditCategoryFormProps) {
  const t = useTranslations('categories');
  const tCommon = useTranslations('common.actions');
  const updateMutation = useUpdateCategoryMutation();

  const form = useAppForm<UpdateCategoryFormValues>({
    schema: updateCategorySchema,
    defaultValues,
  });

  async function handleSubmit(values: UpdateCategoryFormValues) {
    try {
      await updateMutation.mutateAsync({
        categoryId,
        values: { ...values, parentCategoryId: values.parentCategoryId || null },
      });
      toast.success(t('toast.updateSuccess'));
      onDone();
    } catch {
      // The server enforces cycle-prevention with a 409 — surfaced generically here.
      toast.error(t('toast.updateError'));
    }
  }

  return (
    <Form form={form} onSubmit={handleSubmit} className="space-y-4">
      <FormField<UpdateCategoryFormValues> name="name" label={t('formDialog.name')} />
      <FormField<UpdateCategoryFormValues>
        name="description"
        type="textarea"
        label={t('formDialog.description')}
      />
      <FormField<UpdateCategoryFormValues>
        name="parentCategoryId"
        type="select"
        label={t('formDialog.parent')}
        placeholder={t('formDialog.noParent')}
        options={categoryOptions}
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
