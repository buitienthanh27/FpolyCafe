<template>
  <div class="timekeeping-page">
    <div class="timekeeping-card glass-card">
      <header class="page-header">
        <div>
          <h1>Cham cong nhan vien</h1>
          <p>{{ currentDate }}</p>
        </div>
        <div class="clock">{{ currentTime }}</div>
      </header>

      <div class="layout">
        <section class="status-panel">
          <div class="status-card" :class="statusClass">
            <div>
              <h3>{{ statusTitle }}</h3>
              <p>{{ statusDescription }}</p>
            </div>
            <strong v-if="openShift">{{ formatTime(openShift.checkInTime) }}</strong>
          </div>

          <div class="summary-card">
            <div><span>Ca hoan tat hom nay</span><strong>{{ todaySummary.totalCompletedShiftsToday }}</strong></div>
            <div><span>Phut lam viec</span><strong>{{ todaySummary.totalWorkedMinutesToday }}</strong></div>
            <div><span>Phut OT</span><strong>{{ todaySummary.totalOvertimeMinutesToday }}</strong></div>
          </div>

          <div class="action-grid">
            <button class="btn-primary" :disabled="!!openShift" @click="handleCheckIn">Vao ca</button>
            <button class="btn-secondary" :disabled="!canStartBreak" @click="handleStartBreak">Bat dau nghi</button>
            <button class="btn-secondary" :disabled="!canEndBreak" @click="handleEndBreak">Ket thuc nghi</button>
            <button class="btn-danger" :disabled="!openShift" @click="handleCheckOut">Tan ca</button>
          </div>
        </section>

        <section class="history-panel">
          <h2>Lich su cham cong</h2>
          <table class="history-table">
            <thead>
              <tr>
                <th>Ngay</th>
                <th>Check in</th>
                <th>Check out</th>
                <th>Phut lam</th>
                <th>Phut nghi</th>
                <th>Trang thai</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="record in history" :key="record.attendanceId">
                <td>{{ formatDate(record.checkInTime) }}</td>
                <td>{{ formatTime(record.checkInTime) }}</td>
                <td>{{ record.checkOutTime ? formatTime(record.checkOutTime) : '--' }}</td>
                <td>{{ record.workedMinutes }}</td>
                <td>{{ record.breakMinutes }}</td>
                <td><span :class="['status-tag', mapStatusClass(record.status)]">{{ record.status }}</span></td>
              </tr>
              <tr v-if="history.length === 0">
                <td colspan="6" class="empty-msg">Chua co du lieu cham cong.</td>
              </tr>
            </tbody>
          </table>
        </section>
      </div>

      <footer class="footer">
        <button class="btn-secondary" @click="goBack">Quay lai ban hang</button>
      </footer>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';
import attendanceApi from '../api/attendance';

const router = useRouter();
const currentTime = ref('');
const currentDate = ref('');
const openShift = ref(null);
const history = ref([]);
const todaySummary = ref({
  currentShift: null,
  totalWorkedMinutesToday: 0,
  totalOvertimeMinutesToday: 0,
  totalCompletedShiftsToday: 0
});

const hasActiveBreak = computed(() => !!openShift.value?.breaks?.some((item) => item.status === 'Active'));
const canStartBreak = computed(() => !!openShift.value && !hasActiveBreak.value);
const canEndBreak = computed(() => !!openShift.value && hasActiveBreak.value);

const statusTitle = computed(() => {
  if (!openShift.value) return 'Chua vao ca';
  if (hasActiveBreak.value) return 'Dang nghi giua ca';
  return 'Dang trong ca lam';
});

const statusDescription = computed(() => {
  if (!openShift.value) return 'Vui long vao ca de bat dau lam viec.';
  if (hasActiveBreak.value) return 'Ca lam dang tam dung trong luc nghi.';
  return 'Nhan vien dang hoat dong tren he thong cham cong.';
});

const statusClass = computed(() => {
  if (!openShift.value) return 'idle';
  return hasActiveBreak.value ? 'break' : 'working';
});

const tickClock = () => {
  const now = new Date();
  currentTime.value = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  currentDate.value = now.toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
};

const reloadAll = async () => {
  try {
    const [historyData, todayData, openData] = await Promise.all([
      attendanceApi.getMyHistory(),
      attendanceApi.getMyToday(),
      attendanceApi.getOpenShift()
    ]);

    history.value = historyData || [];
    todaySummary.value = todayData || todaySummary.value;
    openShift.value = openData || null;
  } catch (error) {
    console.error('Failed to load timekeeping data:', error);
    Swal.fire('Loi', error?.message || 'Khong the tai du lieu cham cong.', 'error');
  }
};

const askForNote = async (title, confirmButtonText) => {
  const result = await Swal.fire({
    title,
    input: 'textarea',
    inputPlaceholder: 'Nhap ghi chu neu can...',
    showCancelButton: true,
    confirmButtonText,
    cancelButtonText: 'Huy',
    confirmButtonColor: '#2c1810'
  });

  return result.isConfirmed ? (result.value || null) : undefined;
};

