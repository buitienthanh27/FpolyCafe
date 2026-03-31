import api from './index';

const inventoryReceiptApi = {
  getAll: (params = {}) => api.get('/InventoryReceipts', { params }),
  getById: (id) => api.get(`/InventoryReceipts/${id}`),
  getSummary: (params = {}) => api.get('/InventoryReceipts/summary', { params }),
  create: (data) => api.post('/InventoryReceipts', data)
};

export default inventoryReceiptApi;
