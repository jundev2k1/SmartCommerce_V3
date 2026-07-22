'use client';

import { useTranslations } from 'next-intl';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, toast } from '@/shared/ui';
import { updateUserSchema, type UpdateUserFormValues } from '../users.schema';
import { useUpdateUserMutation } from '../api/users.queries';

export interface EditUserFormProps {
  userId: string;
  defaultValues: UpdateUserFormValues;
  onDone: () => void;
}

export function EditUserForm({ userId, defaultValues, onDone }: EditUserFormProps) {
  const t = useTranslations('users');
  const tCommon = useTranslations('common.actions');
  const updateMutation = useUpdateUserMutation();

  const form = useAppForm<UpdateUserFormValues>({
    schema: updateUserSchema,
    defaultValues,
  });

  async function handleSubmit(values: UpdateUserFormValues) {
    try {
      await updateMutation.mutateAsync({ userId, values });
      toast.success(t('toast.updateSuccess'));
      onDone();
    } catch {
      toast.error(t('toast.updateError'));
    }
  }

  return (
    <Form form={form} onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <FormField<UpdateUserFormValues> name="firstName" label={t('fields.firstName')} />
        <FormField<UpdateUserFormValues> name="lastName" label={t('fields.lastName')} />
      </div>
      <FormField<UpdateUserFormValues> name="phoneNumber" label={t('fields.phoneNumber')} />
      <div className="flex justify-end gap-2 pt-2">
        <CancelButton type="button" onClick={onDone}>
          {tCommon('cancel')}
        </CancelButton>
        <SubmitButton>{tCommon('save')}</SubmitButton>
      </div>
    </Form>
  );
}
