/**
 * Confirmed by backend source — BuildingBlock.SharedKernel.Constants.AppRole.
 * `Root` is a superuser role that satisfies every role check/authorization
 * policy on the backend (see `ClaimsPrincipalExtensions.HasRole`) — treat it
 * the same way on the frontend, see `hasRequiredRole` in `shared/config/nav.ts`.
 */
export const AppRole = {
  Root: 'Root',
  Admin: 'Admin',
  User: 'User',
} as const;

export type AppRole = (typeof AppRole)[keyof typeof AppRole];
