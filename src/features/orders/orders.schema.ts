import { z } from 'zod';

/** Mirrors UpdateOrderOwnerInfoRequest (src/services/order/update-order-owner-info.ts). */
export const updateOrderOwnerInfoSchema = z.object({
  customerPhone: z.string().min(1),
  // 500-char cap matches the backend's shippingAddress column (see docs/backend/order/README.md).
  shippingAddress: z.string().min(1).max(500),
});

export type UpdateOrderOwnerInfoFormValues = z.infer<typeof updateOrderOwnerInfoSchema>;
