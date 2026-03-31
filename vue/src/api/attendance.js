import api from './index';

export const attendanceApi = {
  getAll: (params = {}) => api.get('/Attendance', { params }),
  getMyToday: () => api.get('/Attendance/me/today'),
  getOpenShift: () => api.get('/Attendance/me/open-shift'),
  getMyHistory: (params = {}) => api.get('/Attendance/me/history', { params }),
  checkIn: (data) => api.post('/Attendance/check-in', data),
  startBreak: (data) => api.post('/Attendance/break/start', data),
  endBreak: (data) => api.post('/Attendance/break/end', data),
  checkOut: (data) => api.post('/Attendance/check-out', data),
  update: (id, data) => api.put(`/Attendance/${id}`, data),
  autoClose: (params = {}) => api.post('/Attendance/auto-close', null, { params }),
  getDashboard: (params = {}) => api.get('/Attendance/dashboard', { params }),
  getEmployeeSummaries: (params = {}) => api.get('/Attendance/employee-summaries', { params })
};

export default attendanceApi;
