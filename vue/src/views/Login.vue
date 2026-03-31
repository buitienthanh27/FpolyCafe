<template>
  <div class="login-container">
    <div class="bg-img" :style="{ backgroundImage: 'url(/assets/login_bg.png)' }"></div>
    <div class="glass-overlay"></div>

    <div class="login-card glass slide-up">
      <div class="header">
        <h1 class="logo-text">Fpoly<span>Cafe</span></h1>
        <p>Chào mừng bạn quay trở lại!</p>
      </div>

      <form @submit.prevent="handleLogin" class="form-container">
        <div class="input-group">
          <label for="username">Tên đăng nhập</label>
          <div class="input-wrapper">
             <User class="icon" :size="20"/>
             <input 
               type="text" 
               id="username" 
               v-model="loginForm.username" 
               placeholder="Nhập tên đăng nhập" 
               required
               class="input-field"
             />
          </div>
        </div>

        <div class="input-group">
          <label for="password">Mật khẩu</label>
          <div class="input-wrapper">
             <Lock class="icon" :size="20"/>
             <input 
               :type="showPassword ? 'text' : 'password'" 
               id="password" 
               v-model="loginForm.password" 
               placeholder="Nhập mật khẩu" 
               required
               class="input-field"
             />
             <Eye v-if="!showPassword" @click="showPassword = true" class="icon-right" :size="20"/>
             <EyeOff v-else @click="showPassword = false" class="icon-right" :size="20"/>
          </div>
        </div>

        <div class="actions">
           <label class="checkbox-container">
             <input type="checkbox" v-model="rememberMe">
             <span class="checkmark"></span>
             Duy trì đăng nhập
           </label>
           <a href="#" class="forgot-pwd">Quên mật khẩu?</a>
        </div>

        <button type="submit" class="btn btn-primary w-full" :disabled="loading">
           <span v-if="!loading">Đăng Nhập</span>
           <span v-else class="loader"></span>
        </button>
      </form>
      
      <div class="footer">
         <p>Bạn là khách hàng? <router-link to="/">Xem Menu</router-link></p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import { User, Lock, Eye, EyeOff, Coffee } from 'lucide-vue-next';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';
import api from '../api';

const router = useRouter();
const showPassword = ref(false);
const loading = ref(false);
const rememberMe = ref(false);

const loginForm = reactive({
  username: '',
  password: ''
});

const handleLogin = async () => {
  loading.value = true;
  try {
    const response = await api.post('/Auth/login', loginForm);
    console.log('Login success:', response);
    
    localStorage.setItem('token', response.token);
    localStorage.setItem('fullName', response.fullName);
    localStorage.setItem('role', String(response.role));

    if (response.userId !== undefined && response.userId !== null) {
      localStorage.setItem('userId', String(response.userId));
    } else {
      localStorage.removeItem('userId');
    }
    
    Swal.fire({
      icon: 'success',
      title: 'Thành Công!',
      text: `Chào mừng ${response.fullName} quay trở lại!`,
      timer: 1500,
      showConfirmButton: false,
      background: '#F9F5F1',
      color: '#2C1810',
      iconColor: '#D4A373'
    });
    
    // Check role from API response (enum value or string)
    const role = response.role;
    if (role === 0 || role === 'Admin') {
      router.push('/admin');
    } else if (role === 2 || role === 'Manager') {
      router.push('/admin');
    } else if (role === 3 || role === 'Bartender') {
      router.push('/bartender');
    } else {
      router.push('/cashier');
    }
  } catch (error) {
    console.error('Login error details:', error);
    let errorMessage = 'Tên đăng nhập hoặc mật khẩu không chính xác.';
    
    if (error?.code === 'ERR_NETWORK' || error?.message?.includes('Network Error')) {
      errorMessage = `Không thể kết nối tới Server. API hiện đang được cấu hình tại ${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5189/api'}.`;
    } else if (error?.message) {
      errorMessage = error.message;
    } else if (typeof error === 'string') {
      errorMessage = error;
    }
    
    Swal.fire({
      icon: 'error',
      title: 'Lỗi Kết Nối',
      text: errorMessage,
      footer: '<p style="color:red">Gợi ý: Chạy API từ project FpolyCafe (profiles: http)</p>',
      confirmButtonColor: '#2C1810'
    });
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.login-container {
  height: 100vh;
  width: 100vw;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

.bg-img {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-size: cover;
  background-position: center;
  transition: transform 1s ease-out;
}

.login-container:hover .bg-img {
  transform: scale(1.05);
}

.glass-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: radial-gradient(circle, rgba(0,0,0,0.2) 0%, rgba(0,0,0,0.6) 100%);
}

.login-card {
  width: 100%;
  max-width: 450px;
  padding: 3rem;
  z-index: 10;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.header {
  text-align: center;
}

.logo-text {
  font-size: 2.5rem;
  font-family: 'Outfit', sans-serif;
  letter-spacing: -1px;
}

.logo-text span {
  color: var(--secondary);
}

.header p {
  color: #555;
  margin-top: 0.5rem;
  font-size: 0.95rem;
}

.form-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.input-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--primary);
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-wrapper .icon {
  position: absolute;
  left: 1rem;
  color: #aaa;
}

.input-wrapper .icon-right {
  position: absolute;
  right: 1rem;
  color: #aaa;
  cursor: pointer;
  transition: color 0.3s;
}

.input-wrapper .icon-right:hover {
  color: var(--secondary);
}

.input-wrapper input {
  padding-left: 3rem;
  padding-right: 3rem;
}

.actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.85rem;
}

.checkbox-container {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  color: #555;
}

.forgot-pwd {
  color: var(--secondary);
  text-decoration: none;
  font-weight: 500;
}

.w-full {
  width: 100%;
}

.footer {
  text-align: center;
  font-size: 0.9rem;
  color: #555;
}

.footer a {
  color: var(--secondary);
  font-weight: 600;
  text-decoration: none;
}

.slide-up {
  animation: slideUp 0.8s cubic-bezier(0.23, 1, 0.32, 1) forwards;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(40px); }
  to { opacity: 1; transform: translateY(0); }
}

.loader {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
