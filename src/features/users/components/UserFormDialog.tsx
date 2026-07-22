'use client';

import { useTranslations } from 'next-intl';
import { AppModal } from '@/shared/ui';
import { CreateUserForm } from './CreateUserForm';
import { EditUserForm } from './EditUserForm';
import type { UpdateUserFormValues } from '../users.schema';

export type UserFormDialogState =
  { mode: 'create' } | { mode: 'edit'; userId: string; defaultValues: UpdateUserFormValues };

export interface UserFormDialogProps {
  state: UserFormDialogState | null;
  onOpenChange: (open: boolean) => void;
}

export function UserFormDialog({ state, onOpenChange }: UserFormDialogProps) {
  const t = useTranslations('users');
  const isCreate = state?.mode === 'create';

  return (
    <AppModal
      open={state !== null}
      onOpenChange={onOpenChange}
      title={isCreate ? t('createDialog.title') : t('editDialog.title')}
      description={isCreate ? t('createDialog.description') : t('editDialog.description')}
    >
      {state?.mode === 'create' ? (
        <CreateUserForm onDone={() => onOpenChange(false)} />
      ) : state?.mode === 'edit' ? (
        <EditUserForm
          userId={state.userId}
          defaultValues={state.defaultValues}
          onDone={() => onOpenChange(false)}
        />
      ) : null}
    </AppModal>
  );
}
