import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';
import type { NotificationChannelType, NotificationTemplateStatus } from './types/enums';

export interface GetNotificationTemplateResponse {
  id: string;
  name: string | null;
  channel: NotificationChannelType;
  subject: string | null;
  body: string | null;
  variables: string[] | null;
  status: NotificationTemplateStatus;
  createdAt: string;
}

/** GET /notification-templates/{templateId} */
export function getNotificationTemplate(
  templateId: string,
): Promise<GetNotificationTemplateResponse> {
  return apiClient.get(`${BASE_PATH}/notification-templates/${templateId}`).then((res) => res.data);
}
