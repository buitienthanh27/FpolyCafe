<template>
  <div class="admin-layout">
    <aside class="sidebar glass" :class="{ collapsed: isCollapsed }">
      <div class="sidebar-header">
        <div class="logo">
          <Coffee :size="32" color="var(--secondary)" />
          <span v-if="!isCollapsed">Fpoly<span>Cafe</span></span>
        </div>
        <button class="collapse-btn" @click="isCollapsed = !isCollapsed">
          <ChevronLeft v-if="!isCollapsed" :size="20" />
          <ChevronRight v-else :size="20" />
        </button>
      </div>

      <nav class="sidebar-nav">
        <router-link to="/admin" class="nav-item" exact-active-class="active">
          <LayoutDashboard :size="24" />
          <span v-if="!isCollapsed">Tổng quan</span>
        </router-link>
        <router-link to="/admin/products" class="nav-item" active-class="active">
          <Coffee :size="24" />
          <span v-if="!isCollapsed">Sản phẩm</span>
        </router-link>
        <router-link to="/admin/categories" class="nav-item" active-class="active">
          <Tag :size="24" />
          <span v-if="!isCollapsed">Danh mục</span>
        </router-link>
        <router-link to="/admin/toppings" class="nav-item" active-class="active">
          <CupSoda :size="24" />
          <span v-if="!isCollapsed">Topping</span>
        </router-link>
        <router-link to="/admin/bills" class="nav-item" active-class="active">
          <ClipboardList :size="24" />
          <span v-if="!isCollapsed">Hóa đơn</span>
        </router-link>
        <router-link to="/admin/users" class="nav-item" active-class="active">
          <Users :size="24" />
          <span v-if="!isCollapsed">Người dùng</span>
        </router-link>
        <router-link to="/admin/ingredients" class="nav-item" active-class="active">
          <Package :size="24" />
          <span v-if="!isCollapsed">Kho nguyên liệu</span>
        </router-link>
        <router-link to="/admin/recipes" class="nav-item" active-class="active">
          <BookOpen :size="24" />
          <span v-if="!isCollapsed">Công thức</span>
        </router-link>
        <router-link to="/admin/shifts" class="nav-item" active-class="active">
          <Clock :size="24" />
          <span v-if="!isCollapsed">Ca làm việc</span>
        </router-link>
        <router-link to="/admin/promotions" class="nav-item" active-class="active">
          <Ticket :size="24" />
          <span v-if="!isCollapsed">Khuyến mãi</span>
        </router-link>
        <router-link to="/admin/inventory" class="nav-item" active-class="active">
          <Warehouse :size="24" />
          <span v-if="!isCollapsed">Nhập kho</span>
        </router-link>
        <router-link to="/admin/customers" class="nav-item" active-class="active">
          <Contact :size="24" />
          <span v-if="!isCollapsed">Khách hàng</span>
        </router-link>
        <router-link to="/admin/attendance" class="nav-item" active-class="active">
          <ClipboardCheck :size="24" />
          <span v-if="!isCollapsed">Quản lý chấm công</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <button class="logout-btn" @click="handleLogout">
          <LogOut :size="24" />
          <span v-if="!isCollapsed">Đăng xuất</span>
        </button>
      </div>
    </aside>

    <main class="main-content">
      <header class="topbar glass">
        <div class="search-bar">
          <Search :size="20" />
          <input type="text" placeholder="Tìm kiếm nhanh..." />
        </div>

        <div class="user-profile">
          <div class="notifications">
            <Bell :size="24" />
            <span class="badge"></span>
          </div>
          <div class="divider"></div>
          <div class="user-info">
            <div class="user-name">{{ fullName }}</div>
            <div class="user-role">{{ roleLabel }}</div>
          </div>
          <div class="user-avatar">{{ avatarText }}</div>
        </div>
      </header>

      <div class="content-view slide-up">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </div>
    </main>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue';
import {
  Bell,
  BookOpen,
  ChevronLeft,
  ChevronRight,
  ClipboardCheck,
  ClipboardList,
  Clock,
  Coffee,
  Contact,
  CupSoda,
  LayoutDashboard,
  LogOut,
  Package,
  Search,
  Tag,
  Ticket,
  Users,
  Warehouse
} from 'lucide-vue-next';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';

const isCollapsed = ref(false);
const router = useRouter();

