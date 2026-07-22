import { z } from 'zod';

/** Mirrors CreateProductTagRequest. No `color` field exists on this contract — see docs/backend/product/README.md. */
export const createTagSchema = z.object({
  code: z.string().min(1),
  name: z.string().min(1),
});

export type CreateTagFormValues = z.infer<typeof createTagSchema>;

/** Mirrors UpdateProductTagRequest (rename only). */
export const updateTagSchema = z.object({
  name: z.string().min(1),
});

export type UpdateTagFormValues = z.infer<typeof updateTagSchema>;
