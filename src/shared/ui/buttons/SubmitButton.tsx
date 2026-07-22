'use client';

import { useFormContext } from 'react-hook-form';
import { AppButton, type AppButtonProps } from '../AppButton';

/**
 * Reads formState.isSubmitting when rendered inside shared/forms's <Form> —
 * falls back to the `loading` prop when used standalone.
 */
export function SubmitButton({ loading, ...props }: Omit<AppButtonProps, 'type' | 'variant'>) {
  const formContext = useFormContext();
  const isSubmitting = formContext?.formState.isSubmitting ?? false;

  return <AppButton type="submit" variant="default" loading={loading ?? isSubmitting} {...props} />;
}
