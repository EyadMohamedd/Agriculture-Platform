import client from './client';

export const sensorsApi = {
  getReadings: (params) => client.get('/api/sensors/readings', { params }),
  getLatest: (farmId) => client.get(`/api/sensors/latest/${farmId}`),
  getStatistics: (farmId, params) => client.get(`/api/sensors/statistics/${farmId}`, { params })
};
