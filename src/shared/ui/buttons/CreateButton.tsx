import { Plus } from 'lucide-react';
import { AppButton, type AppButtonProps } from '../AppButton';

export function CreateButton({ children, ...props }: Omit<AppButtonProps, 'variant'>) {
  return (
    <AppButton variant="default" {...props}>
      <Plus />
      {children}
    </AppButton>
  );
}
