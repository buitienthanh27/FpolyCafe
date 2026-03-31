<template>
  <div class="payment-container">
    <div class="payment-card glass">
      <header class="payment-header">
        <button @click="goBack" class="btn-back">
          <ChevronLeft :size="22" />
          Quay lại
        </button>
        <h1>Thanh toán đơn hàng</h1>
        <div class="order-id">Mã đơn tạm: #{{ Date.now().toString().slice(-6) }}</div>
      </header>

      <div class="payment-layout">
        <aside class="order-summary">
          <h2>Tóm tắt đơn hàng</h2>

          <div class="summary-items">
            <div v-for="item in store.currentOrder.cart" :key="item.cartId" class="summary-item">
              <div class="item-info">
                <span class="item-name">{{ item.name }} x {{ item.quantity }}</span>
                <span v-if="item.customization" class="item-details">
                  {{ item.customization.sizeName || 'Mặc định' }}
                  <template v-if="item.customization.extras?.length">
                    , {{ item.customization.extras.map((extra) => extra.toppingName).join(', ') }}
                  </template>
                </span>
                <span v-if="item.customization?.notes" class="item-notes">{{ item.customization.notes }}</span>
              </div>
              <span class="item-price">{{ formatPrice(item.price * item.quantity) }}đ</span>
            </div>
          </div>

          <div class="summary-totals">
            <div class="line">
              <span>Tạm tính</span>
              <span>{{ formatPrice(store.currentOrder.subtotal) }}đ</span>
            </div>
            <div v-if="store.currentOrder.discountAmount > 0" class="line discount">
              <span>Mã giảm giá ({{ store.currentOrder.promotionCode }})</span>
              <span>-{{ formatPrice(store.currentOrder.discountAmount) }}đ</span>
            </div>
            <div class="line total">
              <span>Tổng cộng</span>
              <span>{{ formatPrice(store.currentOrder.totalPrice) }}đ</span>
            </div>
          </div>

          <div v-if="store.currentOrder.selectedCustomer" class="customer-info">
            <h3>Khách hàng</h3>
            <div class="cust-card">
              <User :size="20" />
              <div>
                <p class="name">{{ store.currentOrder.selectedCustomer.fullName }}</p>
                <p class="phone">{{ store.currentOrder.selectedCustomer.phoneNumber }}</p>
                <p class="points">Điểm: {{ store.currentOrder.selectedCustomer.rewardPoints }}</p>
              </div>
            </div>
          </div>
        </aside>

        <main class="payment-steps">
          <div class="steps-nav">
            <div :class="['step-tab', { active: currentStep === 1, completed: currentStep > 1 }]">
              <span class="step-num">1</span>
              <span class="step-label">Phương thức</span>
            </div>
            <div class="step-line"></div>
            <div :class="['step-tab', { active: currentStep === 2, completed: currentStep > 2 }]">
              <span class="step-num">2</span>
              <span class="step-label">Thực hiện</span>
            </div>
            <div class="step-line"></div>
            <div :class="['step-tab', { active: currentStep === 3 }]">
              <span class="step-num">3</span>
              <span class="step-label">Hoàn tất</span>
            </div>
          </div>

          <div v-if="currentStep === 1" class="step-content">
            <h3>Chọn phương thức thanh toán</h3>
            <div class="method-grid">
              <button
                v-for="method in methods"
                :key="method.id"
                :class="['method-card', { active: selectedMethod === method.id }]"
                @click="selectedMethod = method.id"
              >
                <component :is="method.icon" :size="32" />
                <span>{{ method.label }}</span>
              </button>
            </div>
            <button @click="nextStep" class="btn-next" :disabled="!selectedMethod">
              Tiếp tục
              <ChevronRight :size="20" />
            </button>
          </div>

          <div v-if="currentStep === 2" class="step-content">
            <div v-if="selectedMethod === 'cash'" class="payment-mode">
              <h3>Thanh toán tiền mặt</h3>
              <div class="input-group">
                <label>Tiền khách đưa (VNĐ)</label>
                <input
                  v-model="amountReceived"
                  type="number"
                  min="0"
                  placeholder="Nhập số tiền..."
                  @input="calculateChange"
                />
              </div>
              <div class="change-display">
                <span>Tiền thừa</span>
                <strong :class="{ negative: change < 0 }">{{ formatPrice(change) }}đ</strong>
              </div>
            </div>

            <div v-if="selectedMethod === 'qr'" class="payment-mode">
              <h3>Quét mã QR chuyển khoản</h3>
              <div class="qr-container">
                <img :src="qrImageUrl" alt="QR thanh toán" />
                <p>Số tiền: {{ formatPrice(store.currentOrder.totalPrice) }}đ</p>
              </div>
              <p class="instruction">Vui lòng quét mã trên ứng dụng ngân hàng hoặc ví điện tử.</p>
            </div>

            <div v-if="selectedMethod === 'card'" class="payment-mode">
              <h3>Thanh toán qua thẻ</h3>
              <div class="card-icon-anim">
                <CreditCard :size="64" />
              </div>
              <p class="instruction">Vui lòng quẹt hoặc chèn thẻ vào thiết bị POS.</p>
            </div>

            <div class="step-footer">
              <button @click="currentStep = 1" class="btn-prev">Quay lại</button>
              <button @click="finishPayment" class="btn-finish" :disabled="isSubmitDisabled">
                {{ isProcessing ? 'Đang xử lý...' : 'Xác nhận thanh toán' }}
              </button>
            </div>
          </div>

          <div v-if="currentStep === 3" class="step-content success-view">
            <div class="success-icon">
              <CheckCircle :size="80" />
            </div>
            <h2>Thanh toán thành công!</h2>
            <p>Đơn hàng đã được chuyển sang khu vực pha chế và đang thực hiện.</p>

            <div class="option-grid">
              <button @click="printReceipt" class="option-card">
                <Printer :size="24" />
                <span>In hóa đơn</span>
              </button>
              <button @click="shareReceipt" class="option-card">
                <Share2 :size="24" />
                <span>Gửi hóa đơn</span>
              </button>
            </div>

            <button @click="completeOrder" class="btn-complete">Bắt đầu đơn mới</button>
          </div>
        </main>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  ChevronLeft, ChevronRight, User,
  Banknote, QrCode, CreditCard,
  CheckCircle, Printer, Share2
} from 'lucide-vue-next';
import Swal from 'sweetalert2';
import { store } from '../store';
import { createBill, addItemToBill, submitBill } from '../api/bill';

