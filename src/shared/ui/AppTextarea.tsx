import type { ComponentProps } from 'react';
import { Textarea } from '@/components/ui/textarea';

export type AppTextareaProps = ComponentProps<typeof Textarea>;

export function AppTextarea(props: AppTextareaProps) {
  return <Textarea {...props} />;
}
