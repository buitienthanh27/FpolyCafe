<template>
  <div class="manage-page">
    <div class="page-header">
      <div>
        <h2>Quản lý ca làm việc</h2>
        <p>Theo dõi ca làm việc thực tế của nhân viên dựa trên dữ liệu chấm công.</p>
      </div>
    </div>

    <div class="stats-grid">
      <div class="stat-card">
        <h4>Đang làm việc</h4>
        <strong>{{ summary.activeShifts }}</strong>
      </div>
      <div class="stat-card">
        <h4>Đang nghỉ giữa ca</h4>
        <strong>{{ summary.employeesOnBreak }}</strong>
      </div>
      <div class="stat-card">
        <h4>Đã hoàn tất</h4>
        <strong>{{ summary.completedShifts }}</strong>
      </div>
      <div class="stat-card">
        <h4>Tổng phút làm</h4>
        <strong>{{ summary.totalWorkedMinutes }}</strong>
      </div>
    </div>

    <div class="table-card">
      <div class="toolbar">
        <div class="search-box">
          <Search :size="18" />
          <input v-model="filters.keyword" type="text" placeholder="Tìm theo tên nhân viên..." @keyup.enter="loadData" />
        </div>
        <input v-model="filters.date" class="date-input" type="date" @change="loadData" />
        <select v-model="filters.status" class="select-input" @change="loadData">
          <option value="">Tất cả trạng thái</option>
          <option value="Working">Đang làm</option>
          <option value="OnBreak">Nghỉ giữa ca</option>
          <option value="Completed">Hoàn tất</option>
          <option value="Adjusted">Điều chỉnh</option>
          <option value="MissingCheckout">Thiếu checkout</option>
        </select>
        <button class="btn btn-secondary" @click="loadData">Tải lại</button>
      </div>

      <div v-if="loading" class="state-box">
        <LoaderCircle class="spin" :size="28" />
        <span>Đang tải dữ liệu ca làm việc...</span>
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Nhân viên</th>
            <th>Vai trò</th>
            <th>Check-in</th>
            <th>Check-out</th>
            <th>Làm việc</th>
            <th>Nghỉ</th>
            <th>OT</th>
            <th>Trạng thái</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in shifts" :key="item.attendanceId">
            <td>{{ item.employeeName }}</td>
            <td>{{ item.role }}</td>
            <td>{{ formatDateTime(item.checkInTime) }}</td>
            <td>{{ item.checkOutTime ? formatDateTime(item.checkOutTime) : '--' }}</td>
            <td>{{ item.workedMinutes }} phút</td>
            <td>{{ item.breakMinutes }} phút</td>
            <td>{{ item.overtimeMinutes }} phút</td>
            <td>
              <span :class="['badge', statusClass(item.status)]">{{ statusLabel(item.status) }}</span>
            </td>
          </tr>
          <tr v-if="shifts.length === 0">
            <td colspan="8" class="empty-row">Không có ca làm việc phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { LoaderCircle, Search } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import shiftApi from '../api/shift';

const loading = ref(false);
const shifts = ref([]);
const summary = ref({
  activeShifts: 0,
  completedShifts: 0,
  employeesOnBreak: 0,
  totalWorkedMinutes: 0
});

const filters = reactive({
  keyword: '',
  status: '',
  date: new Date().toISOString().slice(0, 10)
});

const loadData = async () => {
  loading.value = true;
  try {
    const params = {
      ...(filters.keyword.trim() ? { keyword: filters.keyword.trim() } : {}),
      ...(filters.status ? { status: filters.status } : {}),
      ...(filters.date ? { date: filters.date } : {})
    };

    const [shiftData, summaryData] = await Promise.all([
      shiftApi.getAll(params),
      shiftApi.getSummary({ date: filters.date })
    ]);

    shifts.value = shiftData || [];
    summary.value = summaryData || summary.value;
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể tải dữ liệu ca làm việc.', 'error');
  } finally {
    loading.value = false;
  }
};

const formatDateTime = (value) =>
  new Date(value).toLocaleString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });

const statusLabel = (status) => {
  if (status === 'Working') return 'Đang làm';
  if (status === 'OnBreak') return 'Nghỉ giữa ca';
  if (status === 'Completed') return 'Hoàn tất';
  if (status === 'Adjusted') return 'Điều chỉnh';
  return 'Thiếu checkout';
};

const statusClass = (status) => {
  if (status === 'Working') return 'working';
  if (status === 'OnBreak') return 'break';
  if (status === 'Completed') return 'done';
  if (status === 'Adjusted') return 'adjusted';
  return 'warning';
};

onMounted(loadData);
</script>

<style scoped>
.manage-page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; }
.page-header h2 { color: var(--primary); font-size: 2rem; }
.page-header p { color: #6b7280; margin-top: .25rem; }
.stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; }
.stat-card, .table-card { background: rgba(255,255,255,.82); border: 1px solid rgba(255,255,255,.5); border-radius: 1.25rem; box-shadow: 0 12px 32px rgba(0,0,0,.05); backdrop-filter: blur(16px); }
.stat-card { padding: 1.2rem; }
.stat-card h4 { color: #6b7280; font-size: .9rem; margin-bottom: .4rem; }
.stat-card strong { color: var(--primary); font-size: 1.5rem; }
.table-card { padding: 1.25rem; }
.toolbar { display: flex; gap: .75rem; align-items: center; margin-bottom: 1rem; }
.search-box { flex: 1; display: flex; align-items: center; gap: .6rem; background: white; border: 1px solid #e5e7eb; border-radius: .9rem; padding: .75rem 1rem; }
.search-box input, .date-input, .select-input { width: 100%; border: none; outline: none; background: transparent; }
.date-input, .select-input { max-width: 220px; border: 1px solid #d1d5db; border-radius: .9rem; padding: .75rem 1rem; background: white; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th, .data-table td { padding: .9rem; text-align: left; border-bottom: 1px solid #f3f4f6; }
.data-table th { color: #6b7280; font-size: .8rem; text-transform: uppercase; }
.badge { display: inline-flex; padding: .3rem .7rem; border-radius: 999px; font-size: .78rem; font-weight: 700; }
.badge.working { background: #dcfce7; color: #166534; }
.badge.break { background: #fef3c7; color: #92400e; }
.badge.done { background: #dbeafe; color: #1d4ed8; }
.badge.adjusted { background: #ede9fe; color: #6d28d9; }
.badge.warning { background: #fee2e2; color: #b91c1c; }
.state-box, .empty-row { text-align: center; color: #6b7280; padding: 1.25rem; }
.btn { border: none; border-radius: .85rem; padding: .8rem 1rem; cursor: pointer; font-weight: 700; display: inline-flex; align-items: center; gap: .5rem; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 960px) {
  .stats-grid { grid-template-columns: repeat(2, 1fr); }
  .toolbar, .page-header { flex-direction: column; align-items: stretch; }
}
</style>
