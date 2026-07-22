import type { ComponentProps } from 'react';
import { Checkbox } from '@/components/ui/checkbox';

export type AppCheckboxProps = ComponentProps<typeof Checkbox>;

export function AppCheckbox(props: AppCheckboxProps) {
  return <Checkbox {...props} />;
}
