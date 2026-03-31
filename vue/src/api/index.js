import axios from 'axios';

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5189/api';

const api = axios.create({
  baseURL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response.data,
  (error) => {
    const normalizedError = {
      status: error.response?.status || 0,
      code: error.response?.data?.code || '',
      isAuthError: error.response?.status === 401,
      message:
        error.response?.data?.message ||
        error.response?.data?.title ||
        error.message ||
        'Đã xảy ra lỗi không xác định.',
      raw: error.response?.data || null,
      url: error.config?.url || '',
      method: (error.config?.method || 'get').toUpperCase()
    };

    console.error('[API ERROR]', {
      url: normalizedError.url,
      method: normalizedError.method,
      status: normalizedError.status,
      code: normalizedError.code,
      message: normalizedError.message,
      raw: normalizedError.raw
    });

    return Promise.reject(normalizedError);
  }
);

export default api;
