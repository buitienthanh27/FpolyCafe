<template>
  <div class="dashboard-home">
    <!-- Header Section -->
    <header class="dashboard-header">
      <div class="welcome">
        <h1>Chào buổi sáng, Admin! 👋</h1>
        <p>Đây là tình hình kinh doanh của FpolyCafe hôm nay.</p>
      </div>
      <div class="date-filter shadow-sm">
        <Calendar :size="18" />
        <span>{{ todayStr }}</span>
      </div>
    </header>

    <!-- Stats Grid -->
    <div class="stats-grid">
      <div class="stat-card glass-card purple">
        <div class="icon-box">
          <DollarSign :size="24" />
        </div>
        <div class="stat-content">
          <p>Tổng Doanh Thu</p>
          <h3>{{ formatCurrency(stats.totalRevenue) }}</h3>
          <span class="trend up">
            <TrendingUp :size="14" /> +12% so với hôm qua
          </span>
        </div>
      </div>

      <div class="stat-card glass-card blue">
        <div class="icon-box">
          <ClipboardList :size="24" />
        </div>
        <div class="stat-content">
          <p>Tổng Hóa Đơn</p>
          <h3>{{ stats.totalBills }}</h3>
          <span class="trend up">
            <TrendingUp :size="14" /> +5% mục tiêu ngày
          </span>
        </div>
      </div>

      <div class="stat-card glass-card orange">
        <div class="icon-box">
          <Users :size="24" />
        </div>
        <div class="stat-content">
          <p>Khách Hàng</p>
          <h3>{{ stats.totalCustomers }}</h3>
          <span class="trend">Tích lũy hệ thống</span>
        </div>
      </div>

      <div class="stat-card glass-card green">
        <div class="icon-box">
          <UserCheck :size="24" />
        </div>
        <div class="stat-content">
          <p>Nhân Viên Đang Làm</p>
          <h3>{{ stats.activeEmployees }}</h3>
          <span class="trend">Tính đến hiện tại</span>
        </div>
      </div>
    </div>

    <!-- Charts and Recent Activity -->
    <div class="main-grid">
      <div class="chart-container glass-card">
        <div class="card-header">
          <h3>Doanh Thu 7 Ngày Qua</h3>
          <button class="btn-more">Xem chi tiết</button>
        </div>
        <div class="chart-mockup">
          <div class="bars">
            <div v-for="day in stats.revenueChart" :key="day.day" class="bar-group">
              <div class="bar" :style="{ height: (day.revenue / maxRevenue * 100 || 10) + '%' }">
                <span class="tooltip">{{ formatCurrency(day.revenue) }}</span>
              </div>
              <span class="label">{{ day.day }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="activity-container glass-card">
        <div class="card-header">
          <h3>Hoạt Động Gần Đây</h3>
        </div>
        <div class="activity-list">
          <div v-for="(act, index) in stats.recentActivities" :key="index" class="activity-item">
            <div class="time">{{ act.time }}</div>
            <div class="dot" :class="act.type.toLowerCase()"></div>
            <div class="desc">{{ act.description }}</div>
          </div>
          <div v-if="stats.recentActivities.length === 0" class="empty">
             Chưa có hoạt động mới.
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { DollarSign, ClipboardList, Users, UserCheck, TrendingUp, Calendar } from 'lucide-vue-next';
import api from '../api';

const stats = ref({
  totalRevenue: 0,
  totalBills: 0,
  totalCustomers: 0,
  activeEmployees: 0,
  revenueChart: [],
  recentActivities: []
});

const todayStr = new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });

const formatCurrency = (val) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
};

const maxRevenue = computed(() => {
  if (stats.value.revenueChart.length === 0) return 1;
  return Math.max(...stats.value.revenueChart.map(d => d.revenue)) || 1;
});

const loadStats = async () => {
  try {
    const [dashboard, revenue] = await Promise.all([
      api.get('/Reports/dashboard'),
      api.get('/Reports/revenue?days=7')
    ]);

    stats.value = {
      totalRevenue: dashboard.totalRevenue ?? 0,
      totalBills: dashboard.totalOrders ?? 0,
      totalCustomers: 0,
      activeEmployees: 0,
      revenueChart: (revenue || []).map((item) => ({
        day: new Date(item.date).toLocaleDateString('vi-VN', { weekday: 'short' }),
        revenue: item.revenue ?? 0
      })),
      recentActivities: []
    };
  } catch (error) {
    console.error('Error loading stats:', error);
  }
};

