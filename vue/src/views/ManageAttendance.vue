<template>
  <div class="manage-attendance">
    <div class="page-header slide-up">
      <div class="title-section">
        <h2>Quan ly cham cong</h2>
        <p>Theo doi, loc va dieu chinh ca lam viec cua nhan vien theo backend FpolyCafe.</p>
      </div>
      <button class="btn btn-secondary" @click="handleAutoClose">Auto close</button>
    </div>

    <div class="summary-grid slide-up" style="animation-delay: 0.1s">
      <div class="summary-card glass-card"><h4>Dang lam</h4><strong>{{ dashboard.activeEmployees }}</strong></div>
      <div class="summary-card glass-card"><h4>Dang nghi</h4><strong>{{ dashboard.employeesOnBreak }}</strong></div>
      <div class="summary-card glass-card"><h4>Hoan tat</h4><strong>{{ dashboard.completedShifts }}</strong></div>
      <div class="summary-card glass-card"><h4>Thieu checkout</h4><strong>{{ dashboard.missingCheckoutShifts }}</strong></div>
    </div>

    <div class="filters-row slide-up" style="animation-delay: 0.2s">
      <div class="search-box">
        <Search :size="18" />
        <input v-model="searchQuery" type="text" placeholder="Tim ten nhan vien..." />
      </div>
      <div class="filter-group">
        <input v-model="filterDate" type="date" class="date-input" />
        <select v-model="filterStatus" class="status-select">
          <option value="">Tat ca trang thai</option>
          <option value="Working">Working</option>
          <option value="OnBreak">OnBreak</option>
          <option value="Completed">Completed</option>
          <option value="Adjusted">Adjusted</option>
          <option value="MissingCheckout">MissingCheckout</option>
        </select>
      </div>
    </div>

    <div class="table-container glass-card slide-up" style="animation-delay: 0.3s">
      <div v-if="isLoading" class="state-box">
        <Loader2 class="spin" :size="36" />
        <p>Dang tai du lieu cham cong...</p>
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Nhan vien</th>
            <th>Check in</th>
            <th>Check out</th>
            <th>Lam viec</th>
            <th>Nghi</th>
            <th>OT</th>
            <th>Trang thai</th>
            <th>Thao tac</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in filteredAttendances" :key="item.attendanceId">
            <td>{{ item.employeeName }}</td>
            <td>{{ formatDateTime(item.checkInTime) }}</td>
            <td>{{ item.checkOutTime ? formatDateTime(item.checkOutTime) : '--' }}</td>
            <td>{{ item.workedMinutes }}</td>
            <td>{{ item.breakMinutes }}</td>
            <td>{{ item.overtimeMinutes }}</td>
            <td><span :class="['status-badge', mapStatusClass(item.status)]">{{ item.status }}</span></td>
            <td><button class="icon-btn edit" @click="openModal(item)"><Pencil :size="18" /></button></td>
          </tr>
          <tr v-if="filteredAttendances.length === 0">
            <td colspan="8" class="empty-msg">Khong co ban ghi phu hop.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="summary-table glass-card slide-up" style="animation-delay: 0.4s">
      <h3>Tong hop theo nhan vien</h3>
      <table class="data-table small">
        <thead>
          <tr>
            <th>Nhan vien</th>
            <th>So ca</th>
            <th>Phut lam</th>
            <th>OT</th>
            <th>Luong</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in employeeSummaries" :key="item.employeeId">
            <td>{{ item.employeeName }}</td>
            <td>{{ item.shiftCount }}</td>
            <td>{{ item.workedMinutes }}</td>
            <td>{{ item.overtimeMinutes }}</td>
            <td>{{ formatCurrency(item.salaryAmount) }}</td>
          </tr>
          <tr v-if="employeeSummaries.length === 0">
            <td colspan="5" class="empty-msg">Chua co du lieu tong hop.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal && selectedAttendance" class="modal-overlay" @click.self="showModal = false">
      <div class="modal-content glass-card">
        <div class="modal-header">
          <h3>Dieu chinh cham cong</h3>
          <button class="icon-btn close" @click="showModal = false"><X :size="18" /></button>
        </div>

        <form class="modal-body" @submit.prevent="handleSubmit">
          <div class="form-group">
            <label>Nhan vien</label>
            <input :value="selectedAttendance.employeeName" disabled />
          </div>
          <div class="row">
            <div class="form-group">
              <label>Check in</label>
              <input v-model="form.checkInTime" type="datetime-local" required />
            </div>
            <div class="form-group">
              <label>Check out</label>
              <input v-model="form.checkOutTime" type="datetime-local" />
            </div>
          </div>
          <div class="form-group">
            <label>Ly do</label>
            <input v-model="form.reason" type="text" required placeholder="Vi du: quen checkout" />
          </div>
          <div class="form-group">
            <label>Ghi chu</label>
            <textarea v-model="form.notes" rows="3" />
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="showModal = false">Huy</button>
            <button type="submit" class="btn btn-primary" :disabled="isSubmitting">{{ isSubmitting ? 'Dang luu...' : 'Luu dieu chinh' }}</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { Loader2, Pencil, Search, X } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import attendanceApi from '../api/attendance';

