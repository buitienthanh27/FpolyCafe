<template>
  <div class="manage-bills-wrapper slide-up">
    <div class="page-header glass-card">
      <div>
        <h2 class="title">Quản lý hóa đơn</h2>
        <p class="subtitle">Theo dõi đơn đang thực hiện, đã hoàn tất và đã hủy từ quầy thu ngân.</p>
      </div>
      <div class="stats">
        <div class="stat-box">
          <Clock3 class="stat-icon processing" :size="20" />
          <span>{{ processingBills.length }} đang thực hiện</span>
        </div>
        <div class="stat-box">
          <CheckCircle2 class="stat-icon finished" :size="20" />
          <span>{{ finishedBills.length }} hoàn tất</span>
        </div>
      </div>
    </div>

    <div v-if="loading" class="loading-state glass-card">
      <div class="loader"></div>
      <p>Đang tải dữ liệu hóa đơn...</p>
    </div>

    <div v-else-if="bills.length === 0" class="empty-state glass-card">
      <Coffee class="empty-icon" :size="48" />
      <h3>Chưa có hóa đơn nào</h3>
      <p>Đơn hàng mới từ quầy thu ngân sẽ hiển thị tại đây.</p>
    </div>

    <div v-else class="bills-grid">
      <div v-for="bill in bills" :key="bill.billId" class="bill-card glass-card">
        <div class="bill-header">
          <div class="bill-id">#{{ bill.billId }}</div>
          <div class="bill-time">
            <CalendarClock :size="16" />
            {{ formatDate(bill.createdAt) }}
          </div>
        </div>

        <div class="bill-meta">
          <div class="meta-block">
            <span class="meta-label">Khách hàng</span>
            <strong>{{ bill.customerName || 'Khách vãng lai' }}</strong>
          </div>
          <div class="meta-block">
            <span class="meta-label">Thu ngân</span>
            <strong>{{ bill.createdByName || '--' }}</strong>
          </div>
          <span :class="['status-badge', statusClass(bill.status)]">
            {{ statusLabel(bill.status) }}
          </span>
        </div>

        <div v-if="bill.promotionCode" class="promotion-row">
          <TicketPercent :size="16" />
          <span>Mã giảm giá: {{ bill.promotionCode }}</span>
        </div>

        <div class="bill-items">
          <div v-for="item in bill.details" :key="item.billDetailId" class="item-row">
            <div class="item-main">
              <span class="quantity">{{ item.quantity }}x</span>
              <span class="name">{{ item.productName }} ({{ item.sizeName }})</span>
            </div>
            <div v-if="item.toppings?.length" class="item-toppings">
              + {{ item.toppings.map((topping) => topping.toppingName).join(', ') }}
            </div>
            <div v-if="item.notes" class="item-note">Ghi chú: {{ item.notes }}</div>
          </div>
        </div>

        <div class="bill-summary">
          <div>
            <span>Tạm tính</span>
            <strong>{{ formatCurrency(bill.subtotalAmount) }}</strong>
          </div>
          <div v-if="bill.discountAmount > 0">
            <span>Giảm giá</span>
            <strong>-{{ formatCurrency(bill.discountAmount) }}</strong>
          </div>
          <div class="total-row">
            <span>Tổng cộng</span>
            <strong>{{ formatCurrency(bill.totalAmount) }}</strong>
          </div>
        </div>

        <div class="bill-footer" v-if="canCancel(bill.status)">
          <button class="btn-cancel" @click="handleCancel(bill.billId)">
            <XCircle :size="18" />
            Hủy đơn
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';
import { CalendarClock, CheckCircle2, Clock3, Coffee, TicketPercent, XCircle } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import { cancelBill } from '../api/bill';
import api from '../api';

const bills = ref([]);
const loading = ref(false);

const processingBills = computed(() => bills.value.filter((bill) => bill.status === 'Processing'));
const finishedBills = computed(() => bills.value.filter((bill) => bill.status === 'Finished'));

const loadBills = async () => {
  loading.value = true;
  try {
    const data = await api.get('/Bills');
    bills.value = Array.isArray(data) ? data : [];
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể tải dữ liệu hóa đơn.', 'error');
  } finally {
    loading.value = false;
  }
};

