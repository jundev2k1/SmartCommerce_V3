import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface ProductTagItemResponse {
  id: string;
  code: string | null;
  name: string | null;
}

export interface ListProductTagsResponse {
  tags: ProductTagItemResponse[] | null;
}

/** GET /tags — full flat set, not paginated (small reference data). */
export function listProductTags(): Promise<ListProductTagsResponse> {
  return apiClient.get(`${BASE_PATH}/tags`).then((res) => res.data);
}
