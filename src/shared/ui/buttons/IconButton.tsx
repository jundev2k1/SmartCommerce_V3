import { AppButton, type AppButtonProps } from '../AppButton';

export function IconButton({
  'aria-label': ariaLabel,
  ...props
}: Omit<AppButtonProps, 'size'> & {
  'aria-label': string;
}) {
  return <AppButton variant="ghost" size="icon" aria-label={ariaLabel} {...props} />;
}
