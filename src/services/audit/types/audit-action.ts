/**
 * TODO: backend Swagger declares `AuditAction` as enum [0, 1, 2] with no
 * `x-enumNames`/prose mapping published anywhere in audit-swagger.json.
 * Do not guess the mapping — treat as opaque until the backend documents it.
 * See docs/backend/audit/README.md "Known limitations."
 */
export type AuditAction = number;
