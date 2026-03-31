import api from './index';

const shiftApi = {
  getAll: (params = {}) => api.get('/Shifts', { params }),
  getSummary: (params = {}) => api.get('/Shifts/summary', { params })
};

export default shiftApi;
