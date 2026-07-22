import {
  Users,
  Package,
  FolderTree,
  Tags,
  Search,
  Store,
  ShoppingCart,
  CreditCard,
  ClipboardList,
  ClipboardCheck,
  History,
  Boxes,
  Warehouse,
  ArrowLeftRight,
  Bell,
  Megaphone,
  ListChecks,
  UsersRound,
  Radio,
  FileClock,
  type LucideIcon,
} from 'lucide-react';
import { AppRole } from '@/services/user';

export interface NavItem {
  key: string;
  labelKey: string;
  href?: string;
  icon: LucideIcon;
  implemented: boolean;
  /** Omit for "visible to any authenticated role." Ignored if the parent group already sets its own `roles`. */
  roles?: string[];
}

export interface NavGroup {
  key: string;
  labelKey: string;
  items: NavItem[];
  /** Omit for "visible to any authenticated role." Restricts every item in the group — items don't need their own `roles` on top of this. */
  roles?: string[];
}

/**
 * Sidebar navigation grouped by bounded context, per docs/modules/overview.md.
 * `implemented: false` entries render disabled ("coming soon") and have no
 * href — see docs/decisions ADR list + the Notification reservation rule.
 */
export const navGroups: NavGroup[] = [
  {
    key: 'iam',
    labelKey: 'groups.iam',
    roles: [AppRole.Admin],
    items: [
      { key: 'users', labelKey: 'items.users', href: '/users', icon: Users, implemented: true },
    ],
  },
  {
    key: 'catalog',
    labelKey: 'groups.catalog',
    roles: [AppRole.Admin],
    items: [
      {
        key: 'products',
        labelKey: 'items.products',
        href: '/products',
        icon: Package,
        implemented: true,
      },
      {
        key: 'categories',
        labelKey: 'items.categories',
        href: '/categories',
        icon: FolderTree,
        implemented: true,
      },
      { key: 'tags', labelKey: 'items.tags', href: '/tags', icon: Tags, implemented: true },
      {
        key: 'product-search',
        labelKey: 'items.productSearch',
        href: '/product-search',
        icon: Search,
        implemented: true,
      },
    ],
  },
  {
    key: 'sales',
    labelKey: 'groups.sales',
    items: [
      { key: 'shop', labelKey: 'items.shop', href: '/shop', icon: Store, implemented: true },
      { key: 'cart', labelKey: 'items.cart', href: '/cart', icon: ShoppingCart, implemented: true },
      {
        key: 'checkout',
        labelKey: 'items.checkout',
        href: '/checkout',
        icon: CreditCard,
        implemented: true,
      },
      {
        key: 'order-history',
        labelKey: 'items.orderHistory',
        href: '/orders/history',
        icon: History,
        implemented: true,
      },
      {
        key: 'orders',
        labelKey: 'items.orders',
        href: '/orders',
        icon: ClipboardList,
        implemented: true,
        roles: [AppRole.Admin],
      },
      {
        key: 'orders-approve',
        labelKey: 'items.ordersApprove',
        href: '/orders/approve',
        icon: ClipboardCheck,
        implemented: true,
        roles: [AppRole.Admin],
      },
    ],
  },
  {
    key: 'inventory',
    labelKey: 'groups.inventory',
    roles: [AppRole.Admin],
    items: [
      {
        key: 'inventory',
        labelKey: 'items.inventory',
        href: '/inventory',
        icon: Boxes,
        implemented: true,
      },
      {
        key: 'warehouses',
        labelKey: 'items.warehouses',
        href: '/warehouses',
        icon: Warehouse,
        implemented: true,
      },
      {
        key: 'stock-transactions',
        labelKey: 'items.stockTransactions',
        href: '/stock-transactions',
        icon: ArrowLeftRight,
        implemented: true,
      },
    ],
  },
  {
    key: 'notification',
    labelKey: 'groups.notification',
    items: [
      {
        key: 'notifications',
        labelKey: 'items.notifications',
        href: '/notifications',
        icon: Bell,
        implemented: true,
      },
      {
        key: 'notification-campaigns',
        labelKey: 'items.notificationCampaigns',
        icon: Megaphone,
        implemented: false,
        roles: [AppRole.Admin],
      },
      {
        key: 'notification-rules',
        labelKey: 'items.notificationRules',
        icon: ListChecks,
        implemented: false,
        roles: [AppRole.Admin],
      },
      {
        key: 'notification-groups',
        labelKey: 'items.notificationGroups',
        icon: UsersRound,
        implemented: false,
        roles: [AppRole.Admin],
      },
      {
        key: 'notification-channels',
        labelKey: 'items.notificationChannels',
        icon: Radio,
        implemented: false,
        roles: [AppRole.Admin],
      },
    ],
  },
  {
    key: 'system',
    labelKey: 'groups.system',
    roles: [AppRole.Admin],
    items: [
      { key: 'audit', labelKey: 'items.audit', href: '/audit', icon: FileClock, implemented: true },
    ],
  },
];

/**
 * `Root` satisfies every role check on the backend (see
 * `ClaimsPrincipalExtensions.HasRole`/`AppRole.Root`'s doc comment) — mirrored
 * here so a Root-seeded account isn't wrongly hidden from admin nav/routes.
 */
export function hasRequiredRole(userRoles: string[], requiredRoles: string[] | undefined) {
  if (!requiredRoles || requiredRoles.length === 0) return true;
  if (userRoles.includes(AppRole.Root)) return true;
  return requiredRoles.some((role) => userRoles.includes(role));
}

/**
 * `/orders` is excluded from prefix matching on purpose: `/orders/{id}` is a
 * shared order-detail page any authenticated user can reach (to view their
 * own order), even though the `/orders` admin list itself is Admin-only —
 * its more specific siblings (`/orders/history`, `/orders/approve`) already
 * have their own exact-match entries above.
 */
const PREFIX_MATCH_EXCLUDED_HREFS = new Set(['/orders']);

/**
 * Resolves which roles (if any) a given pathname requires, by finding the
 * matching nav item — exact href match first, then prefix match for nested
 * routes not themselves listed in nav.ts (e.g. `/products/[productId]`
 * inherits `/products`'s restriction). Returns `undefined` for "no
 * restriction" (any authenticated role) or an unmatched path.
 */
export function getRequiredRolesForPath(pathname: string): string[] | undefined {
  for (const group of navGroups) {
    for (const item of group.items) {
      if (item.href && pathname === item.href) {
        return item.roles ?? group.roles;
      }
    }
  }
  for (const group of navGroups) {
    for (const item of group.items) {
      if (!item.href || PREFIX_MATCH_EXCLUDED_HREFS.has(item.href)) continue;
      if (pathname.startsWith(`${item.href}/`)) {
        return item.roles ?? group.roles;
      }
    }
  }
  return undefined;
}
