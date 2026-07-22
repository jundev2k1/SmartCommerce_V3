import { Bell, ShoppingBag, Boxes, Settings, UserRound } from 'lucide-react';

export interface NotificationIconProps {
  category?: string | null;
  className?: string;
}

/**
 * `category`/`type` on `UserNotificationSummaryResponse` are free-text
 * strings, not an enum (see docs/backend/notification/README.md) — this maps
 * a few conventional values to a nicer icon purely as presentation, falling
 * back to a generic bell for anything unrecognized. Unlike the opaque numeric
 * enums elsewhere in this service, guessing at a free-text convention here
 * doesn't misrepresent confirmed backend data — an unmapped value just gets
 * the same generic icon it would have gotten anyway.
 */
const ICONS: Record<string, typeof Bell> = {
  order: ShoppingBag,
  inventory: Boxes,
  stock: Boxes,
  system: Settings,
  account: UserRound,
  user: UserRound,
};

export function NotificationIcon({ category, className }: NotificationIconProps) {
  const Icon = (category && ICONS[category.toLowerCase()]) || Bell;
  return <Icon className={className} />;
}
