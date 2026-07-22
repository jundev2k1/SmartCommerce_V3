import Link from 'next/link';
import { AppEmpty, PrimaryButton } from '@/shared/ui';

export default function ForbiddenPage() {
  return (
    <div className="flex min-h-svh items-center justify-center p-6">
      <AppEmpty
        title="Forbidden"
        description="You don't have permission to access this page."
        action={
          <PrimaryButton asChild>
            <Link href="/">Back to dashboard</Link>
          </PrimaryButton>
        }
      />
    </div>
  );
}