const handleCheckIn = async () => {
  const note = await askForNote('Xac nhan vao ca', 'Vao ca');
  if (note === undefined) return;

  try {
    await attendanceApi.checkIn({ source: 'Web', notes: note });
    await reloadAll();
    Swal.fire('Thanh cong', 'Da vao ca thanh cong.', 'success');
  } catch (error) {
    Swal.fire('Loi', error?.message || 'Khong the vao ca.', 'error');
  }
};

const handleStartBreak = async () => {
  const note = await askForNote('Bat dau nghi giua ca', 'Bat dau nghi');
  if (note === undefined) return;

  try {
    await attendanceApi.startBreak({ note });
    await reloadAll();
    Swal.fire('Thanh cong', 'Da bat dau nghi giua ca.', 'success');
  } catch (error) {
    Swal.fire('Loi', error?.message || 'Khong the bat dau nghi.', 'error');
  }
};

const handleEndBreak = async () => {
  const note = await askForNote('Ket thuc nghi giua ca', 'Ket thuc');
  if (note === undefined) return;

  try {
    await attendanceApi.endBreak({ note });
    await reloadAll();
    Swal.fire('Thanh cong', 'Da ket thuc nghi giua ca.', 'success');
  } catch (error) {
    Swal.fire('Loi', error?.message || 'Khong the ket thuc nghi.', 'error');
  }
};

const handleCheckOut = async () => {
  const note = await askForNote('Xac nhan tan ca', 'Tan ca');
  if (note === undefined) return;

  try {
    await attendanceApi.checkOut({ source: 'Web', notes: note });
    await reloadAll();
    Swal.fire('Thanh cong', 'Da tan ca thanh cong.', 'success');
  } catch (error) {
    Swal.fire('Loi', error?.message || 'Khong the tan ca.', 'error');
  }
};

const goBack = () => router.push('/cashier');

const formatTime = (value) => new Date(value).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
const formatDate = (value) => new Date(value).toLocaleDateString('vi-VN');
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

let timerId;

onMounted(() => {
  tickClock();
  timerId = setInterval(tickClock, 1000);
  reloadAll();
});

onUnmounted(() => {
  clearInterval(timerId);
});
</script>

<style scoped>
.timekeeping-page { min-height: 100vh; background: #f5f0eb; padding: 2rem; display: flex; justify-content: center; }
.glass-card { background: rgba(255,255,255,.82); backdrop-filter: blur(20px); border: 1px solid rgba(255,255,255,.55); border-radius: 1.5rem; box-shadow: 0 18px 40px rgba(0,0,0,.06); }
.timekeeping-card { width: min(1100px, 95vw); padding: 2rem; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
.page-header h1 { color: var(--primary); font-size: 2rem; margin-bottom: .3rem; }
.page-header p { color: #6b7280; }
.clock { font-size: 3rem; color: var(--secondary); font-weight: 800; }
.layout { display: grid; grid-template-columns: 320px 1fr; gap: 1.5rem; }
.status-panel { display: flex; flex-direction: column; gap: 1rem; }
.status-card, .summary-card { background: white; border-radius: 1.2rem; padding: 1.2rem; border: 1px solid #eee; }
.status-card.idle { border-left: 6px solid #cbd5e1; }
.status-card.working { border-left: 6px solid #16a34a; }
.status-card.break { border-left: 6px solid #f59e0b; }
.status-card h3 { margin-bottom: .35rem; }
.status-card p { color: #6b7280; }
.summary-card { display: flex; flex-direction: column; gap: .75rem; }
.summary-card div { display: flex; justify-content: space-between; }
.action-grid { display: grid; gap: .75rem; }
.btn-primary, .btn-secondary, .btn-danger { border: none; border-radius: 1rem; padding: .95rem 1rem; font-weight: 700; cursor: pointer; }
.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.btn-danger { background: #fee2e2; color: #b91c1c; }
.btn-primary:disabled, .btn-secondary:disabled, .btn-danger:disabled { opacity: .55; cursor: not-allowed; }
.history-panel { background: white; border-radius: 1.2rem; padding: 1.2rem; border: 1px solid #eee; }
.history-panel h2 { margin-bottom: 1rem; }
.history-table { width: 100%; border-collapse: collapse; }
.history-table th, .history-table td { padding: .9rem; text-align: left; border-bottom: 1px solid #f3f4f6; }
.history-table th { font-size: .8rem; text-transform: uppercase; color: #6b7280; }
.status-tag { display: inline-block; padding: .35rem .7rem; border-radius: 999px; font-size: .78rem; font-weight: 700; }
.status-tag.working { background: #dcfce7; color: #166534; }
.status-tag.break { background: #fef3c7; color: #92400e; }
.status-tag.done { background: #dbeafe; color: #1d4ed8; }
.status-tag.adjusted { background: #ede9fe; color: #6d28d9; }
.status-tag.warning { background: #fee2e2; color: #b91c1c; }
.status-tag.neutral { background: #f3f4f6; color: #4b5563; }
.empty-msg { text-align: center; color: #9ca3af; }
.footer { margin-top: 1.5rem; display: flex; justify-content: flex-start; }
@media (max-width: 960px) { .layout { grid-template-columns: 1fr; } .clock { font-size: 2.2rem; } .page-header { flex-direction: column; align-items: flex-start; gap: .75rem; } }
</style>
