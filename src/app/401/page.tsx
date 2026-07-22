import Link from 'next/link';
import { AppEmpty, PrimaryButton } from '@/shared/ui';

export default function UnauthorizedPage() {
  return (
    <div className="flex min-h-svh items-center justify-center p-6">
      <AppEmpty
        title="Unauthorized"
        description="You need to sign in to view this page."
        action={
          <PrimaryButton asChild>
            <Link href="/login">Sign in</Link>
          </PrimaryButton>
        }
      />
    </div>
  );
}