const attendances = ref([]);
const employeeSummaries = ref([]);
const dashboard = ref({
  activeEmployees: 0,
  employeesOnBreak: 0,
  completedShifts: 0,
  missingCheckoutShifts: 0,
  totalOvertimeMinutes: 0
});
const isLoading = ref(false);
const isSubmitting = ref(false);
const searchQuery = ref('');
const filterDate = ref('');
const filterStatus = ref('');
const showModal = ref(false);
const selectedAttendance = ref(null);

const form = ref({
  checkInTime: '',
  checkOutTime: '',
  reason: '',
  notes: ''
});

const filteredAttendances = computed(() => {
  const keyword = searchQuery.value.trim().toLowerCase();
  return attendances.value.filter((item) => {
    const matchName = keyword ? item.employeeName.toLowerCase().includes(keyword) : true;
    const matchStatus = filterStatus.value ? item.status === filterStatus.value : true;
    return matchName && matchStatus;
  });
});

const toDateTimeLocal = (value) => {
  if (!value) return '';
  const date = new Date(value);
  const pad = (input) => String(input).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

const buildRangeParams = () => {
  if (!filterDate.value) return {};
  return {
    from: `${filterDate.value}T00:00:00`,
    to: `${filterDate.value}T23:59:59`
  };
};

const loadData = async () => {
  isLoading.value = true;
  try {
    const rangeParams = buildRangeParams();
    const attendanceParams = {
      ...rangeParams,
      ...(filterStatus.value ? { status: filterStatus.value } : {})
    };

    const [attendanceData, dashboardData, employeeSummaryData] = await Promise.all([
      attendanceApi.getAll(attendanceParams),
      attendanceApi.getDashboard(filterDate.value ? { date: filterDate.value } : {}),
      attendanceApi.getEmployeeSummaries(rangeParams)
    ]);

    attendances.value = attendanceData || [];
    dashboard.value = dashboardData || dashboard.value;
    employeeSummaries.value = employeeSummaryData || [];
  } catch (error) {
    console.error('Failed to load attendance admin data:', error);
    Swal.fire('Loi', error?.message || 'Khong the tai du lieu cham cong.', 'error');
  } finally {
    isLoading.value = false;
  }
};

const openModal = (item) => {
  selectedAttendance.value = item;
  form.value = {
    checkInTime: toDateTimeLocal(item.checkInTime),
    checkOutTime: toDateTimeLocal(item.checkOutTime),
    reason: '',
    notes: item.notes || ''
  };
  showModal.value = true;
};

const handleSubmit = async () => {
  if (!selectedAttendance.value) return;

  isSubmitting.value = true;
  try {
    await attendanceApi.update(selectedAttendance.value.attendanceId, {
      checkInTime: new Date(form.value.checkInTime).toISOString(),
      checkOutTime: form.value.checkOutTime ? new Date(form.value.checkOutTime).toISOString() : null,
      reason: form.value.reason,
      notes: form.value.notes || null
    });

    showModal.value = false;
    await loadData();
    Swal.fire('Thanh cong', 'Da cap nhat ban ghi cham cong.', 'success');
  } catch (error) {
    Swal.fire('Loi', error?.message || 'Khong the cap nhat ban ghi.', 'error');
  } finally {
    isSubmitting.value = false;
  }
};

const handleAutoClose = async () => {
  try {
    const result = await attendanceApi.autoClose();
    await loadData();
    Swal.fire('Thanh cong', `Da dong ${result.updated ?? 0} ca dang mo.`, 'success');
  } catch (error) {
    Swal.fire('Loi', error?.message || 'Khong the auto-close.', 'error');
  }
};

const mapStatusClass = (status) => {
  switch (status) {
    case 'Working': return 'working';
    case 'OnBreak': return 'break';
    case 'Completed': return 'done';
    case 'Adjusted': return 'adjusted';
    case 'MissingCheckout': return 'warning';
    default: return 'neutral';
  }
};

const formatDateTime = (value) => new Date(value).toLocaleString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
const formatCurrency = (value) => Number(value || 0).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });

