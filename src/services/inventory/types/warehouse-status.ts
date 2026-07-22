/**
 * TODO: backend Swagger declares `WarehouseStatus` as enum [1, 2] with no
 * x-enumNames/prose mapping published. Do not guess (Active/Inactive is a
 * plausible reading but unconfirmed) — treat as opaque until documented.
 * See docs/backend/inventory/README.md "Known limitations."
 */
export type WarehouseStatus = number;
