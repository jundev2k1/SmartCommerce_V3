'use client';

import { useState } from 'react';
import { ShieldAlert } from 'lucide-react';
import { useTranslations } from 'next-intl';
import { SecondaryButton } from '@/shared/ui';
import { AuditTrailDialog } from './AuditTrailDialog';

export interface AuditTrailButtonProps {
  service: string;
  entityId: string;
}

/** Self-contained button + dialog — drop it in anywhere an entity needs an audit trail. */
export function AuditTrailButton({ service, entityId }: AuditTrailButtonProps) {
  const [open, setOpen] = useState(false);
  const t = useTranslations('entity.auditTrail');

  return (
    <>
      <SecondaryButton onClick={() => setOpen(true)}>
        <ShieldAlert /> {t('button')}
      </SecondaryButton>
      <AuditTrailDialog service={service} entityId={entityId} open={open} onOpenChange={setOpen} />
    </>
  );
}
