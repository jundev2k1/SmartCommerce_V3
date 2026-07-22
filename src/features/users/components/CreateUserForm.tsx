'use client';

import { useTranslations } from 'next-intl';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton, CancelButton, toast } from '@/shared/ui';
import { createUserSchema, type CreateUserFormValues } from '../users.schema';
import { useCreateUserMutation } from '../api/users.queries';

export interface CreateUserFormProps {
  onDone: () => void;
}

export function CreateUserForm({ onDone }: CreateUserFormProps) {
  const t = useTranslations('users');
  const tCommon = useTranslations('common.actions');
  const createMutation = useCreateUserMutation();

  const form = useAppForm<CreateUserFormValues>({
    schema: createUserSchema,
    defaultValues: {
      email: '',
      userName: '',
      phoneNumber: '',
      firstName: '',
      lastName: '',
      tempPassword: '',
    },
  });

  async function handleSubmit(values: CreateUserFormValues) {
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
      <div className="grid grid-cols-2 gap-4">
        <FormField<CreateUserFormValues> name="firstName" label={t('fields.firstName')} />
        <FormField<CreateUserFormValues> name="lastName" label={t('fields.lastName')} />
      </div>
      <FormField<CreateUserFormValues> name="email" type="email" label={t('fields.email')} />
      <FormField<CreateUserFormValues> name="userName" label={t('fields.userName')} />
      <FormField<CreateUserFormValues> name="phoneNumber" label={t('fields.phoneNumber')} />
      <FormField<CreateUserFormValues>
        name="tempPassword"
        type="password"
        label={t('fields.tempPassword')}
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