onMounted(() => {
  loadStats();
});
</script>

<style scoped>
.dashboard-home {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  padding: 0.5rem;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.welcome h1 {
  font-size: 1.8rem;
  color: #2c1810;
  margin-bottom: 0.25rem;
  font-family: 'Outfit', sans-serif;
}

.welcome p {
  color: #777;
}

.date-filter {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: white;
  padding: 0.75rem 1.25rem;
  border-radius: 12px;
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--primary);
}

/* Stats Cards */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.5rem;
}

.stat-card {
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1.25rem;
  transition: transform 0.3s;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.icon-box {
  width: 56px;
  height: 56px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.purple .icon-box { background: linear-gradient(135deg, #a855f7, #7c3aed); }
.blue .icon-box { background: linear-gradient(135deg, #3b82f6, #2563eb); }
.orange .icon-box { background: linear-gradient(135deg, #f97316, #ea580c); }
.green .icon-box { background: linear-gradient(135deg, #10b981, #059669); }

.stat-content p {
  color: #888;
  font-size: 0.85rem;
  margin-bottom: 0.25rem;
}

.stat-content h3 {
  font-size: 1.4rem;
  font-weight: 800;
  margin-bottom: 0.25rem;
  color: #2c1810;
}

.trend {
  font-size: 0.75rem;
  color: #888;
  display: flex;
  align-items: center;
  gap: 4px;
}

.trend.up { color: #10b981; }

/* Main Grid */
.main-grid {
  display: grid;
  grid-template-columns: 1fr 350px;
  gap: 1.5rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.card-header h3 {
  font-size: 1.1rem;
  font-weight: 700;
}

.btn-more {
  background: none;
  border: none;
  color: var(--secondary);
  font-weight: 600;
  font-size: 0.85rem;
  cursor: pointer;
}

/* Chart Mockup */
.chart-mockup {
  height: 250px;
  display: flex;
  align-items: flex-end;
  padding-bottom: 2rem;
}

.bars {
  display: flex;
  justify-content: space-around;
  align-items: flex-end;
  width: 100%;
  height: 100%;
  gap: 1rem;
}

.bar-group {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 100%;
  justify-content: flex-end;
}

.bar {
  width: 100%;
  max-width: 40px;
  background: linear-gradient(to top, var(--primary), var(--secondary));
  border-radius: 8px 8px 0 0;
  position: relative;
  transition: all 0.5s ease;
  cursor: pointer;
}

.bar:hover {
  filter: brightness(1.1);
}

.tooltip {
  position: absolute;
  top: -30px;
  left: 50%;
  transform: translateX(-50%);
  background: #333;
  color: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.7rem;
  opacity: 0;
  transition: 0.2s;
  pointer-events: none;
  white-space: nowrap;
}

.bar:hover .tooltip { opacity: 1; }

.label {
  margin-top: 0.75rem;
  font-size: 0.75rem;
  color: #888;
  font-weight: 600;
}

/* Activity List */
.activity-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  position: relative;
}

.time {
  font-size: 0.75rem;
  color: #aaa;
  font-weight: 600;
  width: 40px;
  padding-top: 2px;
}

.dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: #ddd;
  margin-top: 6px;
  z-index: 2;
}

.dot.bill { background: var(--secondary); }

.activity-item:not(:last-child)::after {
  content: '';
  position: absolute;
  left: 54px;
  top: 16px;
  bottom: -20px;
  width: 2px;
  background: #f0f0f0;
}

.desc {
  font-size: 0.9rem;
  color: #444;
  font-weight: 500;
}

.empty {
  text-align: center;
  color: #bbb;
  padding: 2rem;
}

.glass-card {
  background: white;
  border-radius: 20px;
  padding: 1.5rem;
  border: 1px solid #f0f0f0;
  box-shadow: 0 4px 20px rgba(0,0,0,0.02);
}
</style>
