'use client';

import { useTranslations } from 'next-intl';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, toast } from '@/shared/ui';
import { updateTagSchema, type UpdateTagFormValues } from '../tags.schema';
import { useUpdateTagMutation } from '../api/tags.queries';

export interface EditTagFormProps {
  tagId: string;
  defaultValues: UpdateTagFormValues;
  onDone: () => void;
}

export function EditTagForm({ tagId, defaultValues, onDone }: EditTagFormProps) {
  const t = useTranslations('tags');
  const tCommon = useTranslations('common.actions');
  const updateMutation = useUpdateTagMutation();

  const form = useAppForm<UpdateTagFormValues>({ schema: updateTagSchema, defaultValues });

  async function handleSubmit(values: UpdateTagFormValues) {
    try {
      await updateMutation.mutateAsync({ tagId, values });
      toast.success(t('toast.updateSuccess'));
      onDone();
    } catch {
      toast.error(t('toast.updateError'));
    }
  }

  return (
    <Form form={form} onSubmit={handleSubmit} className="space-y-4">
      <FormField<UpdateTagFormValues> name="name" label={t('formDialog.name')} />
      <div className="flex justify-end gap-2 pt-2">
        <CancelButton type="button" onClick={onDone}>
          {tCommon('cancel')}
        </CancelButton>
        <SubmitButton>{tCommon('save')}</SubmitButton>
      </div>
    </Form>
  );
}
