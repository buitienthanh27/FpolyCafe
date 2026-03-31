import api from './index';

const customerApi = {
  getAll: (params = {}) => api.get('/Customers', { params }),
  getById: (id) => api.get(`/Customers/${id}`),
  create: (data) => api.post('/Customers', data),
  update: (id, data) => api.put(`/Customers/${id}`, data)
};

export default customerApi;
