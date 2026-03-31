import api from './index';

const recipeApi = {
  getAll: () => api.get('/Recipes'),
  getByProduct: (productId) => api.get(`/Recipes/product/${productId}`),
  saveByProduct: (productId, data) => api.put(`/Recipes/product/${productId}`, data)
};

export default recipeApi;
