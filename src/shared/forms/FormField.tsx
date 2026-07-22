'use client';

import { Controller, useFormContext, type FieldValues, type Path } from 'react-hook-form';
import { Label } from '@/components/ui/label';
import { AppInput } from '../ui/AppInput';
import { AppTextarea } from '../ui/AppTextarea';
import { AppSelect, type AppSelectOption } from '../ui/AppSelect';
import { AppCheckbox } from '../ui/AppCheckbox';
import { AppSwitch } from '../ui/AppSwitch';

export type FormFieldType =
  'text' | 'email' | 'password' | 'number' | 'textarea' | 'select' | 'checkbox' | 'switch';

export interface FormFieldProps<TFieldValues extends FieldValues> {
  name: Path<TFieldValues>;
  label?: string;
  type?: FormFieldType;
  placeholder?: string;
  options?: AppSelectOption[];
  description?: string;
}

const inlineTypes: FormFieldType[] = ['checkbox', 'switch'];

/**
 * Wraps Controller + the matching AppInput/AppSelect/... + label/error rendering
 * in one place — see docs/frontend/forms.md. Business code never imports
 * Controller directly.
 */
export function FormField<TFieldValues extends FieldValues>({
  name,
  label,
  type = 'text',
  placeholder,
  options,
  description,
}: FormFieldProps<TFieldValues>) {
  const { control } = useFormContext<TFieldValues>();
  const isInline = inlineTypes.includes(type);
  const errorId = `${name}-error`;
  const descriptionId = `${name}-description`;

  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => {
        const describedBy =
          [fieldState.error ? errorId : null, description ? descriptionId : null]
            .filter(Boolean)
            .join(' ') || undefined;

        return (
          <div className="space-y-1.5">
            {label && !isInline ? <Label htmlFor={name}>{label}</Label> : null}

            {isInline ? (
              <div className="flex items-center gap-2">
                {type === 'checkbox' ? (
                  <AppCheckbox
                    id={name}
                    checked={Boolean(field.value)}
                    onCheckedChange={field.onChange}
                    aria-invalid={!!fieldState.error}
                    aria-describedby={describedBy}
                  />
                ) : (
                  <AppSwitch
                    id={name}
                    checked={Boolean(field.value)}
                    onCheckedChange={field.onChange}
                    aria-invalid={!!fieldState.error}
                    aria-describedby={describedBy}
                  />
                )}
                {label ? <Label htmlFor={name}>{label}</Label> : null}
              </div>
            ) : type === 'textarea' ? (
              <AppTextarea
                id={name}
                placeholder={placeholder}
                aria-invalid={!!fieldState.error}
                aria-describedby={describedBy}
                {...field}
              />
            ) : type === 'select' ? (
              <AppSelect
                options={options ?? []}
                value={field.value}
                onValueChange={field.onChange}
                placeholder={placeholder}
                name={name}
              />
            ) : (
              <AppInput
                id={name}
                type={type}
                placeholder={placeholder}
                aria-invalid={!!fieldState.error}
                aria-describedby={describedBy}
                {...field}
              />
            )}

            {description ? (
              <p id={descriptionId} className="text-muted-foreground text-sm">
                {description}
              </p>
            ) : null}
            {fieldState.error ? (
              <p id={errorId} role="alert" className="text-destructive text-sm">
                {fieldState.error.message}
              </p>
            ) : null}
          </div>
        );
      }}
    />
  );
}
