'use client';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { useAppForm, Form, FormField } from '@/shared/forms';
import { SubmitButton } from '@/shared/ui';
import { loginSchema, type LoginFormValues } from '../auth.schema';
import { useLoginMutation } from '../api/auth.queries';

export function LoginForm() {
  const t = useTranslations('auth.login');
  const router = useRouter();
  const loginMutation = useLoginMutation();

  const form = useAppForm<LoginFormValues>({
    schema: loginSchema,
    defaultValues: { email: '', password: '' },
  });

  async function handleSubmit(values: LoginFormValues) {
    try {
      await loginMutation.mutateAsync(values);
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
      <FormField<LoginFormValues> name="email" type="email" label={t('email')} />
      <FormField<LoginFormValues> name="password" type="password" label={t('password')} />
      <SubmitButton className="w-full">{t('submit')}</SubmitButton>
    </Form>
  );
}
