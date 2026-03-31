import api from './index';

export const createBill = async (userId = null, customerId = null) => {
  return api.post('/Bills', {
    userId: Number.isInteger(userId) ? userId : null,
    customerId: Number.isInteger(customerId) ? customerId : null
  });
};

export const addItemToBill = async (billId, item) => {
  return api.post(`/Bills/${billId}/items`, {
    productId: item.productId,
    sizeId: item.customization?.sizeId,
    toppingIds: (item.customization?.extras || []).map((extra) => extra.toppingId),
    quantity: item.quantity,
    note: item.customization?.notes || ''
  });
};

export const submitBill = async (billId, promotionCode = null) =>
  api.post(`/Bills/${billId}/submit`, {
    promotionCode: promotionCode?.trim() ? promotionCode.trim().toUpperCase() : null
  });

export const completeBill = async (billId) => api.post(`/Bills/${billId}/complete`);
export const cancelBill = async (billId) => api.post(`/Bills/${billId}/cancel`);
