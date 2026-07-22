import { Trash2 } from 'lucide-react';
import { AppButton, type AppButtonProps } from '../AppButton';

/**
 * Confirmation is the caller's responsibility (wrap the trigger in AppModal) —
 * not baked into the button itself. See docs/frontend/ui-components.md.
 */
export function DeleteButton({ children, ...props }: Omit<AppButtonProps, 'variant'>) {
  return (
    <AppButton variant="destructive" {...props}>
      <Trash2 />
      {children}
    </AppButton>
  );
}
