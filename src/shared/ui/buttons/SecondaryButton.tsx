import { AppButton, type AppButtonProps } from '../AppButton';

export function SecondaryButton(props: Omit<AppButtonProps, 'variant'>) {
  return <AppButton variant="secondary" {...props} />;
}
