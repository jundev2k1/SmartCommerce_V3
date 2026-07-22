'use client';

import { useTranslations } from 'next-intl';
import { AppModal, DeleteButton, CancelButton } from '@/shared/ui';

export interface ConfirmDeleteDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
  isPending?: boolean;
  title?: string;
  description?: string;
}

/** Generic delete confirmation — reused by every entity's delete action, never re-implemented per-feature. */
export function ConfirmDeleteDialog({
  open,
  onOpenChange,
  onConfirm,
  isPending,
  title,
  description,
}: ConfirmDeleteDialogProps) {
  const t = useTranslations('entity.confirmDelete');

  return (
    <AppModal
      open={open}
      onOpenChange={onOpenChange}
      title={title ?? t('title', { name: 'this item' })}
      description={description ?? t('description')}
      footer={
        <>
          <CancelButton onClick={() => onOpenChange(false)}>{t('cancel')}</CancelButton>
          <DeleteButton onClick={onConfirm} loading={isPending}>
            {t('confirm')}
          </DeleteButton>
        </>
      }
    />
  );
}
