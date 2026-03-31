<template>
  <div class="bartender-container">
    <div class="header glass slide-up">
      <div class="brand">
        <Coffee :size="32" color="var(--secondary)" />
        <h1>Khu vực <span>Pha chế</span></h1>
      </div>
      <div class="user-info">
        <div class="details">
          <p class="name">{{ fullName }}</p>
          <p class="role">Nhân viên pha chế</p>
        </div>
        <button @click="handleLogout" class="logout-btn">
          <LogOut :size="20" />
        </button>
      </div>
    </div>

    <div class="orders-grid">
      <div v-for="order in activeOrders" :key="order.billId" class="order-card glass slide-up">
        <div class="order-header">
          <div>
            <span class="order-no">#{{ order.billId }}</span>
            <p class="order-customer">{{ order.customerName || 'Khách vãng lai' }}</p>
          </div>
          <span class="time">{{ formatTime(order.createdAt) }}</span>
        </div>

        <div class="order-items">
          <div v-for="item in order.details" :key="item.billDetailId" class="order-item">
            <div class="item-main">
              <span class="qty">{{ item.quantity }}x</span>
              <span class="name">{{ item.productName }}</span>
              <span class="size">({{ item.sizeName }})</span>
            </div>
            <div v-if="item.toppings?.length" class="toppings">
              + {{ item.toppings.map((topping) => topping.toppingName).join(', ') }}
            </div>
            <div v-if="item.notes" class="toppings">
              Ghi chú: {{ item.notes }}
            </div>
          </div>
        </div>

        <div class="order-footer">
          <div class="amount">{{ formatCurrency(order.totalAmount) }}</div>
          <button @click="finishOrder(order.billId)" class="btn-complete">
            <Check :size="20" />
            Hoàn tất đơn
          </button>
        </div>
      </div>

      <div v-if="activeOrders.length === 0 && !loading" class="empty-state glass slide-up">
        <div class="icon-box">
          <CupSoda :size="64" />
        </div>
        <h3>Chưa có đơn đang thực hiện</h3>
        <p>Đơn hàng từ quầy thu ngân sẽ xuất hiện tại đây sau khi thanh toán.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';
import { Check, Coffee, CupSoda, LogOut } from 'lucide-vue-next';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';
import api from '../api';
import { completeBill } from '../api/bill';

const router = useRouter();
const fullName = localStorage.getItem('fullName') || 'Nhân viên pha chế';
const activeOrders = ref([]);
const loading = ref(false);
let pollInterval = null;

const fetchOrders = async () => {
  loading.value = true;
  try {
    const bills = await api.get('/Bills');
    activeOrders.value = (Array.isArray(bills) ? bills : []).filter((bill) => bill.status === 'Processing');
  } catch (error) {
    console.error('Không thể tải danh sách pha chế:', error);
  } finally {
    loading.value = false;
  }
};

const finishOrder = async (billId) => {
  try {
    await completeBill(billId);
    await Swal.fire({
      icon: 'success',
      title: 'Đã hoàn tất',
      text: `Đơn #${billId} đã được cập nhật cho admin.`,
      timer: 1800,
      showConfirmButton: false,
      toast: true,
      position: 'top-end'
    });
    await fetchOrders();
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể hoàn tất đơn hàng.', 'error');
  }
};

const handleLogout = () => {
  localStorage.clear();
  router.push('/login');
};

const formatTime = (dateStr) => {
  const date = new Date(dateStr);
  return `${date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - ${date.toLocaleDateString('vi-VN')}`;
};

const formatCurrency = (value) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(Number(value || 0));

onMounted(() => {
  fetchOrders();
  pollInterval = setInterval(fetchOrders, 10000);
});

onUnmounted(() => {
  if (pollInterval) {
    clearInterval(pollInterval);
  }
});
</script>

<style scoped>
.bartender-container {
  min-height: 100vh;
  padding: 2rem;
  background: var(--background);
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.header {
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-radius: 1.5rem;
}

.brand {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.brand h1 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.6rem;
  color: var(--primary);
}

.brand h1 span {
  color: var(--secondary);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.details {
  text-align: right;
}

.details .name {
  margin: 0;
  font-weight: 700;
  color: var(--primary);
}

.details .role {
  margin: 0.25rem 0 0;
  color: #777;
}

.logout-btn {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  border: none;
  background: #fee2e2;
  color: #ef4444;
  display: flex;
  justify-content: center;
  align-items: center;
  cursor: pointer;
}

.orders-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 1.5rem;
}

.order-card {
  padding: 1.5rem;
  border-radius: 1.5rem;
  border-left: 5px solid var(--secondary);
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.order-header {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px dashed #ddd;
}

.order-no {
  font-weight: 800;
  font-size: 1.2rem;
  color: var(--primary);
}

.order-customer {
  margin: 0.35rem 0 0;
  color: #777;
}

.time {
  color: #888;
  font-size: 0.9rem;
}

.order-items {
  display: flex;
  flex-direction: column;
  gap: 0.9rem;
}

.item-main {
  display: flex;
  gap: 0.45rem;
  font-weight: 700;
  color: var(--primary);
}

.qty {
  color: var(--secondary);
}

.toppings {
  padding-left: 1.8rem;
  color: #777;
  font-size: 0.9rem;
  font-style: italic;
}

.order-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  margin-top: auto;
}

.amount {
  font-weight: 800;
  color: var(--primary);
}

.btn-complete {
  border: none;
  border-radius: 1rem;
  padding: 0.9rem 1.1rem;
  background: var(--primary);
  color: white;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
}

.empty-state {
  grid-column: 1 / -1;
  min-height: 320px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  text-align: center;
  color: #999;
}

.icon-box {
  width: 120px;
  height: 120px;
  background: #f8f1ea;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--secondary);
  margin-bottom: 1.5rem;
}

.glass {
  background: rgba(255, 255, 255, 0.75);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
}

.slide-up {
  animation: slideUp 0.45s ease;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(18px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
