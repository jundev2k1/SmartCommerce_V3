import { apiClient } from '@/shared/lib/api/client';
import { BASE_PATH } from './_base';

export interface RebuildProductSearchIndexResponse {
  productsIndexed: number;
}

/** POST /products/search/rebuild — drops and repopulates the Elasticsearch index from PostgreSQL. */
export function rebuildProductSearchIndex(): Promise<RebuildProductSearchIndexResponse> {
  return apiClient.post(`${BASE_PATH}/products/search/rebuild`).then((res) => res.data);
}