const fullName = computed(() => localStorage.getItem('fullName') || 'Người dùng');
const roleValue = computed(() => localStorage.getItem('role') || '');
const roleLabel = computed(() => {
  if (roleValue.value === '0' || roleValue.value === 'Admin') return 'Quản trị';
  if (roleValue.value === '2' || roleValue.value === 'Manager') return 'Quản lý';
  return 'Nhân viên';
});
const avatarText = computed(() => {
  const name = fullName.value.trim();
  if (!name) return 'U';
  const parts = name.split(/\s+/);
  return parts.slice(0, 2).map((item) => item[0]).join('').toUpperCase();
});

const clearSession = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('fullName');
  localStorage.removeItem('role');
  localStorage.removeItem('userId');
};

const handleLogout = () => {
  Swal.fire({
    title: 'Đăng xuất?',
    text: 'Bạn có chắc chắn muốn rời khỏi hệ thống?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#2C1810',
    cancelButtonColor: '#d33',
    confirmButtonText: 'Đăng xuất',
    cancelButtonText: 'Hủy'
  }).then((result) => {
    if (!result.isConfirmed) return;
    clearSession();
    router.push('/login');
  });
};
</script>

<style scoped>
.admin-layout {
  display: flex;
  min-height: 100vh;
  background-color: var(--background);
  color: var(--text-main);
}

.glass {
  background: rgba(255, 255, 255, 0.75);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
}

.sidebar {
  width: 280px;
  height: 100vh;
  position: fixed;
  left: 0;
  top: 0;
  display: flex;
  flex-direction: column;
  padding: 1.5rem;
  transition: all 0.35s ease;
  z-index: 100;
}

.sidebar.collapsed {
  width: 90px;
  padding: 1rem;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 2rem;
  padding: 0 0.25rem;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  font-family: 'Outfit', sans-serif;
  font-weight: 700;
  font-size: 1.45rem;
}

.logo span span {
  color: var(--secondary);
}

.collapse-btn {
  background: white;
  border: none;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.sidebar-nav {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.7rem;
  overflow-y: auto;
  padding-right: 0.35rem;
}

.sidebar-nav::-webkit-scrollbar {
  width: 5px;
}

.sidebar-nav::-webkit-scrollbar-thumb {
  background: #d1d5db;
  border-radius: 999px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border-radius: 0.85rem;
  color: #555;
  text-decoration: none;
  font-weight: 600;
  transition: 0.2s ease;
}

.nav-item:hover,
.nav-item.active {
  background: var(--primary);
  color: white;
}

.sidebar.collapsed .nav-item {
  justify-content: center;
  padding: 1rem 0;
}

.logout-btn {
  margin-top: auto;
  width: 100%;
  border: none;
  background: none;
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border-radius: 0.85rem;
  cursor: pointer;
  color: #b91c1c;
  font-weight: 700;
}

.logout-btn:hover {
  background: rgba(185, 28, 28, 0.08);
}

.main-content {
  flex: 1;
  margin-left: 280px;
  padding: 1.5rem;
  transition: margin-left 0.35s ease;
}

.collapsed + .main-content {
  margin-left: 90px;
}

.topbar {
  height: 80px;
  padding: 0 1.75rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
  border-radius: 1.25rem;
}

.search-bar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: #fff;
  padding: 0.75rem 1.25rem;
  border-radius: 1rem;
  width: min(420px, 100%);
}

.search-bar input {
  border: none;
  outline: none;
  width: 100%;
  background: transparent;
}

.user-profile {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.notifications {
  position: relative;
  color: #555;
}

.badge {
  position: absolute;
  top: 2px;
  right: 2px;
  width: 8px;
  height: 8px;
  background: var(--secondary);
  border-radius: 50%;
  border: 2px solid white;
}

.divider {
  width: 1px;
  height: 26px;
  background: #d1d5db;
}

.user-info {
  text-align: right;
}

.user-name {
  font-weight: 700;
}

.user-role {
  font-size: 0.85rem;
  color: #6b7280;
}

.user-avatar {
  width: 44px;
  height: 44px;
  border-radius: 0.8rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--primary), var(--secondary));
  color: white;
  font-weight: 800;
}

.content-view {
  min-height: calc(100vh - 140px);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.25s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-up {
  animation: slideUp 0.4s ease;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 960px) {
  .sidebar {
    width: 90px;
    padding: 1rem;
  }

  .main-content {
    margin-left: 90px;
  }

  .logo span,
  .nav-item span,
  .logout-btn span {
    display: none;
  }
}
</style>
