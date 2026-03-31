<template>
  <div class="manage-customers">
    <div class="page-header slide-up">
      <div class="title-section">
        <h2>Quan ly khach hang</h2>
        <p>Them, tim kiem va cap nhat thong tin khach hang dang duoc dong bo voi backend FpolyCafe.</p>
      </div>
      <button @click="openCreateModal" class="btn btn-primary">
        <Plus :size="18" />
        Them khach hang
      </button>
    </div>

    <div class="stats-grid slide-up" style="animation-delay: 0.1s">
      <div class="stat-card glass-card">
        <h4>Tong khach hang</h4>
        <strong>{{ customers.length }}</strong>
      </div>
      <div class="stat-card glass-card">
        <h4>Diem trung binh</h4>
        <strong>{{ averagePoints }}</strong>
      </div>
      <div class="stat-card glass-card">
        <h4>Khach moi</h4>
        <strong>{{ recentCustomers }}</strong>
      </div>
    </div>

    <div class="table-container glass-card slide-up" style="animation-delay: 0.2s">
      <div class="table-actions">
        <div class="search-box">
          <Search :size="18" />
          <input v-model="searchQuery" type="text" placeholder="Tim theo ten hoac so dien thoai..." @keyup.enter="loadCustomers" />
        </div>
        <button class="btn btn-secondary" @click="loadCustomers">Tai lai</button>
      </div>

      <div v-if="loading" class="state-box">
        <Loader2 class="spin" :size="34" />
        <p>Dang tai danh sach khach hang...</p>
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Khach hang</th>
            <th>So dien thoai</th>
            <th>Diem</th>
            <th>Ngay tao</th>
            <th>Hanh dong</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="customer in customers" :key="customer.customerId">
            <td>#{{ customer.customerId }}</td>
            <td>{{ customer.fullName }}</td>
            <td>{{ customer.phoneNumber }}</td>
            <td>{{ customer.rewardPoints }}</td>
            <td>{{ formatDate(customer.createdAt) }}</td>
            <td class="actions-cell">
              <button class="icon-btn edit" @click="openEditModal(customer)">
                <Pencil :size="18" />
              </button>
            </td>
          </tr>
          <tr v-if="customers.length === 0">
            <td colspan="6" class="empty-msg">Chua co khach hang nao phu hop.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content glass-card">
        <div class="modal-header">
          <h3>{{ isEditing ? 'Cap nhat khach hang' : 'Them khach hang moi' }}</h3>
          <button class="icon-btn close" @click="closeModal">
            <X :size="18" />
          </button>
        </div>

        <form class="modal-body" @submit.prevent="handleSubmit">
          <div class="form-group">
            <label>Ho va ten</label>
            <input v-model="form.fullName" type="text" required placeholder="Nguyen Van A" />
          </div>

          <div class="form-group">
            <label>So dien thoai</label>
            <input v-model="form.phoneNumber" type="text" required placeholder="09xxxxxxxx" />
          </div>

          <div v-if="isEditing" class="form-group">
            <label>Diem tich luy</label>
            <input v-model.number="form.rewardPoints" type="number" min="0" />
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeModal">Huy</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Dang luu...' : isEditing ? 'Luu thay doi' : 'Tao khach hang' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';
import { Loader2, Pencil, Plus, Search, X } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import customerApi from '../api/customer';

const customers = ref([]);
const searchQuery = ref('');
const loading = ref(false);
const saving = ref(false);
const showModal = ref(false);
const isEditing = ref(false);
const editingId = ref(null);

const form = ref({
  fullName: '',
  phoneNumber: '',
  rewardPoints: 0
});

const averagePoints = computed(() => {
  if (customers.value.length === 0) return 0;
  const total = customers.value.reduce((sum, customer) => sum + customer.rewardPoints, 0);
  return Math.round(total / customers.value.length);
});

const recentCustomers = computed(() => {
  const now = Date.now();
  return customers.value.filter((customer) => now - new Date(customer.createdAt).getTime() <= 7 * 24 * 60 * 60 * 1000).length;
});

const resetForm = () => {
  form.value = {
    fullName: '',
    phoneNumber: '',
    rewardPoints: 0
  };
};

