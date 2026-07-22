/** Confirmed by backend source — Order.Domain/Enums/OrderStatus.cs. */
export const OrderStatus = {
  Pending: 1,
  Confirmed: 2,
  Cancelled: 3,
  Completed: 4,
} as const;

export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus];
