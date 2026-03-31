import { reactive } from 'vue';

const STORAGE_KEY = 'fpolycafe-current-order';

const createEmptyOrder = () => ({
  cart: [],
  selectedCustomer: null,
  subtotal: 0,
  discountAmount: 0,
  promotionCode: '',
  promotionType: '',
  promotionValue: 0,
  totalPrice: 0
});

const loadStoredOrder = () => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return createEmptyOrder();
    }

    const parsed = JSON.parse(raw);
    return { ...createEmptyOrder(), ...parsed };
  } catch (error) {
    console.error('[STORE ERROR] Failed to restore current order:', error);
    return createEmptyOrder();
  }
};

const persistOrder = (order) => {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(order));
  } catch (error) {
    console.error('[STORE ERROR] Failed to persist current order:', error);
  }
};

export const store = reactive({
  currentOrder: loadStoredOrder(),
  setOrder(order) {
    this.currentOrder = { ...createEmptyOrder(), ...order };
    persistOrder(this.currentOrder);
  },
  clearOrder() {
    this.currentOrder = createEmptyOrder();
    persistOrder(this.currentOrder);
  }
});
