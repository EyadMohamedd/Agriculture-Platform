import client from './client';

export const alertsApi = {
  getAll: (params) => client.get('/api/alerts', { params })
};