watch([filterDate, filterStatus], loadData);
onMounted(loadData);
</script>

<style scoped>
.manage-attendance { display: flex; flex-direction: column; gap: 1.5rem; }
.glass-card { background: rgba(255,255,255,.78); backdrop-filter: blur(16px); border: 1px solid rgba(255,255,255,.45); border-radius: 1.25rem; box-shadow: 0 12px 32px rgba(0,0,0,.05); }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; }
.title-section h2 { font-size: 2rem; color: var(--primary); margin-bottom: .3rem; }
.title-section p { color: #6b7280; }
.btn { border: none; border-radius: .9rem; padding: .8rem 1rem; font-weight: 700; cursor: pointer; }
.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.summary-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; }
.summary-card { padding: 1.1rem; }
.summary-card h4 { color: #777; margin-bottom: .45rem; font-size: .88rem; }
.summary-card strong { font-size: 1.8rem; color: var(--primary); }
.filters-row { display: flex; justify-content: space-between; gap: 1rem; }
.search-box { display: flex; align-items: center; gap: .75rem; background: white; border-radius: .95rem; padding: .85rem 1rem; flex: 1; }
.search-box input { border: none; outline: none; width: 100%; }
.filter-group { display: flex; gap: .75rem; }
.date-input, .status-select, .form-group input, .form-group textarea { border: 1px solid #d1d5db; border-radius: .9rem; padding: .8rem .95rem; outline: none; font-family: inherit; }
.table-container, .summary-table { padding: 1.25rem; }
.summary-table h3 { margin-bottom: 1rem; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th, .data-table td { padding: .9rem; text-align: left; border-bottom: 1px solid #f3f4f6; }
.data-table th { font-size: .8rem; color: #6b7280; text-transform: uppercase; }
.status-badge { display: inline-block; padding: .35rem .7rem; border-radius: 999px; font-size: .78rem; font-weight: 700; }
.status-badge.working { background: #dcfce7; color: #166534; }
.status-badge.break { background: #fef3c7; color: #92400e; }
.status-badge.done { background: #dbeafe; color: #1d4ed8; }
.status-badge.adjusted { background: #ede9fe; color: #6d28d9; }
.status-badge.warning { background: #fee2e2; color: #b91c1c; }
.status-badge.neutral { background: #f3f4f6; color: #4b5563; }
.state-box, .empty-msg { text-align: center; color: #9ca3af; padding: 2rem; }
.icon-btn { width: 36px; height: 36px; border: none; border-radius: .75rem; cursor: pointer; display: inline-flex; align-items: center; justify-content: center; }
.icon-btn.edit { background: #fef3c7; color: #92400e; }
.icon-btn.close { background: transparent; color: #6b7280; }
.spin { animation: rotate 1.2s linear infinite; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.38); display: flex; justify-content: center; align-items: center; backdrop-filter: blur(4px); z-index: 1000; }
.modal-content { width: min(560px, 92vw); padding: 1.4rem; }
.modal-header, .modal-footer { display: flex; justify-content: space-between; align-items: center; }
.modal-body { display: flex; flex-direction: column; gap: 1rem; }
.row { display: grid; grid-template-columns: 1fr 1fr; gap: .75rem; }
.form-group { display: flex; flex-direction: column; gap: .45rem; }
@keyframes rotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
@media (max-width: 960px) { .summary-grid { grid-template-columns: repeat(2, 1fr); } .filters-row { flex-direction: column; } .row { grid-template-columns: 1fr; } }
</style>
