<template>
  <div class="manage-products">
    <div class="page-header slide-up">
      <div class="title-section">
        <h2>Quản Lý Sản Phẩm</h2>
        <p>Danh sách các món trong menu của quán.</p>
      </div>
      <button @click="openAddModal" class="btn btn-primary shadow-hover">
         <Plus :size="20"/>
         Thêm Món Mới
      </button>
    </div>

    <!-- Stats -->
    <div class="stats-grid slide-up" style="animation-delay: 0.1s">
       <div class="stat-card glass-card">
          <div class="stat-icon"><Coffee :size="24"/></div>
          <div class="stat-info">
             <h4>Tổng số món</h4>
             <p class="count">{{ products.length }}</p>
          </div>
       </div>
       <div class="stat-card glass-card">
          <div class="stat-icon"><Tag :size="24"/></div>
          <div class="stat-info">
             <h4>Danh mục</h4>
             <p class="count">{{ categories.length }}</p>
          </div>
       </div>
       <div class="stat-card glass-card">
          <div class="stat-icon"><Package :size="24"/></div>
          <div class="stat-info">
             <h4>Đang mở bán</h4>
             <p class="count">{{ products.filter(p => p.isActive).length }}</p>
          </div>
       </div>
    </div>

    <!-- Table -->
    <div class="table-container glass-card slide-up" style="animation-delay: 0.2s">
      <div class="table-actions">
         <div class="search-box">
            <Search :size="18"/>
            <input v-model="searchQuery" type="text" placeholder="Tìm tên món, mã món...">
         </div>
         <div class="filter-box">
            <Filter :size="18"/>
            <select v-model="filterCategory">
               <option value="">Tất cả danh mục</option>
               <option v-for="cat in categories" :key="cat.categoryId" :value="cat.categoryId">{{ cat.name }}</option>
            </select>
         </div>
      </div>

      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Hình Ảnh</th>
            <th>Tên Sản Phẩm</th>
            <th>Danh Mục</th>
            <th>Giá Bán</th>
            <th>Trạng Thái</th>
            <th>Hành Động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="product in filteredProducts" :key="product.productId">
            <td>#{{ product.productId }}</td>
            <td>
              <img :src="product.imageUrl || 'https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&q=80&w=150'" class="product-thumb" alt="">
            </td>
            <td>
              <div class="name-cell">
                <strong>{{ product.name }}</strong>
              </div>
            </td>
            <td><span class="category-badge">{{ product.categoryName }}</span></td>
            <td><strong>{{ formatPrice(product.price) }}</strong></td>
            <td>
              <span class="status-badge" :class="product.isActive ? 'available' : 'unavailable'">
                {{ product.isActive ? 'Đang bán' : 'Tạm dừng' }}
              </span>
            </td>
            <td class="actions-cell">
               <button class="icon-btn edit" @click="editProduct(product)"><Pencil :size="18"/></button>
               <button class="icon-btn delete" @click="deleteProduct(product)"><Trash2 :size="18"/></button>
            </td>
          </tr>
        </tbody>
      </table>
      
      <div v-if="loading" class="table-loading">
         <div class="spinner"></div>
         Đang tải dữ liệu...
      </div>
      <div v-if="!loading && filteredProducts.length === 0" class="empty-state">
         Không tìm thấy sản phẩm nào!
      </div>
    </div>

    <!-- Product Modal -->
    <div v-if="showModal" class="modal-overlay">
      <div class="modal-content glass-card slide-up-fast">
        <div class="modal-header">
          <h3>{{ isEditing ? 'Cập Nhật Sản Phẩm' : 'Thêm Sản Phẩm Mới' }}</h3>
          <button class="icon-btn close" @click="closeModal"><X :size="20"/></button>
        </div>
        
        <form @submit.prevent="saveProduct" class="modal-body">
          <div class="form-group">
            <label>Tên Sản Phẩm <span class="required">*</span></label>
            <input type="text" v-model="productForm.name" required placeholder="Nhập tên món ăn/thức uống..." />
          </div>
          
          <div class="form-row">
            <div class="form-group w-50">
              <label>Danh Mục <span class="required">*</span></label>
              <select v-model="productForm.categoryId" required>
                <option value="" disabled>--- Chọn danh mục ---</option>
                <option v-for="cat in categories" :key="cat.categoryId" :value="cat.categoryId">{{ cat.name }}</option>
              </select>
            </div>
            <div class="form-group w-50">
              <label>Giá Bán (VNĐ) <span class="required">*</span></label>
              <input type="number" v-model="productForm.price" min="0" required placeholder="VD: 35000" />
            </div>
          </div>

          <div class="form-group">
            <label>Link Hình Ảnh (URL)</label>
            <input type="url" v-model="productForm.imageUrl" placeholder="https://..." />
            <div v-if="productForm.imageUrl" class="image-preview">
              <img :src="productForm.imageUrl" alt="Preview" @error="handleImageError" />
            </div>
          </div>

          <!-- Topping Selection -->
          <div class="form-group toppings-group">
            <label>Topping có thể thêm</label>
            <div class="toppings-list glass-card">
              <div v-for="t in allToppings" :key="t.toppingId" class="topping-item">
                <input 
                  type="checkbox" 
                  :id="'topping-' + t.toppingId" 
                  :value="t.toppingId"
                  v-model="productForm.toppingIds"
                >
                <label :for="'topping-' + t.toppingId">
                  {{ t.toppingName }} <span>(+{{ formatPrice(t.price) }})</span>
                </label>
              </div>
              <div v-if="allToppings.length === 0" class="text-mini-muted">
                Chưa có topping nào được tạo trong hệ thống.
              </div>
            </div>
          </div>

          <div v-if="isEditing" class="form-group switch-group">
            <label>Trạng Thái Bán</label>
            <label class="switch">
              <input type="checkbox" v-model="productForm.isActive">
              <span class="slider round"></span>
            </label>
            <span class="status-text" :class="{ 'active': productForm.isActive }">
              {{ productForm.isActive ? 'Đang kích hoạt' : 'Đang tạm dừng' }}
            </span>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeModal" :disabled="saving">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              <span v-if="saving" class="spinner-small"></span>
              <span v-else>{{ isEditing ? 'Lưu Thay Đổi' : 'Tạo Sản Phẩm' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { Plus, Coffee, Tag, Package, Search, Filter, Pencil, Trash2, X } from 'lucide-vue-next';
import api from '../api';
import Swal from 'sweetalert2';

const products = ref([]);
const categories = ref([]);
const allToppings = ref([]);
const loading = ref(false);
const saving = ref(false);

const searchQuery = ref('');
const filterCategory = ref('');

// Modal state
const showModal = ref(false);
const isEditing = ref(false);
const editingProductId = ref(null);

const productForm = ref({
  name: '',
  categoryId: '',
  price: 0,
  imageUrl: '',
  isActive: true,
  toppingIds: []
});

const filteredProducts = computed(() => {
  return products.value.filter(p => {
    const term = searchQuery.value.toLowerCase();
    const matchesSearch = p.name.toLowerCase().includes(term) || p.productId.toString().includes(term);
    const matchesCategory = filterCategory.value === '' || p.categoryId == filterCategory.value;
    return matchesSearch && matchesCategory;
  });
});

const loadInitialData = async () => {
  loading.value = true;
  try {
    const [productsData, categoriesData, toppingsData] = await Promise.all([
      api.get('/Products'),
      api.get('/Categories'),
      api.get('/Lookups/toppings')
    ]);
    products.value = productsData || [];
    categories.value = categoriesData || [];
    allToppings.value = toppingsData?.filter(t => t.isActive) || [];
  } catch (error) {
    Swal.fire('Lỗi', 'Không thể lấy dữ liệu sản phẩm hoặc topping.', 'error');
  } finally {
    loading.value = false;
  }
};

onMounted(loadInitialData);

const formatPrice = (price) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
};

const handleImageError = (e) => {
  e.target.src = 'https://via.placeholder.com/300?text=Invalid+Image';
};

const resetForm = () => {
  productForm.value = {
    name: '',
    categoryId: categories.value.length > 0 ? categories.value[0].categoryId : '',
    price: 0,
    imageUrl: '',
    isActive: true,
    toppingIds: []
  };
};

const openAddModal = () => {
  resetForm();
  isEditing.value = false;
  editingProductId.value = null;
  showModal.value = true;
};

const editProduct = (product) => {
  productForm.value = {
    name: product.name,
    categoryId: product.categoryId,
    price: product.price,
    imageUrl: product.imageUrl || '',
    isActive: product.isActive,
    toppingIds: product.toppingIds || []
  };
  isEditing.value = true;
  editingProductId.value = product.productId;
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const saveProduct = async () => {
  saving.value = true;
  try {
    const payload = {
      name: productForm.value.name,
      categoryId: productForm.value.categoryId,
      price: productForm.value.price,
      imageUrl: productForm.value.imageUrl,
      ...(isEditing.value ? { isActive: productForm.value.isActive } : {})
    };
    
    if (isEditing.value) {
      await api.put(`/Products/${editingProductId.value}`, payload);
      Swal.fire({
        icon: 'success',
        title: 'Cập nhật thành công',
        showConfirmButton: false,
        timer: 1500
      });
    } else {
      await api.post('/Products', payload);
      Swal.fire({
        icon: 'success',
        title: 'Thêm mới thành công',
        showConfirmButton: false,
        timer: 1500
      });
    }
    
    closeModal();
    loadInitialData(); // Reload to get updated list & standard IDs
  } catch (error) {
    Swal.fire('Lỗi', error.response?.data?.error || 'Có lỗi xảy ra khi lưu sản phẩm.', 'error');
  } finally {
    saving.value = false;
  }
};

const deleteProduct = async (product) => {
  const result = await Swal.fire({
    title: 'Xóa sản phẩm?',
    text: `Bạn có chắc muốn xóa "${product.name}" (#${product.productId})? Thao tác này có thể ảnh hưởng đến dữ liệu báo cáo cũ.`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#d33',
    cancelButtonColor: '#2C1810',
    confirmButtonText: 'Xóa vĩnh viễn',
    cancelButtonText: 'Hủy bỏ'
  });

  if (result.isConfirmed) {
    try {
      await api.delete(`/Products/${product.productId}`);
      products.value = products.value.filter(p => p.productId !== product.productId);
      Swal.fire('Đã xóa!', 'Sản phẩm đã biến mất khỏi hệ thống.', 'success');
    } catch (error) {
      Swal.fire('Thất bại', 'Không thể xóa sản phẩm do có phụ thuộc dữ liệu.', 'error');
    }
  }
};
</script>

<style scoped>
.manage-products {
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
  background: rgba(212, 163, 115, 0.15); /* light secondary */
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

.search-box, .filter-box {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: #fff;
  border: 1px solid #e2e8f0;
  padding: 0.6rem 1rem;
  border-radius: 0.75rem;
  width: 320px;
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);
}

.search-box input, .filter-box select {
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

.product-thumb {
  width: 50px;
  height: 50px;
  border-radius: 0.5rem;
  object-fit: cover;
  box-shadow: 0 4px 6px rgba(0,0,0,0.1);
}

.name-cell {
  display: flex;
  flex-direction: column;
}

.category-badge {
  background: #f1f5f9;
  color: #475569;
  padding: 0.3rem 0.75rem;
  border-radius: 0.5rem;
  font-size: 0.8rem;
  font-weight: 600;
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
  max-width: 600px;
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

.form-row {
  display: flex;
  gap: 1.5rem;
}
.w-50 { flex: 1; }

label {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--primary);
}
.required { color: #ef4444; }

input[type="text"], input[type="number"], input[type="url"], select {
  padding: 0.75rem 1rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.75rem;
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.2s;
  background: #f8fafc;
}
input[type="text"]:focus, input[type="number"]:focus, input[type="url"]:focus, select:focus {
  border-color: var(--secondary);
  background: #fff;
}

.image-preview {
  margin-top: 1rem;
  width: 100px;
  height: 100px;
  border-radius: 0.75rem;
  overflow: hidden;
  border: 1px solid #e2e8f0;
}
.image-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* Toppings Selection List */
.toppings-group {
  margin-top: 1rem;
}

.toppings-list {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
  padding: 1rem;
  max-height: 180px;
  overflow-y: auto;
  background: rgba(248, 250, 252, 0.5);
  border: 1px solid #e2e8f0;
}

.topping-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
}

.topping-item input {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.topping-item label {
  cursor: pointer;
  color: #475569;
}

.topping-item label span {
  color: var(--secondary);
  font-weight: 700;
  font-size: 0.8rem;
}

.text-mini-muted {
  grid-column: span 2;
  text-align: center;
  color: #94a3b8;
  font-size: 0.85rem;
  padding: 1rem 0;
}

.switch-group {
  flex-direction: row;
  align-items: center;
  gap: 1rem;
}

/* Switch UI */
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

/* Utils */
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
