import { z } from 'zod';

/** Mirrors src/services/auth/requests/login.request.ts's LoginRequest exactly. */
export const loginSchema = z.object({
  email: z.string().min(1).email(),
  password: z.string().min(1),
});

export type LoginFormValues = z.infer<typeof loginSchema>;

/** Mirrors src/services/auth/requests/register.request.ts's RegisterRequest exactly. */
export const registerSchema = z.object({
  email: z.string().min(1).email(),
  password: z.string().min(8),
  firstName: z.string().min(1),
  lastName: z.string().min(1),
  phoneNumber: z.string().min(1),
});

export type RegisterFormValues = z.infer<typeof registerSchema>;