const loadCustomers = async () => {
  loading.value = true;
  try {
    const keyword = searchQuery.value.trim();
    customers.value = await customerApi.getAll(keyword ? { keyword } : {});
  } catch (error) {
    console.error('Failed to load customers:', error);
    Swal.fire('Loi', error?.message || 'Khong the tai danh sach khach hang.', 'error');
  } finally {
    loading.value = false;
  }
};

const openCreateModal = () => {
  resetForm();
  isEditing.value = false;
  editingId.value = null;
  showModal.value = true;
};

const openEditModal = (customer) => {
  form.value = {
    fullName: customer.fullName,
    phoneNumber: customer.phoneNumber,
    rewardPoints: customer.rewardPoints
  };
  isEditing.value = true;
  editingId.value = customer.customerId;
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const handleSubmit = async () => {
  saving.value = true;
  try {
    if (isEditing.value) {
      await customerApi.update(editingId.value, form.value);
    } else {
      await customerApi.create({
        fullName: form.value.fullName,
        phoneNumber: form.value.phoneNumber
      });
    }

    closeModal();
    await loadCustomers();
    Swal.fire('Thanh cong', isEditing.value ? 'Da cap nhat khach hang.' : 'Da tao khach hang moi.', 'success');
  } catch (error) {
    console.error('Failed to save customer:', error);
    Swal.fire('Loi', error?.message || 'Khong the luu khach hang.', 'error');
  } finally {
    saving.value = false;
  }
};

const formatDate = (value) =>
  new Date(value).toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' });

onMounted(loadCustomers);
</script>

<style scoped>
.manage-customers { display: flex; flex-direction: column; gap: 2rem; }
.glass-card { background: rgba(255,255,255,.75); backdrop-filter: blur(16px); border: 1px solid rgba(255,255,255,.4); box-shadow: 0 8px 32px rgba(0,0,0,.05); border-radius: 1.25rem; }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; }
.title-section h2 { font-size: 2rem; color: var(--primary); }
.title-section p { color: #777; margin-top: .25rem; max-width: 720px; }
.btn { display: flex; align-items: center; gap: .5rem; padding: .75rem 1.2rem; border-radius: .75rem; border: none; font-weight: 700; cursor: pointer; }
.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.stats-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem; }
.stat-card { padding: 1.25rem; }
.stat-card h4 { color: #777; margin-bottom: .5rem; font-size: .9rem; }
.stat-card strong { color: var(--primary); font-size: 1.8rem; }
.table-container { padding: 1.5rem; }
.table-actions { display: flex; justify-content: space-between; gap: 1rem; margin-bottom: 1.25rem; }
.search-box { display: flex; align-items: center; gap: .75rem; background: white; border: 1px solid #e5e7eb; padding: .75rem 1rem; border-radius: .9rem; flex: 1; }
.search-box input { border: none; outline: none; width: 100%; background: transparent; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th, .data-table td { padding: 1rem; text-align: left; border-bottom: 1px solid #f3f4f6; }
.data-table th { font-size: .8rem; color: #6b7280; text-transform: uppercase; }
.actions-cell { width: 80px; }
.icon-btn { width: 36px; height: 36px; border: none; border-radius: .75rem; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; }
.icon-btn.edit { background: #fef3c7; color: #92400e; }
.icon-btn.close { background: transparent; color: #6b7280; }
.state-box, .empty-msg { text-align: center; color: #6b7280; padding: 2rem; }
.spin { animation: rotate 1.2s linear infinite; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.35); display: flex; align-items: center; justify-content: center; backdrop-filter: blur(4px); z-index: 1000; }
.modal-content { width: min(520px, 92vw); padding: 1.75rem; }
.modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.modal-body { display: flex; flex-direction: column; gap: 1rem; }
.form-group { display: flex; flex-direction: column; gap: .5rem; }
.form-group input { border: 1px solid #d1d5db; border-radius: .85rem; padding: .85rem 1rem; }
.modal-footer { display: flex; justify-content: flex-end; gap: .75rem; margin-top: .5rem; }
@keyframes rotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
</style>
