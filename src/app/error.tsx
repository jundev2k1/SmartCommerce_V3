'use client';

import { useEffect } from 'react';
import { AppEmpty, PrimaryButton } from '@/shared/ui';

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="flex flex-1 items-center justify-center p-6">
      <AppEmpty
        title="Something went wrong"
        description="An unexpected error occurred. You can try again."
        action={<PrimaryButton onClick={reset}>Try again</PrimaryButton>}
      />
    </div>
  );
}
