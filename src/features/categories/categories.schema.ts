import { z } from 'zod';

/** Mirrors CreateProductCategoryRequest. */
export const createCategorySchema = z.object({
  code: z.string().min(1),
  name: z.string().min(1),
  description: z.string().optional(),
  parentCategoryId: z.string().optional(),
});

export type CreateCategoryFormValues = z.infer<typeof createCategorySchema>;

/** Mirrors UpdateProductCategoryRequest. */
export const updateCategorySchema = z.object({
  name: z.string().min(1),
  description: z.string().optional(),
  parentCategoryId: z.string().nullable().optional(),
});

export type UpdateCategoryFormValues = z.infer<typeof updateCategorySchema>;
