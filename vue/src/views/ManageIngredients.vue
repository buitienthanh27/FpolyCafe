<template>
  <div class="manage-page">
    <div class="page-header">
      <div>
        <h2>Quản lý kho nguyên liệu</h2>
        <p>Theo dõi tồn kho, cập nhật định mức tối thiểu và giá nhập gần nhất.</p>
      </div>
      <button class="btn btn-primary" @click="openCreateModal">
        <Plus :size="18" />
        Thêm nguyên liệu
      </button>
    </div>

    <div class="stats-grid">
      <div class="stat-card">
        <h4>Tổng nguyên liệu</h4>
        <strong>{{ summary.totalIngredients }}</strong>
      </div>
      <div class="stat-card warning">
        <h4>Sắp hết hàng</h4>
        <strong>{{ summary.lowStockIngredients }}</strong>
      </div>
      <div class="stat-card danger">
        <h4>Hết hàng</h4>
        <strong>{{ summary.outOfStockIngredients }}</strong>
      </div>
      <div class="stat-card">
        <h4>Giá trị tồn kho</h4>
        <strong>{{ formatCurrency(summary.totalInventoryValue) }}</strong>
      </div>
    </div>

    <div class="table-card">
      <div class="toolbar">
        <div class="search-box">
          <Search :size="18" />
          <input v-model="filters.keyword" type="text" placeholder="Tìm theo tên hoặc đơn vị..." @keyup.enter="loadData" />
        </div>
        <select v-model="filters.status" class="select-input" @change="loadData">
          <option value="">Tất cả trạng thái</option>
          <option value="in_stock">Còn hàng</option>
          <option value="low_stock">Sắp hết</option>
          <option value="out_of_stock">Hết hàng</option>
        </select>
        <button class="btn btn-secondary" @click="loadData">Tải lại</button>
      </div>

      <div v-if="loading" class="state-box">
        <LoaderCircle class="spin" :size="28" />
        <span>Đang tải dữ liệu kho...</span>
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Tên nguyên liệu</th>
            <th>Đơn vị</th>
            <th>Tồn kho</th>
            <th>Ngưỡng tối thiểu</th>
            <th>Giá nhập gần nhất</th>
            <th>Trạng thái</th>
            <th>Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in ingredients" :key="item.ingredientId">
            <td>{{ item.ingredientName }}</td>
            <td>{{ item.unit }}</td>
            <td>{{ formatNumber(item.stockQuantity) }}</td>
            <td>{{ formatNumber(item.minimumStockLevel) }}</td>
            <td>{{ formatCurrency(item.lastUnitCost) }}</td>
            <td>
              <span :class="['badge', item.stockStatus]">{{ stockStatusLabel(item.stockStatus) }}</span>
            </td>
            <td class="actions">
              <button class="icon-btn edit" @click="openEditModal(item)">
                <Pencil :size="16" />
              </button>
              <button class="icon-btn delete" @click="removeIngredient(item)">
                <Trash2 :size="16" />
              </button>
            </td>
          </tr>
          <tr v-if="ingredients.length === 0">
            <td colspan="7" class="empty-row">Chưa có nguyên liệu phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>{{ editingId ? 'Cập nhật nguyên liệu' : 'Thêm nguyên liệu mới' }}</h3>
          <button class="icon-btn" @click="closeModal">
            <X :size="16" />
          </button>
        </div>

        <form class="modal-body" @submit.prevent="submitForm">
          <div class="form-group">
            <label>Tên nguyên liệu</label>
            <input v-model="form.ingredientName" type="text" required />
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Đơn vị</label>
              <input v-model="form.unit" type="text" required placeholder="gram, ml, chai..." />
            </div>
            <div class="form-group">
              <label>Tồn kho hiện tại</label>
              <input v-model.number="form.stockQuantity" type="number" min="0" step="0.01" required />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Ngưỡng tối thiểu</label>
              <input v-model.number="form.minimumStockLevel" type="number" min="0" step="0.01" required />
            </div>
            <div class="form-group">
              <label>Giá nhập gần nhất</label>
              <input v-model.number="form.lastUnitCost" type="number" min="0" step="0.01" required />
            </div>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeModal">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Đang lưu...' : editingId ? 'Lưu thay đổi' : 'Tạo nguyên liệu' }}
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
import ingredientApi from '../api/ingredient';

const loading = ref(false);
const saving = ref(false);
const showModal = ref(false);
const editingId = ref(null);
const ingredients = ref([]);
const summary = ref({
  totalIngredients: 0,
  lowStockIngredients: 0,
  outOfStockIngredients: 0,
  totalInventoryValue: 0
});

const filters = reactive({
  keyword: '',
  status: ''
});

const form = reactive({
  ingredientName: '',
  unit: '',
  stockQuantity: 0,
  minimumStockLevel: 0,
  lastUnitCost: 0
});

const resetForm = () => {
  form.ingredientName = '';
  form.unit = '';
  form.stockQuantity = 0;
  form.minimumStockLevel = 0;
  form.lastUnitCost = 0;
};

const loadData = async () => {
  loading.value = true;
  try {
    const [ingredientData, summaryData] = await Promise.all([
      ingredientApi.getAll({
        ...(filters.keyword.trim() ? { keyword: filters.keyword.trim() } : {}),
        ...(filters.status ? { status: filters.status } : {})
      }),
      ingredientApi.getSummary()
    ]);

    ingredients.value = ingredientData || [];
    summary.value = summaryData || summary.value;
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể tải dữ liệu nguyên liệu.', 'error');
  } finally {
    loading.value = false;
  }
};

