import client from './client';

export const aiApi = {
  ask: (data) => client.post('/api/ai/ask', data),
  getConversations: (params) => client.get('/api/ai/conversations', { params }),
  getConversation: (id) => client.get(`/api/ai/conversations/${id}`)
};
