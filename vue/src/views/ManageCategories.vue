<template>
  <div class="manage-categories">
    <!-- Header -->
    <div class="page-header slide-up">
      <div class="title-section">
        <h2>Quản Lý Danh Mục</h2>
        <p>Phân loại thức uống và các món ăn trong quán.</p>
      </div>
      <button @click="openAddModal" class="btn btn-primary shadow-hover">
         <Plus :size="20"/>
         Thêm Danh Mục Mới
      </button>
    </div>

    <!-- Stats -->
    <div class="stats-grid slide-up" style="animation-delay: 0.1s">
       <div class="stat-card glass-card">
          <div class="stat-icon"><Tag :size="24"/></div>
          <div class="stat-info">
             <h4>Tổng số danh mục</h4>
             <p class="count">{{ categories.length }}</p>
          </div>
       </div>
       <div class="stat-card glass-card">
          <div class="stat-icon"><CheckCircle :size="24"/></div>
          <div class="stat-info">
             <h4>Đang hoạt động</h4>
             <p class="count">{{ categories.filter(c => c.isActive).length }}</p>
          </div>
       </div>
       <div class="stat-card glass-card">
          <div class="stat-icon"><XCircle :size="24"/></div>
          <div class="stat-info">
             <h4>Tạm khóa</h4>
             <p class="count">{{ categories.filter(c => !c.isActive).length }}</p>
          </div>
       </div>
    </div>

    <!-- Table -->
    <div class="table-container glass-card slide-up" style="animation-delay: 0.2s">
      <div class="table-actions">
         <div class="search-box">
            <Search :size="18"/>
            <input v-model="searchQuery" type="text" placeholder="Tìm tên danh mục...">
         </div>
      </div>

      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Tên Danh Mục</th>
            <th>Mô Tả</th>
            <th>Trạng Thái</th>
            <th>Hành Động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="category in filteredCategories" :key="category.categoryId">
            <td>#{{ category.categoryId }}</td>
            <td>
              <div class="name-cell">
                <strong>{{ category.name }}</strong>
              </div>
            </td>
            <td>
              <span class="description-text">{{ category.description || '(Không có mô tả)' }}</span>
            </td>
            <td>
              <span class="status-badge" :class="category.isActive ? 'available' : 'unavailable'">
                {{ category.isActive ? 'Hoạt động' : 'Tạm dừng' }}
              </span>
            </td>
            <td class="actions-cell">
               <button class="icon-btn edit" @click="editCategory(category)"><Pencil :size="18"/></button>
               <button class="icon-btn delete" @click="deleteCategory(category)"><Trash2 :size="18"/></button>
            </td>
          </tr>
        </tbody>
      </table>
      
      <div v-if="loading" class="table-loading">
         <div class="spinner"></div>
         Đang tải dữ liệu...
      </div>
      <div v-if="!loading && filteredCategories.length === 0" class="empty-state">
         Không tìm thấy danh mục nào!
      </div>
    </div>

    <!-- Category Modal -->
    <div v-if="showModal" class="modal-overlay">
      <div class="modal-content glass-card slide-up-fast">
        <div class="modal-header">
          <h3>{{ isEditing ? 'Cập Nhật Danh Mục' : 'Thêm Danh Mục Mới' }}</h3>
          <button class="icon-btn close" @click="closeModal"><X :size="20"/></button>
        </div>
        
        <form @submit.prevent="saveCategory" class="modal-body">
          <div class="form-group">
            <label>Tên Danh Mục <span class="required">*</span></label>
            <input type="text" v-model="categoryForm.name" required placeholder="VD: Cà Phê, Trà Sữa..." />
          </div>
          
          <div class="form-group">
            <label>Mô Tả</label>
            <textarea v-model="categoryForm.description" rows="3" placeholder="Nhập mô tả ngắn gọn về danh mục này..."></textarea>
          </div>

          <div v-if="isEditing" class="form-group switch-group">
            <label>Trạng Thái Hoạt Động</label>
            <label class="switch">
              <input type="checkbox" v-model="categoryForm.isActive">
              <span class="slider round"></span>
            </label>
            <span class="status-text" :class="{ 'active': categoryForm.isActive }">
              {{ categoryForm.isActive ? 'Đang mở' : 'Đang khóa' }}
            </span>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeModal" :disabled="saving">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              <span v-if="saving" class="spinner-small"></span>
              <span v-else>{{ isEditing ? 'Lưu Thay Đổi' : 'Tạo Danh Mục' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { Plus, Tag, CheckCircle, XCircle, Search, Pencil, Trash2, X } from 'lucide-vue-next';
import api from '../api';
import Swal from 'sweetalert2';

const categories = ref([]);
const loading = ref(false);
const saving = ref(false);
const searchQuery = ref('');

// Modal state
const showModal = ref(false);
const isEditing = ref(false);
const editingCategoryId = ref(null);

const categoryForm = ref({
  name: '',
  description: '',
  isActive: true
});

const filteredCategories = computed(() => {
  return categories.value.filter(c => {
    const term = searchQuery.value.toLowerCase();
    return c.name.toLowerCase().includes(term) || c.categoryId.toString().includes(term);
  });
});

const loadInitialData = async () => {
  loading.value = true;
  try {
    const data = await api.get('/Categories');
    categories.value = data || [];
  } catch (error) {
    Swal.fire('Lỗi', 'Không thể lấy dữ liệu danh mục.', 'error');
  } finally {
    loading.value = false;
  }
};

onMounted(loadInitialData);

const resetForm = () => {
  categoryForm.value = {
    name: '',
    description: '',
    isActive: true
  };
};

const openAddModal = () => {
  resetForm();
  isEditing.value = false;
  editingCategoryId.value = null;
  showModal.value = true;
};

const editCategory = (category) => {
  categoryForm.value = {
    name: category.name,
    description: category.description || '',
    isActive: category.isActive
  };
  isEditing.value = true;
  editingCategoryId.value = category.categoryId;
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const saveCategory = async () => {
  saving.value = true;
  try {
    const payload = { ...categoryForm.value };
    
    if (isEditing.value) {
      await api.put(`/Categories/${editingCategoryId.value}`, payload);
      Swal.fire({
        icon: 'success',
        title: 'Cập nhật thành công',
        showConfirmButton: false,
        timer: 1500
      });
    } else {
      await api.post('/Categories', payload);
      Swal.fire({
        icon: 'success',
        title: 'Thêm mới thành công',
        showConfirmButton: false,
        timer: 1500
      });
    }
    
    closeModal();
    loadInitialData(); // Reload 
  } catch (error) {
    Swal.fire('Lỗi', 'Có lỗi xảy ra khi lưu danh mục.', 'error');
  } finally {
    saving.value = false;
  }
};

const deleteCategory = async (category) => {
  const result = await Swal.fire({
    title: 'Xác nhận xóa?',
    text: `Bạn có chắc muốn xóa vĩnh viễn danh mục "${category.name}"? Cảnh báo: Việc xóa danh mục sẽ làm mất hoặc lỗi các sản phẩm nằm trong danh mục này!`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#d33',
    cancelButtonColor: '#2C1810',
    confirmButtonText: 'Xóa vĩnh viễn',
    cancelButtonText: 'Hủy bỏ'
  });

  if (result.isConfirmed) {
    try {
      await api.delete(`/Categories/${category.categoryId}`);
      categories.value = categories.value.filter(c => c.categoryId !== category.categoryId);
      Swal.fire('Đã xóa!', 'Danh mục đã biến mất khỏi hệ thống.', 'success');
    } catch (error) {
      Swal.fire('Thất bại', 'Không thể xóa danh mục này (có thể có sản phẩm rang buộc).', 'error');
    }
  }
};
</script>

<style scoped>
.manage-categories {
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
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.title-section h2 { font-size: 2rem; color: var(--primary); }
.title-section p { color: #777; margin-top: 0.25rem; }

.btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-primary {
  background: var(--primary);
  color: white;
}
.btn-primary:hover:not(:disabled) {
  background: #1a0f0a;
}
.btn-secondary {
  background: #e2e8f0;
  color: #475569;
}
.btn-secondary:hover:not(:disabled) {
  background: #cbd5e1;
}

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  padding: 1.5rem;
}

.stat-icon {
  width: 50px;
  height: 50px;
  background: rgba(212, 163, 115, 0.15);
  border-radius: 0.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--secondary);
}

.stat-info h4 { font-size: 0.9rem; color: #777; }
.count { font-size: 1.7rem; font-weight: 700; color: var(--primary); }

.table-container {
  padding: 1.5rem;
  overflow: hidden;
}

.table-actions {
  display: flex;
  justify-content: space-between;
  margin-bottom: 2rem;
}

.search-box {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: #fff;
  border: 1px solid #e2e8f0;
  padding: 0.6rem 1rem;
  border-radius: 0.75rem;
  width: 350px;
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);
}

.search-box input {
  border: none;
  background: none;
  width: 100%;
  outline: none;
  font-size: 0.95rem;
  color: var(--text-main);
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th {
  text-align: left;
  padding: 1rem;
  font-weight: 700;
  color: var(--primary);
  border-bottom: 2px solid #f1f5f9;
  font-family: 'Outfit';
}

.data-table td {
  padding: 1rem;
  border-bottom: 1px solid #f8fafc;
  vertical-align: middle;
}

.name-cell strong {
  font-size: 1.05rem;
  color: var(--primary);
}

.description-text {
  color: #64748b;
  font-size: 0.9rem;
}

.status-badge {
   padding: 0.3rem 0.75rem;
   border-radius: 0.5rem;
   font-size: 0.8rem;
   font-weight: 600;
}

.status-badge.available { background: #dcfce7; color: #166534; }
.status-badge.unavailable { background: #fee2e2; color: #991b1b; }

.actions-cell {
  display: flex;
  gap: 0.5rem;
}

.icon-btn {
  width: 36px;
  height: 36px;
  border-radius: 0.5rem;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: 0.2s;
  background: transparent;
}

.icon-btn.edit { background: #fef3c7; color: #b45309; }
.icon-btn.delete { background: #fee2e2; color: #991b1b; }
.icon-btn.close { color: #64748b; }

.icon-btn:hover { transform: translateY(-2px); filter: brightness(0.95); }

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  z-index: 999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.modal-content {
  width: 100%;
  max-width: 550px;
  padding: 2rem;
  background: rgba(255, 255, 255, 0.95);
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.modal-header h3 {
  font-size: 1.5rem;
  color: var(--primary);
  font-family: 'Outfit';
}

.form-group {
  margin-bottom: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

label {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--primary);
}
.required { color: #ef4444; }

input[type="text"], textarea {
  padding: 0.75rem 1rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.75rem;
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.2s;
  background: #f8fafc;
  font-family: 'Inter', sans-serif;
}
input[type="text"]:focus, textarea:focus {
  border-color: var(--secondary);
  background: #fff;
}

.switch-group {
  flex-direction: row;
  align-items: center;
  gap: 1rem;
}

.switch {
  position: relative;
  display: inline-block;
  width: 46px;
  height: 24px;
}
.switch input { opacity: 0; width: 0; height: 0; }
.slider {
  position: absolute; cursor: pointer;
  top: 0; left: 0; right: 0; bottom: 0;
  background-color: #cbd5e1;
  transition: .4s;
}
.slider:before {
  position: absolute; content: "";
  height: 18px; width: 18px;
  left: 3px; bottom: 3px;
  background-color: white;
  transition: .4s;
}
input:checked + .slider {
  background-color: var(--secondary);
}
input:checked + .slider:before {
  transform: translateX(22px);
}
.slider.round { border-radius: 24px; }
.slider.round:before { border-radius: 50%; }

.status-text { font-weight: 500; font-size: 0.9rem; color: #64748b; }
.status-text.active { color: var(--secondary); }

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 2rem;
  border-top: 1px solid #e2e8f0;
  padding-top: 1.5rem;
}

.slide-up { animation: slideUp 0.6s cubic-bezier(0.23, 1, 0.32, 1) forwards; }
.slide-up-fast { animation: slideUp 0.3s cubic-bezier(0.23, 1, 0.32, 1) forwards; }

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.table-loading, .empty-state {
  padding: 4rem;
  text-align: center;
  color: #999;
}

.spinner {
  width: 30px;
  height: 30px;
  border: 3px solid #eee;
  border-top-color: var(--primary);
  border-radius: 50%;
  animation: spin 1s infinite linear;
  margin: 0 auto 1rem;
}
.spinner-small {
  width: 18px; height: 18px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 1s infinite linear;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
