import { z } from 'zod';

/** Owner-info step — mirrors the customer-facing fields of CreateOrderRequest (src/services/order/create-order.ts). */
export const checkoutOwnerSchema = z.object({
  customerName: z.string().min(1),
  customerPhone: z.string().min(1),
  // 500-char cap matches the backend's shippingAddress column (see docs/backend/order/README.md).
  shippingAddress: z.string().min(1).max(500),
});

export type CheckoutOwnerFormValues = z.infer<typeof checkoutOwnerSchema>;
