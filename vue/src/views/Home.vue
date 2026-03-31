<template>
  <div class="home-container">
    <section class="hero glass-container">
      <div class="hero-content fade-in">
        <h1>Fpoly<span>Cafe</span></h1>
        <p class="subtitle">Thưởng thức trọn vẹn hương vị Café Việt. Tinh hoa từ những hạt cà phê được tuyển chọn kỹ lưỡng, mang đến trải nghiệm tuyệt vời nhất cho bạn.</p>
        <div class="cta-btns">
            <router-link to="/login" class="btn btn-primary">
                Đăng Nhập Quản Trị
                <ArrowRight :size="18"/>
            </router-link>
            <button class="btn btn-secondary glass">
                Khám Phá Menu
                <Coffee :size="18"/>
            </button>
        </div>
      </div>
      
      <div class="hero-image slide-left">
          <img src="https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?auto=format&fit=crop&q=80&w=1000" alt="Special Coffee">
      </div>
    </section>

    <section class="features">
       <div class="feature-card glass-card">
          <div class="feature-icon"><Zap :size="32"/></div>
          <h3>Đặt Món Nhanh</h3>
          <p>Dễ dàng đặt món chỉ với vài chạm trên bất kỳ thiết bị nào.</p>
       </div>
       <div class="feature-card glass-card">
          <div class="feature-icon"><Heart :size="32"/></div>
          <h3>Yêu Thích</h3>
          <p>Lưu lại những hương vị bạn yêu thích để trải nghiệm lần sau.</p>
       </div>
       <div class="feature-card glass-card">
          <div class="feature-icon"><Clock :size="32"/></div>
          <h3>Giao Hàng Siêu Tốc</h3>
          <p>Chất lượng cà phê luôn nóng hổi khi đến tay bạn.</p>
       </div>
    </section>
  </div>
</template>

<script setup>
import { ArrowRight, Coffee, Zap, Heart, Clock } from 'lucide-vue-next';
</script>

<style scoped>
.home-container {
  min-height: 100vh;
  background: var(--background);
  padding: 4rem 2rem;
  max-width: 1300px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 6rem;
}

.hero {
  display: grid;
  grid-template-columns: 1fr 1fr;
  align-items: center;
  gap: 4rem;
  background: var(--white);
  padding: 4rem;
  border-radius: 2.5rem;
  box-shadow: var(--shadow);
}

.hero-content h1 {
  font-size: 5rem;
  margin-bottom: 1rem;
}

.hero-content h1 span {
  color: var(--secondary);
}

.subtitle {
  font-size: 1.25rem;
  color: #666;
  margin-bottom: 2.5rem;
  max-width: 90%;
}

.cta-btns {
  display: flex;
  gap: 1.5rem;
}

.hero-image img {
  width: 100%;
  border-radius: 2rem;
  box-shadow: var(--shadow-lg);
  aspect-ratio: 1;
  object-fit: cover;
}

.features {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2rem;
}

.feature-card {
  padding: 3rem 2rem;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
}

.feature-icon {
  width: 64px;
  height: 64px;
  background: var(--accent);
  color: var(--primary);
  border-radius: 1.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.feature-card h3 {
  font-size: 1.5rem;
}

.feature-card p {
  color: #777;
}

.slide-left {
  animation: slideLeft 1.2s cubic-bezier(0.23, 1, 0.32, 1) forwards;
}

@keyframes slideLeft {
  from { opacity: 0; transform: translateX(50px); }
  to { opacity: 1; transform: translateX(0); }
}

@media (max-width: 968px) {
  .hero {
     grid-template-columns: 1fr;
     text-align: center;
     padding: 3rem 1.5rem;
  }
  .hero-content h1 { font-size: 3.5rem; }
  .cta-btns { justify-content: center; }
  .features { grid-template-columns: 1fr; }
  .subtitle { margin: 0 auto 2.5rem; }
}
</style>
