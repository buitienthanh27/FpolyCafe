<template>
  <div class="manage-page">
    <div class="page-header">
      <div>
        <h2>Quản lý nhập kho</h2>
        <p>Tạo phiếu nhập, cộng tồn kho tự động và theo dõi lịch sử nhập hàng.</p>
      </div>
      <button class="btn btn-primary" @click="openModal">
        <Plus :size="18" />
        Tạo phiếu nhập
      </button>
    </div>

    <div class="stats-grid">
      <div class="stat-card">
        <h4>Tổng phiếu nhập</h4>
        <strong>{{ summary.totalReceipts }}</strong>
      </div>
      <div class="stat-card">
        <h4>Tổng chi phí</h4>
        <strong>{{ formatCurrency(summary.totalSpend) }}</strong>
      </div>
      <div class="stat-card">
        <h4>Nhà cung cấp</h4>
        <strong>{{ summary.distinctSuppliers }}</strong>
      </div>
      <div class="stat-card">
        <h4>Dòng nhập</h4>
        <strong>{{ summary.importedItems }}</strong>
      </div>
    </div>

    <div class="table-card">
      <div class="toolbar">
        <div class="search-box">
          <Search :size="18" />
          <input v-model="filters.keyword" type="text" placeholder="Tìm theo nhà cung cấp hoặc người nhập..." @keyup.enter="loadData" />
        </div>
        <input v-model="filters.from" class="date-input" type="date" @change="loadData" />
        <input v-model="filters.to" class="date-input" type="date" @change="loadData" />
        <button class="btn btn-secondary" @click="loadData">Tải lại</button>
      </div>

      <div v-if="loading" class="state-box">
        <LoaderCircle class="spin" :size="28" />
        <span>Đang tải phiếu nhập...</span>
      </div>

      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Mã phiếu</th>
            <th>Nhà cung cấp</th>
            <th>Người nhập</th>
            <th>Ngày nhập</th>
            <th>Tổng tiền</th>
            <th>Chi tiết</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="receipt in receipts" :key="receipt.receiptId">
            <td>{{ receipt.receiptCode }}</td>
            <td>
              <strong>{{ receipt.supplierName }}</strong>
              <div class="sub-text">{{ receipt.supplierAddress || 'Chưa có địa chỉ' }}</div>
            </td>
            <td>{{ receipt.createdBy }}</td>
            <td>{{ formatDateTime(receipt.createdAt) }}</td>
            <td>{{ formatCurrency(receipt.totalCost) }}</td>
            <td>
              <button class="btn btn-secondary btn-sm" @click="showDetails(receipt)">Xem</button>
            </td>
          </tr>
          <tr v-if="receipts.length === 0">
            <td colspan="6" class="empty-row">Chưa có phiếu nhập nào.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-card large">
        <div class="modal-header">
          <h3>Tạo phiếu nhập kho</h3>
          <button class="icon-btn" @click="closeModal">
            <X :size="16" />
          </button>
        </div>

        <form class="modal-body" @submit.prevent="submitReceipt">
          <div class="form-row">
            <div class="form-group">
              <label>Nhà cung cấp</label>
              <input v-model="form.supplierName" type="text" required />
            </div>
            <div class="form-group">
              <label>Địa chỉ</label>
              <input v-model="form.supplierAddress" type="text" />
            </div>
          </div>

          <div class="form-group">
            <label>Ghi chú</label>
            <textarea v-model="form.notes" rows="2" />
          </div>

          <div class="section-title">Danh sách nguyên liệu</div>

          <div v-for="(item, index) in form.items" :key="index" class="line-item">
            <select v-model.number="item.ingredientId" required class="select-input">
              <option :value="null">Chọn nguyên liệu</option>
              <option v-for="ingredient in ingredientOptions" :key="ingredient.ingredientId" :value="ingredient.ingredientId">
                {{ ingredient.ingredientName }} ({{ ingredient.unit }})
              </option>
            </select>
            <input v-model.number="item.quantity" type="number" min="0.01" step="0.01" placeholder="Số lượng" required />
            <input v-model.number="item.unitPrice" type="number" min="0" step="0.01" placeholder="Đơn giá" required />
            <button type="button" class="icon-btn delete" @click="removeItem(index)" :disabled="form.items.length === 1">
              <Trash2 :size="16" />
            </button>
          </div>

          <button type="button" class="btn btn-secondary btn-sm" @click="addItem">
            <Plus :size="16" />
            Thêm dòng
          </button>

          <div class="summary-line">
            <span>Tổng phiếu nhập</span>
            <strong>{{ formatCurrency(totalCost) }}</strong>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeModal">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              {{ saving ? 'Đang lưu...' : 'Lưu phiếu nhập' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue';
import { LoaderCircle, Plus, Search, Trash2, X } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import ingredientApi from '../api/ingredient';
import inventoryReceiptApi from '../api/inventoryReceipt';

const loading = ref(false);
const saving = ref(false);
const showModal = ref(false);
const receipts = ref([]);
const ingredientOptions = ref([]);
const summary = ref({
  totalReceipts: 0,
  totalSpend: 0,
  distinctSuppliers: 0,
  importedItems: 0
});

const filters = reactive({
  keyword: '',
  from: '',
  to: ''
});

const form = reactive({
  supplierName: '',
  supplierAddress: '',
  notes: '',
  items: [{ ingredientId: null, quantity: 1, unitPrice: 0 }]
});

const totalCost = computed(() =>
  form.items.reduce((sum, item) => sum + (Number(item.quantity) || 0) * (Number(item.unitPrice) || 0), 0)
);