const router = useRouter();
const currentStep = ref(1);
const selectedMethod = ref('cash');
const amountReceived = ref(null);
const change = ref(0);
const isProcessing = ref(false);

const methods = [
  { id: 'cash', label: 'Tiền mặt', icon: Banknote },
  { id: 'qr', label: 'Mã QR', icon: QrCode },
  { id: 'card', label: 'Quẹt thẻ', icon: CreditCard }
];

const qrImageUrl = computed(
  () => `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=FpolyCafe_${store.currentOrder.totalPrice}`
);

const isSubmitDisabled = computed(
  () => isProcessing.value || (selectedMethod.value === 'cash' && change.value < 0)
);

const formatPrice = (price) => Number(price || 0).toLocaleString('vi-VN');

const goBack = () => {
  router.push('/cashier');
};

const nextStep = () => {
  currentStep.value = 2;
  if (selectedMethod.value === 'cash') {
    amountReceived.value = store.currentOrder.totalPrice;
    calculateChange();
  }
};

const calculateChange = () => {
  if (amountReceived.value === null || amountReceived.value === '') {
    change.value = 0;
    return;
  }

  change.value = Number(amountReceived.value) - Number(store.currentOrder.totalPrice || 0);
};

const finishPayment = async () => {
  if (isSubmitDisabled.value) {
    return;
  }

  isProcessing.value = true;
  try {
    const rawUserId = localStorage.getItem('userId');
    const userId = rawUserId ? Number(rawUserId) : null;
    const customerId = store.currentOrder.selectedCustomer?.customerId ?? null;

    const billId = await createBill(userId, customerId);
    for (const item of store.currentOrder.cart) {
      await addItemToBill(billId, item);
    }

    await submitBill(billId, store.currentOrder.promotionCode);

    currentStep.value = 3;
    Swal.fire({
      icon: 'success',
      title: 'Thành công',
      text: 'Đơn hàng đã được gửi sang pha chế và đang thực hiện.',
      timer: 2000,
      showConfirmButton: false
    });
  } catch (error) {
    console.error('Lỗi thanh toán:', error);
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: 'Không thể xử lý thanh toán. Vui lòng thử lại.'
    });
  } finally {
    isProcessing.value = false;
  }
};

const printReceipt = () => {
  Swal.fire('In hóa đơn', 'Đang kết nối với máy in...', 'info');
};

const shareReceipt = () => {
  Swal.fire('Chia sẻ', 'Hóa đơn đã được gửi qua Email/Zalo.', 'success');
};

const completeOrder = () => {
  store.clearOrder();
  router.push('/cashier');
};

const checkOrder = () => {
  if (store.currentOrder.cart.length === 0) {
    router.push('/cashier');
  }
};

onMounted(() => {
  checkOrder();
});
</script>

<style scoped>
.payment-container {
  min-height: 100vh;
  background: #f5f0eb;
  padding: 2rem;
  display: flex;
  justify-content: center;
  align-items: flex-start;
}

