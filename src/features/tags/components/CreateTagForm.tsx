'use client';

import { useTranslations } from 'next-intl';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, toast } from '@/shared/ui';
import { createTagSchema, type CreateTagFormValues } from '../tags.schema';
import { useCreateTagMutation } from '../api/tags.queries';

export function CreateTagForm({ onDone }: { onDone: () => void }) {
  const t = useTranslations('tags');
  const tCommon = useTranslations('common.actions');
  const createMutation = useCreateTagMutation();

  const form = useAppForm<CreateTagFormValues>({
    schema: createTagSchema,
    defaultValues: { code: '', name: '' },
  });

  async function handleSubmit(values: CreateTagFormValues) {
    try {
      await createMutation.mutateAsync(values);
      toast.success(t('toast.createSuccess'));
      onDone();
    } catch {
      toast.error(t('toast.createError'));
    }
  }

  return (
    <Form form={form} onSubmit={handleSubmit} className="space-y-4">
      <FormField<CreateTagFormValues> name="code" label={t('formDialog.code')} />
      <FormField<CreateTagFormValues> name="name" label={t('formDialog.name')} />
      <div className="flex justify-end gap-2 pt-2">
        <CancelButton type="button" onClick={onDone}>
          {tCommon('cancel')}
        </CancelButton>
        <SubmitButton>{tCommon('create')}</SubmitButton>
      </div>
    </Form>
  );
}
