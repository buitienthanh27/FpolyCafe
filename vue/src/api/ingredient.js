import api from './index';

const ingredientApi = {
  getAll: (params = {}) => api.get('/Ingredients', { params }),
  getById: (id) => api.get(`/Ingredients/${id}`),
  getSummary: () => api.get('/Ingredients/summary'),
  create: (data) => api.post('/Ingredients', data),
  update: (id, data) => api.put(`/Ingredients/${id}`, data),
  remove: (id) => api.delete(`/Ingredients/${id}`)
};

export default ingredientApi;
