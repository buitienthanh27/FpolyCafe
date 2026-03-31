<template>
  <div class="manage-page">
    <div class="page-header">
      <div>
        <h2>Quản lý khuyến mãi</h2>
        <p>Tạo và quản lý mã giảm giá dùng chung cho hệ thống.</p>
      </div>
      <button class="btn btn-primary" @click="openCreateModal">
        <Plus :size="18" />
        Tạo khuyến mãi
      </button>
    </div>

    <div class="table-card">
      <div class="toolbar">
        <div class="search-box">
          <Search :size="18" />
          <input v-model="filters.keyword" type="text" placeholder="Tìm theo mã hoặc mô tả..." @keyup.enter="loadData" />
        </div>
        <select v-model="filters.status" class="select-input" @change="loadData">
          <option value="">Tất cả trạng thái</option>
          <option value="active">Đang hoạt động</option>
          <option value="scheduled">Sắp diễn ra</option>
          <option value="expired">Hết hạn</option>
          <option value="inactive">Tạm ngừng</option>
        </select>
        <button class="btn btn-secondary" @click="loadData">Tải lại</button>
      </div>

      <div v-if="loading" class="state-box">
        <LoaderCircle class="spin" :size="28" />
        <span>Đang tải danh sách khuyến mãi...</span>
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Mã</th>
            <th>Mô tả</th>
            <th>Loại</th>
            <th>Giá trị</th>
            <th>Đơn tối thiểu</th>
            <th>Hiệu lực</th>
            <th>Trạng thái</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="promotion in promotions" :key="promotion.promotionId">
            <td><strong>{{ promotion.code }}</strong></td>
            <td>{{ promotion.description }}</td>
            <td>{{ promotion.discountType === 'Percentage' ? 'Phần trăm' : 'Tiền mặt' }}</td>
            <td>{{ formatDiscount(promotion) }}</td>
            <td>{{ promotion.minimumOrderAmount ? formatCurrency(promotion.minimumOrderAmount) : 'Không yêu cầu' }}</td>
            <td>{{ formatDate(promotion.startsAt) }} - {{ formatDate(promotion.endsAt) }}</td>
            <td>
              <span :class="['badge', promotion.status]">{{ statusLabel(promotion.status) }}</span>
            </td>
            <td class="actions">
              <button class="icon-btn edit" @click="openEditModal(promotion)">
                <Pencil :size="16" />
              </button>
              <button class="icon-btn delete" @click="removePromotion(promotion)">
                <Trash2 :size="16" />
              </button>
            </td>
          </tr>
          <tr v-if="promotions.length === 0">
            <td colspan="8" class="empty-row">Chưa có khuyến mãi phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>{{ editingId ? 'Cập nhật khuyến mãi' : 'Tạo khuyến mãi mới' }}</h3>
          <button class="icon-btn" @click="closeModal">
            <X :size="16" />
          </button>
        </div>

        <form class="modal-body" @submit.prevent="submitForm">
          <div class="form-row">
            <div class="form-group">
              <label>Mã khuyến mãi</label>
              <input v-model="form.code" type="text" required />
            </div>
            <div class="form-group">
              <label>Loại giảm giá</label>
              <select v-model="form.discountType" class="select-input">
                <option value="Percentage">Phần trăm</option>
                <option value="FixedAmount">Tiền mặt</option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label>Mô tả</label>
            <input v-model="form.description" type="text" required />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Giá trị</label>
              <input v-model.number="form.discountValue" type="number" min="0.01" step="0.01" required />
            </div>
            <div class="form-group">
              <label>Đơn tối thiểu</label>
              <input v-model.number="form.minimumOrderAmount" type="number" min="0" step="0.01" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Bắt đầu</label>
              <input v-model="form.startsAt" type="datetime-local" required />
            </div>
            <div class="form-group">
              <label>Kết thúc</label>
              <input v-model="form.endsAt" type="datetime-local" required />
            </div>
          </div>

          <label class="checkbox-line">
            <input v-model="form.isActive" type="checkbox" />
            Kích hoạt ngay sau khi lưu
          </label>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeModal">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Đang lưu...' : editingId ? 'Lưu thay đổi' : 'Tạo khuyến mãi' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue';
import { LoaderCircle, Pencil, Plus, Search, Trash2, X } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import promotionApi from '../api/promotion';

const loading = ref(false);
const saving = ref(false);
const showModal = ref(false);
const editingId = ref(null);
const promotions = ref([]);

const filters = reactive({
  keyword: '',
  status: ''
});

const form = reactive({
  code: '',
  description: '',
  discountType: 'Percentage',
  discountValue: 0,
  minimumOrderAmount: null,
  startsAt: '',
  endsAt: '',
  isActive: true
});

const resetForm = () => {
  form.code = '';
  form.description = '';
  form.discountType = 'Percentage';
  form.discountValue = 0;
  form.minimumOrderAmount = null;
  form.startsAt = '';
  form.endsAt = '';
  form.isActive = true;
};

const toLocalInput = (value) => {
  const date = new Date(value);
  const pad = (input) => String(input).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
};

const loadData = async () => {
  loading.value = true;
  try {
    promotions.value = await promotionApi.getAll({
      ...(filters.keyword.trim() ? { keyword: filters.keyword.trim() } : {}),
      ...(filters.status ? { status: filters.status } : {})
    });
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể tải danh sách khuyến mãi.', 'error');
  } finally {
    loading.value = false;
  }
};

