import { z } from 'zod';

/**
 * Mirrors CreateUserRequest — `roles` is intentionally left off the UI (no
 * "list valid roles" endpoint exists to build a picker against), so it's not
 * part of this form schema at all. See docs/modules/user-management.md.
 */
export const createUserSchema = z.object({
  email: z.string().min(1).email(),
  userName: z.string().min(1),
  phoneNumber: z.string().min(1),
  firstName: z.string().min(1),
  lastName: z.string().min(1),
  tempPassword: z.string().min(8).optional().or(z.literal('')),
});

export type CreateUserFormValues = z.infer<typeof createUserSchema>;

/** Mirrors UpdateUserRequest exactly. */
export const updateUserSchema = z.object({
  firstName: z.string().min(1),
  lastName: z.string().min(1),
  phoneNumber: z.string().min(1),
});

export type UpdateUserFormValues = z.infer<typeof updateUserSchema>;
