import client from './client';

export const adminApi = {
  getUsers: (params) => client.get('/api/admin/users', { params }),
  deleteUser: (id) => client.delete(`/api/admin/users/${id}`),
  getValidationRanges: () => client.get('/api/admin/validation-ranges'),
  updateValidationRange: (id, data) => client.put(`/api/admin/validation-ranges/${id}`, data)
};
