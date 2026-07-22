'use client';

import { useTranslations } from 'next-intl';
import { AppModal } from '@/shared/ui';
import { CreateTagForm } from './CreateTagForm';
import { EditTagForm } from './EditTagForm';
import type { UpdateTagFormValues } from '../tags.schema';

export type TagFormDialogState =
  { mode: 'create' } | { mode: 'edit'; tagId: string; defaultValues: UpdateTagFormValues };

export interface TagFormDialogProps {
  state: TagFormDialogState | null;
  onOpenChange: (open: boolean) => void;
}

export function TagFormDialog({ state, onOpenChange }: TagFormDialogProps) {
  const t = useTranslations('tags.formDialog');
  const isCreate = state?.mode === 'create';

  return (
    <AppModal
      open={state !== null}
      onOpenChange={onOpenChange}
      title={isCreate ? t('createTitle') : t('editTitle')}
    >
      {state?.mode === 'create' ? (
        <CreateTagForm onDone={() => onOpenChange(false)} />
      ) : state?.mode === 'edit' ? (
        <EditTagForm
          tagId={state.tagId}
          defaultValues={state.defaultValues}
          onDone={() => onOpenChange(false)}
        />
      ) : null}
    </AppModal>
  );
}
