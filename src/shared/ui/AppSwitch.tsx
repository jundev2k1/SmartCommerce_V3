import type { ComponentProps } from 'react';
import { Switch } from '@/components/ui/switch';

export type AppSwitchProps = ComponentProps<typeof Switch>;

export function AppSwitch(props: AppSwitchProps) {
  return <Switch {...props} />;
}
