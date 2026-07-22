import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { AudienceType, NotificationGroupStatus } from './types/enums';

export interface GetNotificationGroupResponse {
  id: string;
  name: string | null;
  description: string | null;
  status: NotificationGroupStatus;
  audienceType: AudienceType;
  audienceConfigJson: string | null;
  createdAt: string;
}

/** GET /notification-groups/{groupId} */
export function getNotificationGroup(groupId: string): Promise<GetNotificationGroupResponse> {
  return apiClient.get(`${BASE_PATH}/notification-groups/${groupId}`).then((res) => res.data);
}