const openCreateModal = () => {
  resetForm();
  editingId.value = null;
  showModal.value = true;
};

const openEditModal = (item) => {
  editingId.value = item.ingredientId;
  form.ingredientName = item.ingredientName;
  form.unit = item.unit;
  form.stockQuantity = item.stockQuantity;
  form.minimumStockLevel = item.minimumStockLevel;
  form.lastUnitCost = item.lastUnitCost;
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
      ingredientName: form.ingredientName,
      unit: form.unit,
      stockQuantity: form.stockQuantity,
      minimumStockLevel: form.minimumStockLevel,
      lastUnitCost: form.lastUnitCost
    };

    if (editingId.value) {
      await ingredientApi.update(editingId.value, payload);
    } else {
      await ingredientApi.create(payload);
    }

    closeModal();
    await loadData();
    Swal.fire('Thành công', editingId.value ? 'Đã cập nhật nguyên liệu.' : 'Đã thêm nguyên liệu mới.', 'success');
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể lưu nguyên liệu.', 'error');
  } finally {
    saving.value = false;
  }
};

const removeIngredient = async (item) => {
  const result = await Swal.fire({
    title: 'Xóa nguyên liệu?',
    text: `Bạn có chắc muốn xóa "${item.ingredientName}" không?`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Xóa',
    cancelButtonText: 'Hủy'
  });

  if (!result.isConfirmed) return;

  try {
    await ingredientApi.remove(item.ingredientId);
    await loadData();
    Swal.fire('Thành công', 'Đã xóa nguyên liệu.', 'success');
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể xóa nguyên liệu.', 'error');
  }
};

const stockStatusLabel = (status) => {
  if (status === 'in_stock') return 'Còn hàng';
  if (status === 'low_stock') return 'Sắp hết';
  return 'Hết hàng';
};

const formatCurrency = (value) =>
  Number(value || 0).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });

const formatNumber = (value) => Number(value || 0).toLocaleString('vi-VN');

onMounted(loadData);
</script>

<style scoped>
.manage-page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; }
.page-header h2 { color: var(--primary); font-size: 2rem; }
.page-header p { color: #6b7280; margin-top: .25rem; }
.stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1rem; }
.stat-card, .table-card, .modal-card { background: rgba(255,255,255,.82); border: 1px solid rgba(255,255,255,.5); border-radius: 1.25rem; box-shadow: 0 12px 32px rgba(0,0,0,.05); backdrop-filter: blur(16px); }
.stat-card { padding: 1.2rem; }
.stat-card h4 { color: #6b7280; font-size: .9rem; margin-bottom: .4rem; }
.stat-card strong { color: var(--primary); font-size: 1.6rem; }
.stat-card.warning strong { color: #b45309; }
.stat-card.danger strong { color: #b91c1c; }
.table-card { padding: 1.25rem; }
.toolbar { display: flex; gap: .75rem; align-items: center; margin-bottom: 1rem; }
.search-box { flex: 1; display: flex; align-items: center; gap: .6rem; background: white; border: 1px solid #e5e7eb; border-radius: .9rem; padding: .75rem 1rem; }
.search-box input, .select-input, .form-group input { width: 100%; border: none; outline: none; background: transparent; }
.select-input { max-width: 220px; border: 1px solid #d1d5db; border-radius: .9rem; padding: .75rem 1rem; background: white; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th, .data-table td { padding: .9rem; text-align: left; border-bottom: 1px solid #f3f4f6; }
.data-table th { color: #6b7280; font-size: .8rem; text-transform: uppercase; }
.badge { display: inline-flex; padding: .3rem .7rem; border-radius: 999px; font-size: .78rem; font-weight: 700; }
.badge.in_stock { background: #dcfce7; color: #166534; }
.badge.low_stock { background: #fef3c7; color: #92400e; }
.badge.out_of_stock { background: #fee2e2; color: #b91c1c; }
.actions { display: flex; gap: .5rem; }
.icon-btn { border: none; background: transparent; width: 36px; height: 36px; border-radius: .75rem; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; }
.icon-btn.edit { background: #fef3c7; color: #92400e; }
.icon-btn.delete { background: #fee2e2; color: #b91c1c; }
.state-box, .empty-row { text-align: center; color: #6b7280; padding: 1.25rem; }
.spin { animation: spin 1s linear infinite; }
.btn { border: none; border-radius: .85rem; padding: .8rem 1rem; cursor: pointer; font-weight: 700; display: inline-flex; align-items: center; gap: .5rem; }
.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.35); display: flex; align-items: center; justify-content: center; z-index: 1000; backdrop-filter: blur(4px); }
.modal-card { width: min(560px, 92vw); padding: 1.5rem; }
.modal-header, .modal-footer { display: flex; justify-content: space-between; align-items: center; }
.modal-body { display: flex; flex-direction: column; gap: 1rem; margin-top: 1rem; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: .75rem; }
.form-group { display: flex; flex-direction: column; gap: .45rem; }
.form-group input { border: 1px solid #d1d5db; border-radius: .85rem; padding: .8rem .95rem; background: white; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 960px) {
  .stats-grid { grid-template-columns: repeat(2, 1fr); }
  .toolbar, .page-header { flex-direction: column; align-items: stretch; }
  .form-row { grid-template-columns: 1fr; }
}
</style>
