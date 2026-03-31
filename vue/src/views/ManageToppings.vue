<template>
  <div class="manage-toppings">
    <div class="page-header slide-up">
      <div class="title-section">
        <h2>Quan ly topping</h2>
        <p>Backend hien tai chi cung cap topping lookup de phuc vu ban hang.</p>
      </div>
      <button class="btn btn-disabled" disabled>
        <Plus :size="20" />
        Chua ho tro CRUD topping
      </button>
    </div>

    <div class="notice-card glass-card slide-up" style="animation-delay: 0.1s">
      <Info :size="20" />
      <div>
        <strong>Che do chi doc</strong>
        <p>Du lieu duoi day duoc tai tu <code>/api/Lookups/toppings</code>. Neu muon sua topping, backend can bo sung ToppingsController.</p>
      </div>
    </div>

    <div class="stats-grid slide-up" style="animation-delay: 0.2s">
      <div class="stat-card glass-card">
        <div class="stat-icon"><CupSoda :size="24"/></div>
        <div class="stat-info">
          <h4>Tong so topping</h4>
          <p class="count">{{ toppings.length }}</p>
        </div>
      </div>
      <div class="stat-card glass-card">
        <div class="stat-icon"><ShieldCheck :size="24"/></div>
        <div class="stat-info">
          <h4>Dang hoat dong</h4>
          <p class="count">{{ toppings.filter(t => t.isActive).length }}</p>
        </div>
      </div>
    </div>

    <div class="table-container glass-card slide-up" style="animation-delay: 0.3s">
      <div class="table-actions">
        <div class="search-box">
          <Search :size="18"/>
          <input v-model="searchQuery" type="text" placeholder="Tim theo ten topping...">
        </div>
      </div>

      <table class="data-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Ten topping</th>
            <th>Gia ban</th>
            <th>Trang thai</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="topping in filteredToppings" :key="topping.toppingId">
            <td>#{{ topping.toppingId }}</td>
            <td><strong>{{ topping.toppingName }}</strong></td>
            <td><span class="price-text">{{ formatPrice(topping.price) }}</span></td>
            <td>
              <span class="status-badge" :class="topping.isActive ? 'available' : 'unavailable'">
                {{ topping.isActive ? 'Dang ban' : 'Tam ngung' }}
              </span>
            </td>
          </tr>
          <tr v-if="!loading && filteredToppings.length === 0">
            <td colspan="4" class="empty-state">Khong co topping nao.</td>
          </tr>
        </tbody>
      </table>

      <div v-if="loading" class="table-loading">
        <div class="spinner"></div>
        Dang tai du lieu topping...
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { Plus, CupSoda, ShieldCheck, Search, Info } from 'lucide-vue-next';
import api from '../api';
import Swal from 'sweetalert2';

const toppings = ref([]);
const loading = ref(false);
const searchQuery = ref('');

const filteredToppings = computed(() =>
  toppings.value.filter((t) => t.toppingName.toLowerCase().includes(searchQuery.value.toLowerCase()))
);

const loadData = async () => {
  loading.value = true;
  try {
    toppings.value = await api.get('/Lookups/toppings');
  } catch (error) {
    console.error(error);
    Swal.fire('Loi', 'Khong the tai topping lookup tu backend.', 'error');
  } finally {
    loading.value = false;
  }
};

const formatPrice = (price) =>
  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);

onMounted(loadData);
</script>

<style scoped>
.manage-toppings { display: flex; flex-direction: column; gap: 2rem; }
.glass-card { background: rgba(255,255,255,.75); backdrop-filter: blur(16px); border: 1px solid rgba(255,255,255,.4); box-shadow: 0 8px 32px rgba(0,0,0,.05); border-radius: 1.25rem; }
.page-header { display: flex; justify-content: space-between; align-items: center; }
.title-section h2 { font-size: 2rem; color: var(--primary); }
.title-section p { color: #777; margin-top: .25rem; }
.btn { display: flex; align-items: center; gap: .5rem; padding: .75rem 1.5rem; border: none; border-radius: .75rem; font-weight: 600; }
.btn-disabled { background: #e5e7eb; color: #6b7280; cursor: not-allowed; }
.notice-card { display: flex; gap: 1rem; padding: 1rem 1.25rem; color: #374151; }
.notice-card p { margin-top: .25rem; color: #6b7280; }
.stats-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1.5rem; }
.stat-card { display: flex; align-items: center; gap: 1.5rem; padding: 1.5rem; }
.stat-icon { width: 50px; height: 50px; background: rgba(212,163,115,.15); border-radius: .75rem; display: flex; align-items: center; justify-content: center; color: var(--secondary); }
.stat-info h4 { font-size: .9rem; color: #777; }
.count { font-size: 1.7rem; font-weight: 700; color: var(--primary); }
.table-container { padding: 1.5rem; overflow: hidden; }
.search-box { display: flex; align-items: center; gap: .75rem; background: #fff; border: 1px solid #e2e8f0; padding: .6rem 1rem; border-radius: .75rem; width: 400px; }
.search-box input { border: none; background: none; width: 100%; outline: none; }
.data-table { width: 100%; border-collapse: collapse; }
.data-table th { text-align: left; padding: 1rem; font-weight: 700; color: var(--primary); border-bottom: 2px solid #f1f5f9; }
.data-table td { padding: 1rem; border-bottom: 1px solid #f8fafc; }
.price-text { font-weight: 700; color: var(--secondary); }
.status-badge { padding: .3rem .75rem; border-radius: .5rem; font-size: .8rem; font-weight: 600; }
.status-badge.available { background: #dcfce7; color: #166534; }
.status-badge.unavailable { background: #f1f5f9; color: #475569; }
.table-loading, .empty-state { padding: 3rem; text-align: center; color: #999; }
.spinner { width: 30px; height: 30px; border: 3px solid #eee; border-top-color: var(--primary); border-radius: 50%; animation: spin 1s infinite linear; margin: 0 auto 1rem; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
