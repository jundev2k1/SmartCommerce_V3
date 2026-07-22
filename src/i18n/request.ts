import { getRequestConfig } from 'next-intl/server';
import { DEFAULT_LOCALE } from '@/shared/lib/constants';

import common from './messages/en/common.json';
import nav from './messages/en/nav.json';
import dashboard from './messages/en/dashboard.json';
import modules from './messages/en/modules.json';
import auth from './messages/en/auth.json';
import users from './messages/en/users.json';
import entity from './messages/en/entity.json';
import products from './messages/en/products.json';
import categories from './messages/en/categories.json';
import tags from './messages/en/tags.json';
import productSearch from './messages/en/product-search.json';
import shop from './messages/en/shop.json';
import cart from './messages/en/cart.json';
import checkout from './messages/en/checkout.json';
import orders from './messages/en/orders.json';
import commerce from './messages/en/commerce.json';
import inventory from './messages/en/inventory.json';
import inventoryUi from './messages/en/inventory-ui.json';
import notifications from './messages/en/notifications.json';
import notificationsUi from './messages/en/notifications-ui.json';

/**
 * Single-locale request config (no URL-prefixed routing yet — see
 * docs/frontend/i18n.md). Adding a locale later means adding a messages/<locale>
 * set and registering it here; no component changes.
 */
export default getRequestConfig(async () => {
  return {
    locale: DEFAULT_LOCALE,
    messages: {
      common,
      nav,
      dashboard,
      modules,
      auth,
      users,
      entity,
      products,
      categories,
      tags,
      productSearch,
      shop,
      cart,
      checkout,
      orders,
      commerce,
      inventory,
      inventoryUi,
      notifications,
      notificationsUi,
    },
  };
});
