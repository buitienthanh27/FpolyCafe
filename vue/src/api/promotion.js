import api from './index';

const promotionApi = {
  getAll: (params = {}) => api.get('/Promotions', { params }),
  getAvailable: (orderAmount = 0) => api.get('/Promotions/available', { params: { orderAmount } }),
  getById: (id) => api.get(`/Promotions/${id}`),
  create: (data) => api.post('/Promotions', data),
  update: (id, data) => api.put(`/Promotions/${id}`, data),
  remove: (id) => api.delete(`/Promotions/${id}`),
  validate: (code, orderAmount) => api.get(`/Promotions/validate/${encodeURIComponent(code)}`, { params: { orderAmount } })
};

export default promotionApi;
