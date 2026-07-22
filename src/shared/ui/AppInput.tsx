import type { ComponentProps } from 'react';
import { Input } from '@/components/ui/input';

export type AppInputProps = ComponentProps<typeof Input>;

export function AppInput(props: AppInputProps) {
  return <Input {...props} />;
}
