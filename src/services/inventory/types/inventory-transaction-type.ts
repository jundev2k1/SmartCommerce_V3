/**
 * TODO: backend Swagger declares `InventoryTransactionType` as enum [1, 2, 3]
 * with no x-enumNames/prose mapping. The service groups StockIn/StockOut/Adjust
 * endpoints in that order, which loosely suggests 1=StockIn, 2=StockOut,
 * 3=Adjustment — but this is inferred from section ordering, not confirmed by
 * the backend. Treat as opaque until documented.
 * See docs/backend/inventory/README.md "Known limitations."
 */
export type InventoryTransactionType = number;