const handleCancel = async (billId) => {
  const result = await Swal.fire({
    icon: 'warning',
    title: 'Xác nhận hủy đơn',
    text: `Bạn có chắc muốn hủy hóa đơn #${billId}?`,
    showCancelButton: true,
    confirmButtonText: 'Hủy đơn',
    cancelButtonText: 'Đóng',
    confirmButtonColor: '#dc2626'
  });

  if (!result.isConfirmed) {
    return;
  }

  try {
    await cancelBill(billId);
    await Swal.fire('Thành công', 'Hóa đơn đã được hủy.', 'success');
    await loadBills();
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể hủy hóa đơn.', 'error');
  }
};

const canCancel = (status) => status === 'Waiting' || status === 'Processing';

const statusLabel = (status) => {
  if (status === 'Waiting') return 'Đơn nháp';
  if (status === 'Processing') return 'Đang thực hiện';
  if (status === 'Finished') return 'Hoàn tất';
  if (status === 'Cancelled') return 'Đã hủy';
  return status || 'Không rõ';
};

const statusClass = (status) => {
  if (status === 'Processing') return 'processing';
  if (status === 'Finished') return 'finished';
  if (status === 'Cancelled') return 'cancelled';
  return 'waiting';
};

const formatCurrency = (value) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(Number(value || 0));

const formatDate = (dateString) => {
  if (!dateString) {
    return '--';
  }

  const date = new Date(dateString);
  return `${date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - ${date.toLocaleDateString('vi-VN')}`;
};

onMounted(loadBills);
</script>

<style scoped>
.manage-bills-wrapper {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.glass-card {
  background: rgba(255, 255, 255, 0.75);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
  border-radius: 1.25rem;
  padding: 1.5rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
}

.title {
  font-size: 1.8rem;
  color: var(--primary);
  margin: 0 0 0.35rem;
}

.subtitle {
  color: #666;
  margin: 0;
}

.stats {
  display: flex;
  gap: 0.75rem;
}

.stat-box {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  border-radius: 999px;
  padding: 0.6rem 1rem;
  background: rgba(255, 255, 255, 0.92);
  font-weight: 700;
  color: var(--primary);
}

.stat-icon.processing {
  color: #f59e0b;
}

.stat-icon.finished {
  color: #16a34a;
}

.loading-state,
.empty-state {
  min-height: 280px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  text-align: center;
  color: #666;
}

.loader {
  width: 42px;
  height: 42px;
  border: 4px solid rgba(0, 0, 0, 0.08);
  border-top-color: var(--secondary);
  border-radius: 50%;
  animation: spin 0.9s linear infinite;
  margin-bottom: 1rem;
}

.bills-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: 1.5rem;
}

.bill-card {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.bill-header,
.bill-meta {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
}

.bill-id {
  background: var(--primary);
  color: #fff;
  padding: 0.35rem 0.8rem;
  border-radius: 999px;
  font-weight: 700;
}

.bill-time,
.promotion-row {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  color: #8a5a2b;
  font-size: 0.9rem;
}

.meta-block {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.meta-label {
  font-size: 0.78rem;
  color: #888;
}

.status-badge {
  padding: 0.4rem 0.8rem;
  border-radius: 999px;
  font-weight: 700;
  font-size: 0.85rem;
}

.status-badge.waiting {
  background: #fef3c7;
  color: #92400e;
}

.status-badge.processing {
  background: #fde68a;
  color: #92400e;
}

.status-badge.finished {
  background: #dcfce7;
  color: #166534;
}

.status-badge.cancelled {
  background: #fee2e2;
  color: #991b1b;
}

.bill-items {
  display: flex;
  flex-direction: column;
  gap: 0.9rem;
  padding: 1rem 0;
  border-top: 1px dashed rgba(0, 0, 0, 0.1);
  border-bottom: 1px dashed rgba(0, 0, 0, 0.1);
}

.item-row {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.item-main {
  display: flex;
  gap: 0.4rem;
  color: var(--primary);
  font-weight: 600;
}

.quantity {
  color: var(--secondary);
}

.item-toppings,
.item-note {
  color: #666;
  font-size: 0.9rem;
  padding-left: 1.5rem;
}

.bill-summary {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
}

.bill-summary > div {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
}

.total-row {
  font-size: 1.05rem;
  color: var(--primary);
}

.bill-footer {
  display: flex;
  justify-content: flex-end;
}

.btn-cancel {
  border: none;
  border-radius: 0.9rem;
  background: #fee2e2;
  color: #b91c1c;
  padding: 0.8rem 1rem;
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  font-weight: 700;
  cursor: pointer;
}

.empty-icon {
  color: #d6d3d1;
  margin-bottom: 1rem;
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
