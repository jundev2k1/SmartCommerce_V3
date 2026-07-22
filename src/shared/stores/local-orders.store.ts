import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface LocalOrdersState {
  orderIds: string[];
  addOrderId: (orderId: string) => void;
}

/**
 * The Order service has no list/search endpoint (see
 * docs/backend/order/README.md — only GetOrder-by-id, CreateOrder, CancelOrder
 * exist). This store tracks the ids of orders created from this browser so
 * "My Orders" has something to list. Every field shown per order still comes
 * from a real GetOrder call — this only supplies which ids to fetch. Orders
 * placed from another browser/device won't appear here. See
 * docs/modules/client-mock.md.
 */
export const useLocalOrdersStore = create<LocalOrdersState>()(
  persist(
    (set) => ({
      orderIds: [],
      addOrderId: (orderId) =>
        set((state) => ({
          orderIds: state.orderIds.includes(orderId)
            ? state.orderIds
            : [orderId, ...state.orderIds],
        })),
    }),
    { name: 'simpleshopui-local-orders' },
  ),
);
