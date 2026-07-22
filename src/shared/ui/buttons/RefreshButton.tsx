import { RefreshCw } from 'lucide-react';
import { AppButton, type AppButtonProps } from '../AppButton';

export function RefreshButton({
  loading,
  disabled,
  ...props
}: Omit<AppButtonProps, 'variant' | 'children'>) {
  return (
    <AppButton variant="ghost" size="icon" disabled={disabled || loading} {...props}>
      <RefreshCw className={loading ? 'animate-spin' : undefined} />
    </AppButton>
  );
}
