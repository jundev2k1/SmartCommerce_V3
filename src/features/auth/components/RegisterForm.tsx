'use client';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton } from '@/shared/ui';
import { registerSchema, type RegisterFormValues } from '../auth.schema';
import { useRegisterMutation } from '../api/auth.queries';

export function RegisterForm() {
  const t = useTranslations('auth.register');
  const router = useRouter();
  const registerMutation = useRegisterMutation();

  const form = useAppForm<RegisterFormValues>({
    schema: registerSchema,
    defaultValues: { email: '', password: '', firstName: '', lastName: '', phoneNumber: '' },
  });

  async function handleSubmit(values: RegisterFormValues) {
    try {
      await registerMutation.mutateAsync(values);
      // Register sets the same session cookies login does — go straight to the dashboard.
      router.replace('/');
    } catch {
      form.setError('root', { message: t('error') });
    }
  }

  return (
    <Form form={form} onSubmit={handleSubmit} className="space-y-4">
      {form.formState.errors.root ? (
        <p role="alert" className="text-destructive text-sm">
          {form.formState.errors.root.message}
        </p>
      ) : null}
      <div className="grid grid-cols-2 gap-4">
        <FormField<RegisterFormValues> name="firstName" label={t('firstName')} />
        <FormField<RegisterFormValues> name="lastName" label={t('lastName')} />
      </div>
      <FormField<RegisterFormValues> name="email" type="email" label={t('email')} />
      <FormField<RegisterFormValues> name="phoneNumber" label={t('phoneNumber')} />
      <FormField<RegisterFormValues> name="password" type="password" label={t('password')} />
      <SubmitButton className="w-full">{t('submit')}</SubmitButton>
    </Form>
  );
}
