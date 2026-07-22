'use client';

import { useRouter } from 'next/navigation';
import { ArrowLeft } from 'lucide-react';
import { AppButton, type AppButtonProps } from '../AppButton';

export function BackButton({ children, onClick, ...props }: Omit<AppButtonProps, 'variant'>) {
  const router = useRouter();

  return (
    <AppButton
      variant="ghost"
      onClick={(e) => {
        onClick?.(e);
        router.back();
      }}
      {...props}
    >
      <ArrowLeft />
      {children}
    </AppButton>
  );
}
