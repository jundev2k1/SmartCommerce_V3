/**
 * TODO: backend Swagger declares `ProductCategoryStatus` as enum [1, 2] with no
 * x-enumNames/prose mapping published. Do not guess — treat as opaque until
 * documented. See docs/backend/product/README.md "Known limitations."
 */
export type ProductCategoryStatus = number;
