'use client';

import { useForm, type FieldValues, type Resolver, type UseFormProps } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import type { z } from 'zod';

export interface UseAppFormOptions<TFieldValues extends FieldValues> extends Omit<
  UseFormProps<TFieldValues>,
  'resolver'
> {
  schema: z.ZodType<TFieldValues, TFieldValues>;
}

/** Wraps react-hook-form's useForm + zodResolver — see docs/frontend/forms.md. */
export function useAppForm<TFieldValues extends FieldValues>({
  schema,
  ...options
}: UseAppFormOptions<TFieldValues>) {
  return useForm<TFieldValues>({
    resolver: zodResolver(schema) as Resolver<TFieldValues>,
    mode: 'onBlur',
    ...options,
  });
}
