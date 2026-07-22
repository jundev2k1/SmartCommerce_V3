import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';

export interface AppAvatarProps {
  src?: string;
  alt?: string;
  fallback: string;
  className?: string;
}

export function AppAvatar({ src, alt, fallback, className }: AppAvatarProps) {
  return (
    <Avatar className={className}>
      {src ? <AvatarImage src={src} alt={alt ?? fallback} /> : null}
      <AvatarFallback>{fallback}</AvatarFallback>
    </Avatar>
  );
}
