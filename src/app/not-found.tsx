import Link from 'next/link';
import { AppEmpty, PrimaryButton } from '@/shared/ui';

export default function NotFound() {
  return (
    <div className="flex flex-1 items-center justify-center p-6">
      <AppEmpty
        title="Page not found"
        description="The page you're looking for doesn't exist."
        action={
          <PrimaryButton asChild>
            <Link href="/">Back to dashboard</Link>
          </PrimaryButton>
        }
      />
    </div>
  );
}
