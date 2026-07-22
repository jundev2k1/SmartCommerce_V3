import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { AudienceType } from './types/enums';

export interface CreateNotificationGroupCommand {
  name: string;
  description?: string;
  audienceType: AudienceType;
  /** Free-form JSON string describing the audience config (shape depends on audienceType). */
  audienceConfigJson?: string;
}

export interface CreateNotificationGroupResponse {
  id: string;
}

/** POST /notification-groups — a target audience that campaigns broadcast to. */
export function createNotificationGroup(
  command: CreateNotificationGroupCommand,
): Promise<CreateNotificationGroupResponse> {
  return apiClient.post(`${BASE_PATH}/notification-groups`, command).then((res) => res.data);
}