.payment-card {
  width: 1000px;
  max-width: 95vw;
  border-radius: 30px;
  padding: 2rem;
  display: flex;
  flex-direction: column;
  gap: 2rem;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.05);
}

.payment-header {
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  align-items: center;
  gap: 1rem;
}

.btn-back,
.btn-prev,
.btn-finish,
.btn-next,
.btn-complete,
.option-card,
.method-card {
  font-family: inherit;
}

.btn-back {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  font-weight: 600;
}

.payment-header h1 {
  text-align: center;
  font-size: 1.85rem;
  color: #2c1810;
}

.order-id {
  text-align: right;
  color: #999;
  font-weight: 600;
}

.payment-layout {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 2rem;
}

.order-summary {
  background: rgba(255, 255, 255, 0.55);
  padding: 1.5rem;
  border-radius: 24px;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.summary-items {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  max-height: 320px;
  overflow-y: auto;
}

.summary-item,
.line,
.step-footer {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
}

.item-info {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.item-name {
  font-weight: 700;
}

.item-details,
.item-notes,
.phone {
  font-size: 0.88rem;
  color: #666;
}

.summary-totals {
  border-top: 1px dashed #ddd;
  padding-top: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.line.total {
  font-size: 1.2rem;
  font-weight: 800;
  color: #d4a373;
}

.cust-card {
  display: flex;
  gap: 1rem;
  align-items: center;
  padding: 1rem;
  border-radius: 16px;
  background: white;
  border: 1px solid #eee;
}

.name {
  font-weight: 700;
}

.points {
  font-size: 0.88rem;
  color: #d4a373;
  font-weight: 600;
}

.payment-steps {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.steps-nav {
  display: flex;
  align-items: center;
}

.step-tab {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.step-num {
  width: 40px;
  height: 40px;
  border-radius: 999px;
  background: #ececec;
  color: #999;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
}

.step-label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #999;
  text-align: center;
}

.step-tab.active .step-num,
.step-tab.completed .step-num {
  color: white;
}

.step-tab.active .step-num {
  background: #2c1810;
}

.step-tab.completed .step-num {
  background: #d4a373;
}

.step-tab.active .step-label {
  color: #2c1810;
}

.step-line {
  flex: 1;
  height: 2px;
  background: #ececec;
  margin: 0 1rem;
  transform: translateY(-12px);
}

.step-content {
  animation: slide-up 0.25s ease-out;
}

@keyframes slide-up {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.method-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin: 1.5rem 0;
}

.method-card,
.option-card {
  border: 2px solid transparent;
  border-radius: 20px;
  background: white;
  cursor: pointer;
  transition: 0.2s ease;
}

.method-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  padding: 2rem 1rem;
}

.method-card.active {
  border-color: #d4a373;
  background: #fdfaf7;
  color: #d4a373;
}

.btn-next,
.btn-finish,
.btn-complete {
  border: none;
  border-radius: 16px;
  cursor: pointer;
  font-weight: 700;
}

.btn-next,
.btn-finish,
.btn-complete {
  background: #2c1810;
  color: white;
}

.btn-next {
  width: 100%;
  padding: 1rem 1.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.payment-mode {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.input-group input {
  width: 100%;
  padding: 1.25rem;
  font-size: 1.6rem;
  border-radius: 18px;
  border: 2px solid #eee;
  text-align: right;
  font-weight: 700;
  font-family: inherit;
}

.change-display,
.qr-container,
.card-icon-anim {
  border-radius: 20px;
  background: white;
}

.change-display {
  padding: 1.5rem;
  align-items: center;
}

.change-display strong {
  font-size: 2rem;
  color: #2c1810;
}

.change-display strong.negative {
  color: #dc2626;
}

.qr-container,
.card-icon-anim {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem;
}

.instruction {
  color: #666;
}

.btn-prev {
  border: none;
  background: #efe8df;
  color: #2c1810;
  border-radius: 16px;
  padding: 1rem 1.25rem;
  cursor: pointer;
  font-weight: 700;
}

.btn-finish {
  padding: 1rem 1.5rem;
}

.btn-finish:disabled,
.btn-next:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.success-view {
  text-align: center;
}

.success-icon {
  color: #16a34a;
  margin-bottom: 1rem;
}

.option-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
  margin: 1.5rem 0;
}

.option-card {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  padding: 1rem;
}

@media (max-width: 960px) {
  .payment-layout {
    grid-template-columns: 1fr;
  }

  .payment-header {
    grid-template-columns: 1fr;
    justify-items: start;
  }

  .payment-header h1,
  .order-id {
    text-align: left;
  }

  .method-grid,
  .option-grid {
    grid-template-columns: 1fr;
  }
}
</style>
