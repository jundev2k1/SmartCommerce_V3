import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

/** DELETE /tags/{tagId} — refuses if still assigned to any product. */
export function deleteProductTag(tagId: string): Promise<void> {
  return apiClient.delete(`${BASE_PATH}/tags/${tagId}`).then(() => undefined);
}