const resetForm = () => {
  form.supplierName = '';
  form.supplierAddress = '';
  form.notes = '';
  form.items = [{ ingredientId: null, quantity: 1, unitPrice: 0 }];
};

const loadData = async () => {
  loading.value = true;
  try {
    const params = {
      ...(filters.keyword.trim() ? { keyword: filters.keyword.trim() } : {}),
      ...(filters.from ? { from: filters.from } : {}),
      ...(filters.to ? { to: filters.to } : {})
    };

    const [receiptData, summaryData, ingredientData] = await Promise.all([
      inventoryReceiptApi.getAll(params),
      inventoryReceiptApi.getSummary(params),
      ingredientApi.getAll()
    ]);

    receipts.value = receiptData || [];
    summary.value = summaryData || summary.value;
    ingredientOptions.value = ingredientData || [];
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể tải dữ liệu nhập kho.', 'error');
  } finally {
    loading.value = false;
  }
};

const openModal = () => {
  resetForm();
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const addItem = () => {
  form.items.push({ ingredientId: null, quantity: 1, unitPrice: 0 });
};

const removeItem = (index) => {
  if (form.items.length === 1) return;
  form.items.splice(index, 1);
};

const submitReceipt = async () => {
  saving.value = true;
  try {
    const selectedIds = form.items.map((item) => item.ingredientId).filter(Boolean);
    if (new Set(selectedIds).size !== selectedIds.length) {
      throw new Error('Mỗi nguyên liệu chỉ được chọn một lần trong phiếu nhập.');
    }

    await inventoryReceiptApi.create({
      supplierName: form.supplierName,
      supplierAddress: form.supplierAddress || null,
      notes: form.notes || null,
      items: form.items.map((item) => ({
        ingredientId: item.ingredientId,
        quantity: item.quantity,
        unitPrice: item.unitPrice
      }))
    });

    closeModal();
    await loadData();
    Swal.fire('Thành công', 'Đã tạo phiếu nhập kho.', 'success');
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể tạo phiếu nhập.', 'error');
  } finally {
    saving.value = false;
  }
};

const showDetails = (receipt) => {
  const html = receipt.details?.length
    ? receipt.details
        .map(
          (item) =>
            `<div style="display:flex;justify-content:space-between;gap:12px;margin-bottom:8px;"><span>${item.ingredientName} x ${item.quantity}</span><strong>${formatCurrency(item.lineTotal)}</strong></div>`
        )
        .join('')
    : '<div>Không có chi tiết.</div>';

  Swal.fire({
    title: receipt.receiptCode,
    html,
    width: 640,
    confirmButtonText: 'Đóng'
  });
};

const formatCurrency = (value) =>
  Number(value || 0).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });

const formatDateTime = (value) =>
  new Date(value).toLocaleString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });

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
.stat-card strong { color: var(--primary); font-size: 1.5rem; }
.table-card { padding: 1.25rem; }
.toolbar { display: flex; gap: .75rem; align-items: center; margin-bottom: 1rem; }
.search-box { flex: 1; display: flex; align-items: center; gap: .6rem; background: white; border: 1px solid #e5e7eb; border-radius: .9rem; padding: .75rem 1rem; }
.search-box input, .date-input, .select-input, .form-group input, .form-group textarea { width: 100%; border: none; outline: none; background: transparent; }
.date-input, .select-input, .line-item input { border: 1px solid #d1d5db; border-radius: .9rem; padding: .75rem 1rem; background: white; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th, .data-table td { padding: .9rem; text-align: left; border-bottom: 1px solid #f3f4f6; vertical-align: top; }
.data-table th { color: #6b7280; font-size: .8rem; text-transform: uppercase; }
.sub-text { color: #6b7280; font-size: .84rem; margin-top: .2rem; }
.state-box, .empty-row { text-align: center; color: #6b7280; padding: 1.25rem; }
.spin { animation: spin 1s linear infinite; }
.btn { border: none; border-radius: .85rem; padding: .8rem 1rem; cursor: pointer; font-weight: 700; display: inline-flex; align-items: center; gap: .5rem; }
.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.btn-sm { padding: .6rem .8rem; font-size: .88rem; }
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.35); display: flex; align-items: center; justify-content: center; z-index: 1000; backdrop-filter: blur(4px); }
.modal-card { width: min(760px, 94vw); padding: 1.5rem; }
.modal-card.large { max-height: 90vh; overflow: auto; }
.modal-header, .modal-footer { display: flex; justify-content: space-between; align-items: center; }
.modal-body { display: flex; flex-direction: column; gap: 1rem; margin-top: 1rem; }
.form-group { display: flex; flex-direction: column; gap: .45rem; }
.form-group input, .form-group textarea { border: 1px solid #d1d5db; border-radius: .85rem; padding: .8rem .95rem; background: white; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: .75rem; }
.section-title { font-weight: 700; color: var(--primary); margin-top: .5rem; }
.line-item { display: grid; grid-template-columns: 1.6fr 1fr 1fr auto; gap: .75rem; align-items: center; }
.summary-line { display: flex; justify-content: space-between; align-items: center; padding-top: .75rem; border-top: 1px dashed #d1d5db; font-size: 1rem; }
.summary-line strong { color: var(--primary); font-size: 1.2rem; }
.icon-btn { border: none; background: transparent; width: 36px; height: 36px; border-radius: .75rem; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; }
.icon-btn.delete { background: #fee2e2; color: #b91c1c; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 960px) {
  .stats-grid { grid-template-columns: repeat(2, 1fr); }
  .toolbar, .page-header { flex-direction: column; align-items: stretch; }
  .form-row, .line-item { grid-template-columns: 1fr; }
}
</style>
