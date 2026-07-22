'use client';

import { useTranslations } from 'next-intl';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, toast } from '@/shared/ui';
import type { AppSelectOption } from '@/shared/ui';
import { createCategorySchema, type CreateCategoryFormValues } from '../categories.schema';
import { useCreateCategoryMutation } from '../api/categories.queries';

export interface CreateCategoryFormProps {
  categoryOptions: AppSelectOption[];
  defaultParentId?: string;
  onDone: () => void;
}

export function CreateCategoryForm({
  categoryOptions,
  defaultParentId,
  onDone,
}: CreateCategoryFormProps) {
  const t = useTranslations('categories');
  const tCommon = useTranslations('common.actions');
  const createMutation = useCreateCategoryMutation();

  const form = useAppForm<CreateCategoryFormValues>({
    schema: createCategorySchema,
    defaultValues: { code: '', name: '', description: '', parentCategoryId: defaultParentId ?? '' },
  });

  async function handleSubmit(values: CreateCategoryFormValues) {
    try {
      await createMutation.mutateAsync({
        ...values,
        parentCategoryId: values.parentCategoryId || undefined,
      });
      toast.success(t('toast.createSuccess'));
      onDone();
    } catch {
      toast.error(t('toast.createError'));
    }
  }

  return (
    <Form form={form} onSubmit={handleSubmit} className="space-y-4">
      <FormField<CreateCategoryFormValues> name="code" label={t('formDialog.code')} />
      <FormField<CreateCategoryFormValues> name="name" label={t('formDialog.name')} />
      <FormField<CreateCategoryFormValues>
        name="description"
        type="textarea"
        label={t('formDialog.description')}
      />
      <FormField<CreateCategoryFormValues>
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
        <SubmitButton>{tCommon('create')}</SubmitButton>
      </div>
    </Form>
  );
}
