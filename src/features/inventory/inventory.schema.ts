import { z } from 'zod';

/**
 * Numeric fields are validated strings, not `z.coerce.number()` — same
 * constraint as features/products (see products.schema.ts): useAppForm
 * requires the schema's input/output types to match exactly, which
 * `z.coerce` violates.
 */
const numericString = (message: string) =>
  z
    .string()
    .min(1, message)
    .refine((v) => !Number.isNaN(Number(v)), message);

export const createWarehouseSchema = z.object({
  code: z.string().min(1),
  name: z.string().min(1),
  address: z.string().min(1),
});

export type CreateWarehouseFormValues = z.infer<typeof createWarehouseSchema>;

export const stockInSchema = z.object({
  quantity: numericString('Quantity must be a number'),
  reason: z.string().min(1),
});

export type StockInFormValues = z.infer<typeof stockInSchema>;

export const stockOutSchema = z.object({
  quantity: numericString('Quantity must be a number'),
  reason: z.string().min(1),
});

export type StockOutFormValues = z.infer<typeof stockOutSchema>;

export const adjustStockSchema = z.object({
  newQuantity: numericString('Quantity must be a number'),
  reason: z.string().min(1),
});

export type AdjustStockFormValues = z.infer<typeof adjustStockSchema>;
