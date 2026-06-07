import client from './client';

export const usersApi = {
  getProfile: () => client.get('/api/users/profile'),
  updateProfile: (data) => client.put('/api/users/profile', data),
  changePassword: (data) => client.put('/api/users/password', data)
};
