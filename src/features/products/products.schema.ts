import { z } from 'zod';

const numericString = (message: string) =>
  z
    .string()
    .min(1, message)
    .refine((v) => !Number.isNaN(Number(v)), message);

const optionalNumericString = z
  .string()
  .optional()
  .refine((v) => !v || !Number.isNaN(Number(v)), 'Must be a number');

/**
 * Number fields are kept as validated numeric strings, not `z.coerce.number()`
 * — useAppForm requires the schema's input/output types to match exactly
 * (see shared/forms/useAppForm.ts), which `z.coerce` violates (its input type
 * is `unknown`). Parsing to a real number happens where the request payload
 * is built, in api/products.queries.ts.
 */

/** Shared by add-variation and update-variation forms. */
export const variationFormSchema = z.object({
  sku: z.string().min(1),
  price: numericString('Price must be a number'),
  barcode: z.string().optional(),
  cost: optionalNumericString,
  weight: optionalNumericString,
  status: z.string().optional(),
  images: z.array(z.string()).optional(),
});

export type VariationFormValues = z.infer<typeof variationFormSchema>;

/** CreateProduct is an aggregate create — at least one variation up front. */
export const createProductSchema = z.object({
  code: z.string().min(1),
  name: z.string().min(1),
  description: z.string().optional(),
  slug: z.string().min(1),
  variations: z.array(variationFormSchema).min(1),
});

export type CreateProductFormValues = z.infer<typeof createProductSchema>;

/** Mirrors UpdateProductRequest. */
export const updateProductSchema = z.object({
  name: z.string().min(1),
  description: z.string().optional(),
  slug: z.string().min(1),
});

export type UpdateProductFormValues = z.infer<typeof updateProductSchema>;
