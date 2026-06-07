import client from './client';

export const farmsApi = {
  getAll: (params) => client.get('/api/farms', { params }),
  create: (data) => client.post('/api/farms', data),
  update: (id, data) => client.put(`/api/farms/${id}`, data),
  delete: (id) => client.delete(`/api/farms/${id}`),

  getValidationRanges: (farmId) => client.get(`/api/farms/${farmId}/validation-ranges`),
  upsertValidationRange: (farmId, data) => client.post(`/api/farms/${farmId}/validation-ranges`, data),
  deleteValidationRange: (farmId, rangeId) => client.delete(`/api/farms/${farmId}/validation-ranges/${rangeId}`)
};
