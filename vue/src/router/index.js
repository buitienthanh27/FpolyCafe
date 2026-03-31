import { createRouter, createWebHistory } from 'vue-router';

const requireAuth = () => {
  const token = localStorage.getItem('token');
  if (!token) {
    return '/login';
  }

  return true;
};

const requireAdminAccess = () => {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');

  if (!token) {
    return '/login';
  }

  if (role === '0' || role === '2' || role === 'Admin' || role === 'Manager') {
    return true;
  }

  return '/cashier';
};

const requireBartenderAccess = () => {
  const token = localStorage.getItem('token');
  const role = localStorage.getItem('role');

  if (!token) {
    return '/login';
  }

  if (role === '0' || role === '2' || role === '3' || role === 'Admin' || role === 'Manager' || role === 'Bartender') {
    return true;
  }

  return '/cashier';
};

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue'),
    meta: { title: 'FpolyCafe - Trang chủ' }
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { title: 'Đăng nhập - FpolyCafe' }
  },
  {
    path: '/cashier',
    name: 'Cashier',
    component: () => import('../views/Cashier.vue'),
    beforeEnter: requireAuth,
    meta: { title: 'Bán hàng - FpolyCafe' }
  },
  {
    path: '/payment',
    name: 'Payment',
    component: () => import('../views/Payment.vue'),
    beforeEnter: requireAuth,
    meta: { title: 'Thanh toán - FpolyCafe' }
  },
  {
    path: '/timekeeping',
    name: 'Timekeeping',
    component: () => import('../views/Timekeeping.vue'),
    beforeEnter: requireAuth,
    meta: { title: 'Chấm công - FpolyCafe' }
  },
  {
    path: '/admin',
    name: 'AdminDashboard',
    component: () => import('../views/AdminDashboard.vue'),
    beforeEnter: requireAdminAccess,
    meta: { title: 'Quản trị hệ thống' },
    children: [
      {
        path: '',
        name: 'DashboardOverview',
        component: () => import('../views/DashboardHome.vue')
      },
      {
        path: 'products',
        name: 'ManageProducts',
        component: () => import('../views/ManageProducts.vue')
      },
      {
        path: 'categories',
        name: 'ManageCategories',
        component: () => import('../views/ManageCategories.vue')
      },
      {
        path: 'bills',
        name: 'ManageBills',
        component: () => import('../views/ManageBills.vue')
      },
      {
        path: 'customers',
        name: 'ManageCustomers',
        component: () => import('../views/ManageCustomers.vue')
      },
      {
        path: 'reports',
        name: 'Reports',
        component: () => import('../views/Reports.vue')
      },
      {
        path: 'toppings',
        name: 'ManageToppings',
        component: () => import('../views/ManageToppings.vue')
      },
      {
        path: 'users',
        name: 'ManageUsers',
        component: () => import('../views/ManageUsers.vue')
      },
      {
        path: 'ingredients',
        name: 'ManageIngredients',
        component: () => import('../views/ManageIngredients.vue')
      },
      {
        path: 'recipes',
        name: 'ManageRecipes',
        component: () => import('../views/ManageRecipes.vue')
      },
      {
        path: 'shifts',
        name: 'ManageShifts',
        component: () => import('../views/ManageShifts.vue')
      },
      {
        path: 'promotions',
        name: 'ManagePromotions',
        component: () => import('../views/ManagePromotions.vue')
      },
      {
        path: 'inventory',
        name: 'ManageInventory',
        component: () => import('../views/ManageInventory.vue')
      },
      {
        path: 'attendance',
        name: 'ManageAttendance',
        component: () => import('../views/ManageAttendance.vue')
      }
    ]
  },
  {
    path: '/bartender',
    name: 'Bartender',
    component: () => import('../views/Bartender.vue'),
    beforeEnter: requireBartenderAccess,
    meta: { title: 'Khu vực pha chế - FpolyCafe' }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

router.beforeEach((to) => {
  document.title = to.meta.title || 'FpolyCafe';
  return true;
});

export default router;
