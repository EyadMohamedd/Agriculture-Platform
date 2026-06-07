import client from './client';

export const authApi = {
  login: (data) => client.post('/api/auth/login', data),
  register: (data) => client.post('/api/auth/register', data),
  logout: () => client.post('/api/auth/logout'),
  forgotPassword: (data) => client.post('/api/auth/forgot-password', data),
  resetPassword: (data) => client.post('/api/auth/reset-password', data),
  deleteAccount: () => client.delete('/api/auth/account')
};