const openCreateModal = () => {
  resetForm();
  editingId.value = null;
  showModal.value = true;
};

const openEditModal = (promotion) => {
  editingId.value = promotion.promotionId;
  form.code = promotion.code;
  form.description = promotion.description;
  form.discountType = promotion.discountType;
  form.discountValue = promotion.discountValue;
  form.minimumOrderAmount = promotion.minimumOrderAmount;
  form.startsAt = toLocalInput(promotion.startsAt);
  form.endsAt = toLocalInput(promotion.endsAt);
  form.isActive = promotion.isActive;
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
  editingId.value = null;
};

const submitForm = async () => {
  saving.value = true;
  try {
    const payload = {
      code: form.code,
      description: form.description,
      discountType: form.discountType,
      discountValue: form.discountValue,
      minimumOrderAmount: form.minimumOrderAmount || null,
      startsAt: new Date(form.startsAt).toISOString(),
      endsAt: new Date(form.endsAt).toISOString(),
      isActive: form.isActive
    };

    if (editingId.value) {
      await promotionApi.update(editingId.value, payload);
    } else {
      await promotionApi.create(payload);
    }

    closeModal();
    await loadData();
    Swal.fire('Thành công', editingId.value ? 'Đã cập nhật khuyến mãi.' : 'Đã tạo khuyến mãi mới.', 'success');
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể lưu khuyến mãi.', 'error');
  } finally {
    saving.value = false;
  }
};

const removePromotion = async (promotion) => {
  const result = await Swal.fire({
    title: 'Xóa khuyến mãi?',
    text: `Bạn có chắc muốn xóa mã "${promotion.code}" không?`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Xóa',
    cancelButtonText: 'Hủy'
  });

  if (!result.isConfirmed) return;

  try {
    await promotionApi.remove(promotion.promotionId);
    await loadData();
    Swal.fire('Thành công', 'Đã xóa khuyến mãi.', 'success');
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể xóa khuyến mãi.', 'error');
  }
};

const formatDate = (value) => new Date(value).toLocaleDateString('vi-VN');
const formatCurrency = (value) => Number(value || 0).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
const formatDiscount = (promotion) => promotion.discountType === 'Percentage' ? `${promotion.discountValue}%` : formatCurrency(promotion.discountValue);
const statusLabel = (status) => {
  if (status === 'active') return 'Đang hoạt động';
  if (status === 'scheduled') return 'Sắp diễn ra';
  if (status === 'expired') return 'Hết hạn';
  return 'Tạm ngừng';
};

onMounted(loadData);
</script>

<style scoped>
.manage-page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; }
.page-header h2 { color: var(--primary); font-size: 2rem; }
.page-header p { color: #6b7280; margin-top: .25rem; }
.table-card, .modal-card { background: rgba(255,255,255,.82); border: 1px solid rgba(255,255,255,.5); border-radius: 1.25rem; box-shadow: 0 12px 32px rgba(0,0,0,.05); backdrop-filter: blur(16px); }
.table-card { padding: 1.25rem; }
.toolbar { display: flex; gap: .75rem; align-items: center; margin-bottom: 1rem; }
.search-box { flex: 1; display: flex; align-items: center; gap: .6rem; background: white; border: 1px solid #e5e7eb; border-radius: .9rem; padding: .75rem 1rem; }
.search-box input, .select-input, .form-group input { width: 100%; border: none; outline: none; background: transparent; }
.select-input, .form-group input { border: 1px solid #d1d5db; border-radius: .85rem; padding: .8rem .95rem; background: white; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th, .data-table td { padding: .9rem; text-align: left; border-bottom: 1px solid #f3f4f6; }
.data-table th { color: #6b7280; font-size: .8rem; text-transform: uppercase; }
.badge { display: inline-flex; padding: .3rem .7rem; border-radius: 999px; font-size: .78rem; font-weight: 700; }
.badge.active { background: #dcfce7; color: #166534; }
.badge.scheduled { background: #dbeafe; color: #1d4ed8; }
.badge.expired { background: #f3f4f6; color: #4b5563; }
.badge.inactive { background: #fee2e2; color: #b91c1c; }
.actions { display: flex; gap: .5rem; }
.state-box, .empty-row { text-align: center; color: #6b7280; padding: 1.25rem; }
.btn { border: none; border-radius: .85rem; padding: .8rem 1rem; cursor: pointer; font-weight: 700; display: inline-flex; align-items: center; gap: .5rem; }
.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.icon-btn { border: none; background: transparent; width: 36px; height: 36px; border-radius: .75rem; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; }
.icon-btn.edit { background: #fef3c7; color: #92400e; }
.icon-btn.delete { background: #fee2e2; color: #b91c1c; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.35); display: flex; align-items: center; justify-content: center; z-index: 1000; backdrop-filter: blur(4px); }
.modal-card { width: min(620px, 92vw); padding: 1.5rem; }
.modal-header, .modal-footer { display: flex; justify-content: space-between; align-items: center; }
.modal-body { display: flex; flex-direction: column; gap: 1rem; margin-top: 1rem; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: .75rem; }
.form-group { display: flex; flex-direction: column; gap: .45rem; }
.checkbox-line { display: flex; align-items: center; gap: .5rem; color: #374151; }
.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 960px) {
  .toolbar, .page-header { flex-direction: column; align-items: stretch; }
  .form-row { grid-template-columns: 1fr; }
}
</style>
