<template>
  <div class="manage-users">
    <div class="page-header">
      <div class="title-section">
        <h2>Quản Lý Người Dùng</h2>
        <p>Thêm tài khoản và phân quyền cho nhân sự sử dụng hệ thống.</p>
      </div>
      <button class="btn btn-primary" @click="openAddModal">
        <Plus :size="18" />
        Tạo tài khoản
      </button>
    </div>

    <div class="stats-grid">
      <div class="stat-card glass-card">
        <div class="stat-icon"><Users :size="24" /></div>
        <div>
          <h4>Tổng tài khoản</h4>
          <p class="count">{{ users.length }}</p>
        </div>
      </div>
      <div class="stat-card glass-card">
        <div class="stat-icon"><ShieldCheck :size="24" /></div>
        <div>
          <h4>Quản trị / Quản lý</h4>
          <p class="count">{{ users.filter((item) => item.role === 'Admin' || item.role === 'Manager').length }}</p>
        </div>
      </div>
      <div class="stat-card glass-card">
        <div class="stat-icon"><LockKeyhole :size="24" /></div>
        <div>
          <h4>Tạm khóa</h4>
          <p class="count">{{ users.filter((item) => !item.isActive).length }}</p>
        </div>
      </div>
    </div>

    <div class="table-container glass-card">
      <div class="table-actions">
        <div class="search-box">
          <Search :size="18" />
          <input
            v-model.trim="searchQuery"
            type="text"
            placeholder="Tìm theo tên đăng nhập hoặc họ tên..."
          />
        </div>
      </div>

      <div v-if="loading" class="table-state">
        <div class="spinner"></div>
        Đang tải dữ liệu người dùng...
      </div>

      <div v-else-if="forbidden" class="table-state">
        Bạn không có quyền truy cập màn này. Hãy đăng nhập bằng tài khoản `Admin` hoặc `Manager`.
      </div>

      <template v-else>
        <table class="data-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Họ tên</th>
              <th>Tên đăng nhập</th>
              <th>Vai trò</th>
              <th>Trạng thái</th>
              <th>Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in filteredUsers" :key="user.userId">
              <td>#{{ user.userId }}</td>
              <td>
                <div class="name-cell">
                  <strong>{{ user.fullName }}</strong>
                  <span class="email-text">{{ user.email || 'Chưa cập nhật email' }}</span>
                </div>
              </td>
              <td>
                <span class="username-pill">@{{ user.username }}</span>
              </td>
              <td>
                <span class="role-badge" :class="roleClass(user.role)">
                  {{ roleLabel(user.role) }}
                </span>
              </td>
              <td>
                <span class="status-badge" :class="user.isActive ? 'available' : 'unavailable'">
                  {{ user.isActive ? 'Đang hoạt động' : 'Tạm khóa' }}
                </span>
              </td>
              <td class="actions-cell">
                <button class="icon-btn edit" title="Chỉnh sửa" @click="editUser(user)">
                  <Pencil :size="18" />
                </button>
                <button
                  v-if="user.isActive"
                  class="icon-btn delete"
                  title="Tạm khóa"
                  @click="lockUser(user)"
                >
                  <Lock :size="18" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <div v-if="!filteredUsers.length" class="table-state">
          Không tìm thấy tài khoản phù hợp.
        </div>
      </template>
    </div>

    <div v-if="showModal" class="modal-overlay">
      <div class="modal-content glass-card">
        <div class="modal-header">
          <h3>{{ isEditing ? 'Cập nhật người dùng' : 'Tạo tài khoản mới' }}</h3>
          <button class="icon-btn close" @click="closeModal">
            <X :size="18" />
          </button>
        </div>

        <form class="modal-body" @submit.prevent="saveUser">
          <div class="form-row" v-if="!isEditing">
            <div class="form-group">
              <label>Tên đăng nhập</label>
              <input v-model.trim="userForm.username" type="text" required />
            </div>
            <div class="form-group">
              <label>Mật khẩu tạm</label>
              <input v-model="userForm.password" type="password" required />
            </div>
          </div>

          <div v-if="isEditing" class="edit-warning">
            <Info :size="18" />
            Tên đăng nhập và mật khẩu không chỉnh sửa ở màn này.
          </div>

          <div class="form-group">
            <label>Họ và tên</label>
            <input v-model.trim="userForm.fullName" type="text" required />
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Email</label>
              <input v-model.trim="userForm.email" type="email" />
            </div>
            <div class="form-group">
              <label>Vai trò</label>
              <select v-model="userForm.role" required>
                <option value="Staff">Nhân viên</option>
                <option value="Bartender">Pha chế</option>
                <option value="Manager">Quản lý</option>
                <option value="Admin">Quản trị</option>
              </select>
            </div>
          </div>

          <div v-if="isEditing" class="switch-row">
            <label class="switch-label">Trạng thái tài khoản</label>
            <label class="switch">
              <input v-model="userForm.isActive" type="checkbox" />
              <span class="slider"></span>
            </label>
            <span class="switch-text">{{ userForm.isActive ? 'Đang hoạt động' : 'Tạm khóa' }}</span>
          </div>

          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" :disabled="saving" @click="closeModal">
              Hủy
            </button>
            <button type="submit" class="btn btn-primary" :disabled="saving">
              <span v-if="saving" class="spinner-small"></span>
              <span v-else>{{ isEditing ? 'Lưu thay đổi' : 'Tạo tài khoản' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';
import { Info, Lock, LockKeyhole, Pencil, Plus, Search, ShieldCheck, Users, X } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import api from '../api';

const users = ref([]);
const loading = ref(false);
const saving = ref(false);
const forbidden = ref(false);
const searchQuery = ref('');

const showModal = ref(false);
const isEditing = ref(false);
const editingUserId = ref(null);

const userForm = ref({
  username: '',
  password: '',
  fullName: '',
  email: '',
  role: 'Staff',
  isActive: true
});

const filteredUsers = computed(() => {
  const term = searchQuery.value.toLowerCase();
  return users.value.filter((item) => {
    return item.username.toLowerCase().includes(term) || item.fullName.toLowerCase().includes(term);
  });
});

const roleLabel = (role) => {
  if (role === 'Admin') return 'Quản trị';
  if (role === 'Manager') return 'Quản lý';
  if (role === 'Bartender') return 'Pha chế';
  return 'Nhân viên';
};

const roleClass = (role) => {
  if (role === 'Admin') return 'role-admin';
  if (role === 'Manager') return 'role-manager';
  if (role === 'Bartender') return 'role-bartender';
  return 'role-staff';
};

const resetForm = () => {
  userForm.value = {
    username: '',
    password: '',
    fullName: '',
    email: '',
    role: 'Staff',
    isActive: true
  };
};

const loadUsers = async () => {
  loading.value = true;
  forbidden.value = false;
  try {
    const data = await api.get('/Users');
    users.value = Array.isArray(data) ? data : [];
  } catch (error) {
    if (error?.status === 403) {
      forbidden.value = true;
      users.value = [];
      return;
    }

    Swal.fire('Lỗi', error?.message || 'Không thể lấy dữ liệu người dùng.', 'error');
  } finally {
    loading.value = false;
  }
};

const openAddModal = () => {
  resetForm();
  isEditing.value = false;
  editingUserId.value = null;
  showModal.value = true;
};

const editUser = (user) => {
  userForm.value = {
    username: user.username,
    password: '',
    fullName: user.fullName,
    email: user.email || '',
    role: user.role,
    isActive: user.isActive
  };
  isEditing.value = true;
  editingUserId.value = user.userId;
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const saveUser = async () => {
  saving.value = true;
  try {
    if (isEditing.value) {
      await api.put(`/Users/${editingUserId.value}`, {
        fullName: userForm.value.fullName,
        email: userForm.value.email,
        role: userForm.value.role,
        isActive: userForm.value.isActive
      });
    } else {
      await api.post('/Users', {
        username: userForm.value.username,
        password: userForm.value.password,
        fullName: userForm.value.fullName,
        email: userForm.value.email,
        role: userForm.value.role
      });
    }

    await Swal.fire({
      icon: 'success',
      title: isEditing.value ? 'Đã cập nhật người dùng' : 'Đã tạo tài khoản',
      timer: 1400,
      showConfirmButton: false
    });

    closeModal();
    await loadUsers();
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể lưu thông tin người dùng.', 'error');
  } finally {
    saving.value = false;
  }
};

const lockUser = async (user) => {
  const result = await Swal.fire({
    title: 'Tạm khóa tài khoản?',
    text: `Bạn muốn tạm khóa tài khoản @${user.username}?`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#d33',
    cancelButtonColor: '#2C1810',
    confirmButtonText: 'Tạm khóa',
    cancelButtonText: 'Hủy'
  });

  if (!result.isConfirmed) return;

  try {
    await api.put(`/Users/${user.userId}`, {
      fullName: user.fullName,
      email: user.email,
      role: user.role,
      isActive: false
    });

    await Swal.fire({
      icon: 'success',
      title: 'Đã tạm khóa tài khoản',
      timer: 1400,
      showConfirmButton: false
    });

    await loadUsers();
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể tạm khóa tài khoản.', 'error');
  }
};

onMounted(loadUsers);
</script>

<style scoped>
.manage-users {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.title-section h2 {
  font-size: 2rem;
  color: var(--primary);
}

.title-section p {
  color: #6b7280;
  margin-top: 0.25rem;
}

.glass-card {
  background: rgba(255, 255, 255, 0.78);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
  border-radius: 1.25rem;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  border: none;
  border-radius: 0.75rem;
  padding: 0.75rem 1.2rem;
  cursor: pointer;
  font-weight: 700;
}

.btn-primary {
  background: var(--primary);
  color: white;
}

.btn-secondary {
  background: #e5e7eb;
  color: #374151;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.25rem;
}

.stat-icon {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.9rem;
  color: var(--secondary);
  background: rgba(212, 163, 115, 0.15);
}

.stat-card h4 {
  color: #6b7280;
  font-size: 0.9rem;
}

.count {
  color: var(--primary);
  font-weight: 700;
  font-size: 1.6rem;
}

.table-container {
  padding: 1.25rem;
}

.table-actions {
  margin-bottom: 1rem;
}

.search-box {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: min(420px, 100%);
  padding: 0.75rem 1rem;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 0.85rem;
}

.search-box input {
  width: 100%;
  border: none;
  outline: none;
  background: transparent;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table th,
.data-table td {
  padding: 0.95rem;
  text-align: left;
  border-bottom: 1px solid #f1f5f9;
}

.data-table th {
  color: #6b7280;
  font-size: 0.85rem;
  text-transform: uppercase;
}

.name-cell {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.name-cell strong {
  color: var(--primary);
}

.email-text {
  color: #6b7280;
  font-size: 0.85rem;
}

.username-pill {
  display: inline-block;
  background: rgba(212, 163, 115, 0.15);
  color: var(--secondary);
  padding: 0.25rem 0.65rem;
  border-radius: 999px;
  font-weight: 700;
}

.role-badge,
.status-badge {
  display: inline-flex;
  padding: 0.3rem 0.75rem;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 700;
}

.role-admin {
  background: #fee2e2;
  color: #991b1b;
}

.role-manager {
  background: #ede9fe;
  color: #6d28d9;
}

.role-bartender {
  background: #fef3c7;
  color: #92400e;
}

.role-staff {
  background: #dbeafe;
  color: #1d4ed8;
}

.status-badge.available {
  background: #dcfce7;
  color: #166534;
}

.status-badge.unavailable {
  background: #e5e7eb;
  color: #374151;
}

.actions-cell {
  display: flex;
  gap: 0.5rem;
}

.icon-btn {
  width: 36px;
  height: 36px;
  border: none;
  border-radius: 0.7rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.icon-btn.edit {
  background: #fef3c7;
  color: #b45309;
}

.icon-btn.delete {
  background: #fee2e2;
  color: #b91c1c;
}

.icon-btn.close {
  background: #f3f4f6;
  color: #6b7280;
}

.table-state {
  padding: 2rem;
  text-align: center;
  color: #6b7280;
}

.spinner,
.spinner-small {
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.spinner {
  width: 28px;
  height: 28px;
  margin: 0 auto 1rem;
  border: 3px solid #e5e7eb;
  border-top-color: var(--primary);
}

.spinner-small {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.35);
  border-top-color: white;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.28);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  z-index: 60;
}

.modal-content {
  width: min(640px, 100%);
  padding: 1.5rem;
  background: rgba(255, 255, 255, 0.96);
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1rem;
}

.modal-header h3 {
  color: var(--primary);
  font-size: 1.35rem;
}

.modal-body {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
}

.form-group label,
.switch-label {
  color: var(--primary);
  font-weight: 700;
}

.form-group input,
.form-group select {
  padding: 0.8rem 1rem;
  border: 1px solid #d1d5db;
  border-radius: 0.85rem;
  background: #f8fafc;
  outline: none;
}

.edit-warning {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.85rem 1rem;
  border-radius: 0.85rem;
  background: #eff6ff;
  color: #1d4ed8;
}

.switch-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.switch {
  position: relative;
  width: 46px;
  height: 24px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  inset: 0;
  border-radius: 999px;
  background: #d1d5db;
  transition: 0.2s;
}

.slider::before {
  content: '';
  position: absolute;
  width: 18px;
  height: 18px;
  left: 3px;
  bottom: 3px;
  border-radius: 50%;
  background: white;
  transition: 0.2s;
}

.switch input:checked + .slider {
  background: var(--secondary);
}

.switch input:checked + .slider::before {
  transform: translateX(22px);
}

.switch-text {
  color: #6b7280;
  font-weight: 600;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 0.5rem;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 900px) {
  .stats-grid,
  .form-row {
    grid-template-columns: 1fr;
  }

  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>
