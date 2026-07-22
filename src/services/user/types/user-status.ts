/** Confirmed by backend prose (unlike most other enums in this project — see docs/backend). */
export const UserStatus = {
  Active: 1,
  Inactive: 2,
  Suspended: 3,
} as const;

export type UserStatus = (typeof UserStatus)[keyof typeof UserStatus];
