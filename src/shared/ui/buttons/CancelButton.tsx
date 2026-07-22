import { AppButton, type AppButtonProps } from '../AppButton';

export function CancelButton(props: Omit<AppButtonProps, 'variant'>) {
  return <AppButton variant="ghost" {...props} />;
}
