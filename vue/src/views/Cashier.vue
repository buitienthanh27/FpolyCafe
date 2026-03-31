<template>
  <div class="cashier-page">
    <aside class="sidebar glass-card">
      <div class="brand">
        <h1>Fpoly<span>Cafe</span></h1>
        <p>{{ fullName }}</p>
      </div>

      <div class="sidebar-actions">
        <router-link to="/timekeeping" class="side-link">
          <ClipboardList :size="18" />
          Chấm công
        </router-link>
        <button class="side-link logout" @click="handleLogout">
          <LogOut :size="18" />
          Đăng xuất
        </button>
      </div>

      <div class="category-list">
        <button :class="['category-btn', { active: selectedCategory === null }]" @click="selectedCategory = null">
          Tất cả
        </button>
        <button
          v-for="category in categories"
          :key="category.categoryId"
          :class="['category-btn', { active: selectedCategory === category.categoryId }]"
          @click="selectedCategory = category.categoryId"
        >
          {{ category.name }}
        </button>
      </div>
    </aside>

    <main class="main-panel">
      <header class="topbar glass-card">
        <div class="search-box">
          <Search :size="18" />
          <input v-model="searchQuery" type="text" placeholder="Tìm sản phẩm..." />
        </div>
        <div class="date-box">{{ currentDate }}</div>
      </header>

      <section class="product-grid">
        <button
          v-for="product in filteredProducts"
          :key="product.productId"
          class="product-card glass-card"
          @click="openProduct(product)"
        >
          <div class="product-image-wrap">
            <img
              v-if="product.imageUrl"
              :src="product.imageUrl"
              :alt="product.name"
              class="product-image"
              @error="handleProductImageError"
            >
            <div v-else class="product-icon">
              <Coffee :size="28" />
            </div>
          </div>
          <strong class="product-name">{{ product.name }}</strong>
          <span>{{ formatPrice(product.price) }}đ</span>
        </button>
      </section>
    </main>

    <section class="cart-panel glass-card">
      <div class="cart-header">
        <h2>Đơn hàng hiện tại</h2>
        <button class="clear-btn" @click="clearCart" :disabled="cart.length === 0">Xóa hết</button>
      </div>

      <div class="customer-box">
        <label>Khách hàng</label>
        <div v-if="!selectedCustomer" class="customer-search">
          <input v-model="searchPhone" type="text" placeholder="Nhập số điện thoại..." @keyup.enter="searchCustomer" />
          <button class="btn-secondary small" @click="searchCustomer">Tìm</button>
          <button class="btn-primary small" @click="openAddCustomerModal">Mới</button>
        </div>
        <div v-else class="selected-customer">
          <div>
            <strong>{{ selectedCustomer.fullName }}</strong>
            <p>{{ selectedCustomer.phoneNumber }} | Điểm tích lũy: {{ selectedCustomer.rewardPoints }}</p>
          </div>
          <button class="icon-clear" @click="clearSelectedCustomer">×</button>
        </div>
        <p class="helper-text">
          Nhập số điện thoại để kiểm tra khách thành viên. Khách vãng lai vẫn có thể tiếp tục thanh toán bình thường.
        </p>
      </div>

      <div class="cart-list">
        <div v-for="item in cart" :key="item.cartId" class="cart-item">
          <div>
            <strong>{{ item.name }}</strong>
            <p>
              {{ item.customization.sizeName }}
              <template v-if="item.customization.extras.length">
                | {{ item.customization.extras.map((extra) => extra.toppingName).join(', ') }}
              </template>
            </p>
            <p v-if="item.customization.notes" class="note">{{ item.customization.notes }}</p>
            <span>{{ formatPrice(item.price) }}đ</span>
          </div>
          <div class="qty-box">
            <button @click="decreaseQty(item)">-</button>
            <span>{{ item.quantity }}</span>
            <button @click="increaseQty(item)">+</button>
            <button class="remove-btn" @click="removeFromCart(item)">
              <Trash2 :size="15" />
            </button>
          </div>
        </div>
        <div v-if="cart.length === 0" class="empty-cart">
          <ShoppingCart :size="44" />
          <p>Chưa có món nào trong giỏ.</p>
        </div>
      </div>

      <div class="promotion-box">
        <div class="promotion-header">
          <label>Khuyến mãi từ hệ thống</label>
          <button v-if="selectedPromotion" class="clear-promotion-btn" @click="clearPromotion">Bỏ chọn</button>
        </div>

        <div v-if="cart.length === 0" class="promotion-empty">
          Thêm món vào đơn để xem các khuyến mãi khả dụng.
        </div>

        <div v-else-if="availablePromotions.length === 0" class="promotion-empty">
          Chưa có khuyến mãi phù hợp với giá trị đơn hàng hiện tại.
        </div>

        <div v-else class="promotion-list">
          <button
            v-for="promotion in availablePromotions"
            :key="promotion.promotionId"
            :class="['promotion-card', { active: selectedPromotion?.promotionId === promotion.promotionId }]"
            @click="selectPromotion(promotion)"
          >
            <div class="promotion-main">
              <strong>{{ promotion.code }}</strong>
              <span>{{ promotion.description }}</span>
            </div>
            <div class="promotion-side">
              <strong>{{ formatPromotionValue(promotion) }}</strong>
              <small v-if="promotion.minimumOrderAmount">Tối thiểu {{ formatPrice(promotion.minimumOrderAmount) }}đ</small>
            </div>
          </button>
        </div>
      </div>

      <div class="summary-box">
        <div><span>Tạm tính</span><strong>{{ formatPrice(subtotal) }}đ</strong></div>
        <div v-if="selectedPromotion"><span>Mã giảm giá</span><strong>{{ selectedPromotion.code }}</strong></div>
        <div v-if="discountAmount > 0"><span>Giảm giá</span><strong>-{{ formatPrice(discountAmount) }}đ</strong></div>
        <div class="total"><span>Tổng cộng</span><strong>{{ formatPrice(totalPrice) }}đ</strong></div>
      </div>

      <button class="checkout-btn" :disabled="cart.length === 0" @click="handleCheckout">
        <CreditCard :size="18" />
        Thanh toán
      </button>
    </section>

    <div v-if="showDetailModal && selectedProduct" class="modal-overlay" @click.self="showDetailModal = false">
      <div class="modal-card product-config-modal glass-card">
        <div class="modal-header modal-header-lg">
          <div>
            <h3>{{ selectedProduct.name }}</h3>
            <p class="modal-subtitle">Tùy chỉnh size, topping và ghi chú trước khi thêm vào giỏ.</p>
          </div>
          <button class="icon-clear close-modal-btn" @click="showDetailModal = false">×</button>
        </div>

        <div class="config-section">
          <div class="section-header">
            <label>Chọn size</label>
          </div>
          <div class="size-segment">
            <button
              v-for="size in selectedProduct.sizes"
              :key="size.sizeId"
              :class="['size-pill', { active: tempCustomization.sizeId === size.sizeId }]"
              @click="tempCustomization.sizeId = size.sizeId; tempCustomization.sizeName = size.sizeName"
            >
              <span class="size-pill-label">{{ size.sizeName }}</span>
            </button>
          </div>
        </div>

        <div class="config-section">
          <div class="section-header">
            <label>Chọn topping</label>
            <span class="section-meta">{{ tempCustomization.extras.length }} đã chọn</span>
          </div>
          <div class="topping-list">
            <label
              v-for="extra in extrasOptions"
              :key="extra.toppingId"
              :class="['topping-card', { active: tempCustomization.extras.some((item) => item.toppingId === extra.toppingId) }]"
            >
              <input v-model="tempCustomization.extras" type="checkbox" :value="extra" />
              <div class="topping-copy">
                <strong>{{ extra.toppingName }}</strong>
                <span>+{{ formatPrice(extra.price) }}đ</span>
              </div>
              <span class="topping-check">
                {{ tempCustomization.extras.some((item) => item.toppingId === extra.toppingId) ? 'Đã chọn' : 'Chọn' }}
              </span>
            </label>
          </div>
        </div>

        <div class="config-section">
          <div class="section-header">
            <label>Ghi chú thêm</label>
          </div>
          <textarea
            v-model="tempCustomization.notes"
            rows="3"
            class="config-notes"
            placeholder="Ví dụ: ít đá, ít ngọt, không lấy ống hút..."
          />
        </div>

        <div class="modal-footer config-footer">
          <div class="price-block">
            <span>Tạm tính</span>
            <strong>{{ formatPrice(tempTotalPrice) }}đ</strong>
          </div>
          <button class="btn-primary add-cart-btn" @click="confirmAddToCart">Thêm vào giỏ</button>
        </div>
      </div>
    </div>

    <div v-if="showAddCustomerModal" class="modal-overlay" @click.self="showAddCustomerModal = false">
      <div class="modal-card glass-card">
        <div class="modal-header">
          <h3>Thêm khách hàng</h3>
          <button class="icon-clear" @click="showAddCustomerModal = false">×</button>
        </div>

        <div class="form-group">
          <label>Họ và tên</label>
          <input v-model="newCustomer.fullName" type="text" placeholder="Nhập họ tên..." />
        </div>

        <div class="form-group">
          <label>Số điện thoại</label>
          <input v-model="newCustomer.phoneNumber" type="text" placeholder="Nhập số điện thoại..." />
        </div>

        <div class="modal-footer">
          <button class="btn-secondary" @click="showAddCustomerModal = false">Đóng</button>
          <button class="btn-primary" @click="handleAddCustomer">Tạo khách hàng</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { ClipboardList, Coffee, CreditCard, LogOut, Search, ShoppingCart, Trash2 } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import api from '../api';
import customerApi from '../api/customer';
import promotionApi from '../api/promotion';
import { store } from '../store';

const router = useRouter();
const fullName = ref(localStorage.getItem('fullName') || 'Nhân viên');
const products = ref([]);
const categories = ref([]);
const extrasOptions = ref([]);
const availablePromotions = ref([]);
const selectedCategory = ref(null);
const searchQuery = ref('');
const cart = ref([...(store.currentOrder.cart || [])]);

const searchPhone = ref(store.currentOrder.selectedCustomer?.phoneNumber || '');
const selectedCustomer = ref(store.currentOrder.selectedCustomer || null);
const showAddCustomerModal = ref(false);
const newCustomer = ref({ fullName: '', phoneNumber: '' });

const voucherCode = ref(store.currentOrder.promotionCode || '');
const discountAmount = ref(Number(store.currentOrder.discountAmount || 0));
const promotionType = ref(store.currentOrder.promotionType || '');
const promotionValue = ref(Number(store.currentOrder.promotionValue || 0));
const selectedPromotion = ref(null);

const showDetailModal = ref(false);
const selectedProduct = ref(null);
const tempCustomization = ref({
  sizeId: null,
  sizeName: 'M',
  extras: [],
  notes: ''
});

const currentDate = computed(() => new Date().toLocaleDateString('vi-VN'));

const filteredProducts = computed(() => {
  const keyword = searchQuery.value.trim().toLowerCase();
  return products.value.filter((product) => {
    const matchCategory = selectedCategory.value ? product.categoryId === selectedCategory.value : true;
    const matchKeyword = keyword ? product.name.toLowerCase().includes(keyword) : true;
    return matchCategory && matchKeyword;
  });
});

const subtotal = computed(() => cart.value.reduce((sum, item) => sum + item.price * item.quantity, 0));
const totalPrice = computed(() => Math.max(0, subtotal.value - discountAmount.value));

const tempTotalPrice = computed(() => {
  if (!selectedProduct.value) return 0;
  const extrasCost = tempCustomization.value.extras.reduce((sum, item) => sum + item.price, 0);
  return selectedProduct.value.price + extrasCost;
});

const fetchData = async () => {
  try {
    const [productData, categoryData, toppingData, sizeData] = await Promise.all([
      api.get('/Products'),
      api.get('/Categories'),
      api.get('/Lookups/toppings'),
      api.get('/Lookups/sizes')
    ]);

    const sharedSizes = (sizeData || []).map((size) => ({
      sizeId: size.sizeId,
      sizeName: size.sizeName,
      priceAdd: 0
    }));

    products.value = (productData || [])
      .filter((product) => product.isActive)
      .map((product) => ({ ...product, sizes: sharedSizes }));
    categories.value = (categoryData || []).filter((category) => category.isActive);
    extrasOptions.value = (toppingData || []).filter((topping) => topping.isActive);
  } catch (error) {
    console.error('Failed to load cashier data:', error);
    Swal.fire('Lỗi', 'Không thể tải dữ liệu bán hàng.', 'error');
  }
};

const loadAvailablePromotions = async () => {
  if (subtotal.value <= 0) {
    availablePromotions.value = [];
    clearPromotion();
    return;
  }

  try {
    const promotions = await promotionApi.getAvailable(subtotal.value);
    availablePromotions.value = promotions || [];

    if (voucherCode.value) {
      const matched = availablePromotions.value.find((item) => item.code === voucherCode.value);
      if (matched) {
        selectedPromotion.value = matched;
        applyPromotion(matched, false);
      } else {
        clearPromotion();
      }
    }
  } catch (error) {
    console.error('Failed to load available promotions:', error);
    availablePromotions.value = [];
  }
};

const openProduct = (product) => {
  selectedProduct.value = product;
  tempCustomization.value = {
    sizeId: product.sizes[0]?.sizeId ?? null,
    sizeName: product.sizes[0]?.sizeName ?? 'M',
    extras: [],
    notes: ''
  };
  showDetailModal.value = true;
};

const confirmAddToCart = () => {
  const customization = {
    sizeId: tempCustomization.value.sizeId,
    sizeName: tempCustomization.value.sizeName,
    extras: [...tempCustomization.value.extras],
    notes: tempCustomization.value.notes
  };

  const existingItem = cart.value.find((item) =>
    item.productId === selectedProduct.value.productId &&
    item.customization.sizeId === customization.sizeId &&
    item.customization.notes === customization.notes &&
    JSON.stringify(item.customization.extras.map((extra) => extra.toppingId).sort()) ===
      JSON.stringify(customization.extras.map((extra) => extra.toppingId).sort())
  );

  if (existingItem) {
    existingItem.quantity += 1;
  } else {
    cart.value.push({
      ...selectedProduct.value,
      cartId: Date.now() + Math.random(),
      quantity: 1,
      price: tempTotalPrice.value,
      customization
    });
  }

  showDetailModal.value = false;
};

const increaseQty = (item) => {
  item.quantity += 1;
};

const decreaseQty = (item) => {
  if (item.quantity > 1) {
    item.quantity -= 1;
    return;
  }

  removeFromCart(item);
};

const removeFromCart = (item) => {
  cart.value = cart.value.filter((current) => current.cartId !== item.cartId);
};

const clearSelectedCustomer = () => {
  selectedCustomer.value = null;
  searchPhone.value = '';
};

const clearPromotion = () => {
  selectedPromotion.value = null;
  voucherCode.value = '';
  discountAmount.value = 0;
  promotionType.value = '';
  promotionValue.value = 0;
};

const clearCart = () => {
  cart.value = [];
  clearSelectedCustomer();
  availablePromotions.value = [];
  clearPromotion();
  store.clearOrder();
};

const searchCustomer = async () => {
  if (!searchPhone.value.trim()) return;

  try {
    const results = await customerApi.getAll({ keyword: searchPhone.value.trim() });
    if (!results || results.length === 0) {
      newCustomer.value = { fullName: '', phoneNumber: searchPhone.value.trim() };
      showAddCustomerModal.value = true;
      await Swal.fire('Không tìm thấy', 'Chưa có khách hàng này trong hệ thống.', 'info');
      return;
    }

    selectedCustomer.value = results.find((item) => item.phoneNumber === searchPhone.value.trim()) || results[0];
    Swal.fire('Thành công', `Đã chọn ${selectedCustomer.value.fullName}.`, 'success');
  } catch (error) {
    console.error('Failed to search customer:', error);
    Swal.fire('Lỗi', error?.message || 'Không thể tìm khách hàng.', 'error');
  }
};

const openAddCustomerModal = () => {
  newCustomer.value = { fullName: '', phoneNumber: searchPhone.value.trim() };
  showAddCustomerModal.value = true;
};

const handleAddCustomer = async () => {
  if (!newCustomer.value.fullName.trim() || !newCustomer.value.phoneNumber.trim()) {
    Swal.fire('Lỗi', 'Vui lòng nhập đủ họ tên và số điện thoại.', 'error');
    return;
  }

  try {
    const customerId = await customerApi.create({
      fullName: newCustomer.value.fullName.trim(),
      phoneNumber: newCustomer.value.phoneNumber.trim()
    });
    selectedCustomer.value = await customerApi.getById(customerId);
    searchPhone.value = selectedCustomer.value.phoneNumber;
    showAddCustomerModal.value = false;
    Swal.fire('Thành công', 'Đã tạo khách hàng mới.', 'success');
  } catch (error) {
    console.error('Failed to create customer:', error);
    Swal.fire('Lỗi', error?.message || 'Không thể tạo khách hàng.', 'error');
  }
};

const calculatePromotionDiscount = (promotion) => {
  if (!promotion) return 0;
  const isPercentage = String(promotion.discountType || '').toLowerCase() === 'percentage';
  const rawDiscount = isPercentage
    ? Math.round((subtotal.value * Number(promotion.discountValue || 0)) / 100)
    : Number(promotion.discountValue || 0);

  return Math.min(subtotal.value, rawDiscount);
};

const applyPromotion = (promotion, notify = true) => {
  selectedPromotion.value = promotion;
  voucherCode.value = promotion.code;
  promotionType.value = promotion.discountType || '';
  promotionValue.value = Number(promotion.discountValue || 0);
  discountAmount.value = calculatePromotionDiscount(promotion);

  if (notify) {
    Swal.fire('Thành công', `Đã áp dụng mã ${promotion.code}.`, 'success');
  }
};

const selectPromotion = (promotion) => {
  applyPromotion(promotion);
};

const handleCheckout = () => {
  if (cart.value.length === 0) return;

  store.setOrder({
    cart: cart.value,
    selectedCustomer: selectedCustomer.value,
    subtotal: subtotal.value,
    discountAmount: discountAmount.value,
    promotionCode: voucherCode.value.trim().toUpperCase(),
    promotionType: promotionType.value,
    promotionValue: promotionValue.value,
    totalPrice: totalPrice.value
  });

  router.push('/payment');
};

const handleLogout = () => {
  localStorage.removeItem('token');
  localStorage.removeItem('fullName');
  localStorage.removeItem('role');
  localStorage.removeItem('userId');
  router.push('/login');
};

const handleProductImageError = (event) => {
  const image = event?.target;
  if (!image) {
    return;
  }

  image.style.display = 'none';
  const wrapper = image.parentElement;
  if (!wrapper || wrapper.querySelector('.product-icon')) {
    return;
  }

  const fallback = document.createElement('div');
  fallback.className = 'product-icon';
  fallback.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M10 2v2"/><path d="M14 2v2"/><path d="M16 8a4 4 0 0 1-8 0V6h8z"/><path d="M6 10h12"/><path d="M7 22h10"/><path d="M8 10v8a4 4 0 0 0 8 0v-8"/><path d="M5 6h14"/></svg>';
  wrapper.appendChild(fallback);
};

const formatPrice = (value) => Number(value || 0).toLocaleString('vi-VN');

const formatPromotionValue = (promotion) => {
  const isPercentage = String(promotion.discountType || '').toLowerCase() === 'percentage';
  return isPercentage
    ? `Giảm ${Number(promotion.discountValue || 0)}%`
    : `Giảm ${formatPrice(promotion.discountValue || 0)}đ`;
};

watch(subtotal, async () => {
  await loadAvailablePromotions();
}, { immediate: true });

watch(
  [cart, selectedCustomer, subtotal, discountAmount, voucherCode, promotionType, promotionValue, totalPrice],
  () => {
    store.setOrder({
      cart: cart.value,
      selectedCustomer: selectedCustomer.value,
      subtotal: subtotal.value,
      discountAmount: discountAmount.value,
      promotionCode: voucherCode.value,
      promotionType: promotionType.value,
      promotionValue: promotionValue.value,
      totalPrice: totalPrice.value
    });
  },
  { deep: true }
);

onMounted(fetchData);
</script>

<style scoped>
.cashier-page { min-height: 100vh; display: grid; grid-template-columns: 240px 1fr 380px; gap: 1rem; padding: 1rem; background: #f5f0eb; }
.glass-card { background: rgba(255,255,255,.78); backdrop-filter: blur(18px); border: 1px solid rgba(255,255,255,.5); border-radius: 1.25rem; box-shadow: 0 12px 32px rgba(0,0,0,.05); }
.sidebar, .cart-panel { padding: 1.25rem; }
.brand h1 { font-size: 1.8rem; color: var(--primary); }
.brand h1 span { color: var(--secondary); }
.brand p { color: #6b7280; margin-top: .35rem; }
.sidebar-actions { display: flex; flex-direction: column; gap: .75rem; margin: 1.25rem 0; }
.side-link { display: flex; align-items: center; gap: .6rem; text-decoration: none; color: var(--primary); background: #f4ece2; padding: .85rem 1rem; border-radius: .9rem; border: none; cursor: pointer; font-weight: 600; }
.side-link.logout { background: #fff1f2; color: #b91c1c; }
.category-list { display: flex; flex-direction: column; gap: .6rem; }
.category-btn { border: none; background: transparent; text-align: left; padding: .8rem 1rem; border-radius: .9rem; cursor: pointer; font-weight: 600; color: #4b5563; }
.category-btn.active, .category-btn:hover { background: #2c1810; color: white; }
.main-panel { display: flex; flex-direction: column; gap: 1rem; }
.topbar { display: flex; justify-content: space-between; align-items: center; padding: 1rem 1.25rem; }
.search-box { display: flex; align-items: center; gap: .75rem; background: white; padding: .8rem 1rem; border-radius: 1rem; min-width: 320px; }
.search-box input { border: none; outline: none; width: 100%; }
.date-box { color: #6b7280; font-weight: 600; }
.product-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(170px, 1fr)); gap: 1rem; }
.product-card { padding: 1rem; display: flex; flex-direction: column; align-items: center; gap: .7rem; border: none; cursor: pointer; text-align: center; min-height: 220px; }
.product-image-wrap { width: 100%; aspect-ratio: 1 / 1; border-radius: 1rem; overflow: hidden; background: #fdf5eb; display: flex; align-items: center; justify-content: center; }
.product-image { width: 100%; height: 100%; object-fit: cover; display: block; }
.product-icon { width: 72px; height: 72px; border-radius: 50%; background: #fdf5eb; display: flex; align-items: center; justify-content: center; color: var(--secondary); }
.product-icon :deep(svg) { width: 28px; height: 28px; }
.product-name { display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; min-height: 2.8em; color: var(--primary); }
.product-card span { color: var(--secondary); font-weight: 700; }
.cart-header, .modal-header, .modal-footer, .summary-box div, .selected-customer { display: flex; justify-content: space-between; align-items: center; }
.customer-box, .promotion-box, .summary-box { margin-bottom: 1rem; }
.customer-box label, .promotion-header label { display: block; font-weight: 700; margin-bottom: .5rem; }
.customer-search { display: flex; gap: .5rem; }
.customer-search input, .form-group input, .form-group textarea, .config-notes { width: 100%; border: 1px solid #d1d5db; border-radius: .9rem; padding: .8rem .95rem; outline: none; font-family: inherit; }
.selected-customer { background: #fdf7ee; border: 1px solid #e6c9a6; border-radius: .95rem; padding: .85rem 1rem; }
.selected-customer p, .brand p, .note, .helper-text { margin: 0; font-size: .85rem; color: #6b7280; }
.helper-text { margin-top: .55rem; line-height: 1.45; }
.cart-list { display: flex; flex-direction: column; gap: .85rem; min-height: 180px; max-height: 360px; overflow: auto; }
.cart-item { display: flex; justify-content: space-between; gap: .75rem; background: white; border-radius: 1rem; padding: .9rem; }
.qty-box { display: flex; align-items: center; gap: .35rem; }
.qty-box button, .remove-btn, .icon-clear, .clear-btn, .clear-promotion-btn { border: none; background: #f3f4f6; height: 30px; border-radius: .6rem; cursor: pointer; font-family: inherit; }
.qty-box button, .remove-btn, .icon-clear { width: 30px; }
.clear-btn, .clear-promotion-btn { padding: 0 .75rem; }
.remove-btn { color: #b91c1c; display: inline-flex; align-items: center; justify-content: center; }
.icon-clear { background: transparent; font-size: 1.4rem; color: #6b7280; }
.empty-cart, .promotion-empty { text-align: center; color: #9ca3af; padding: 1rem 0; font-size: .92rem; }
.promotion-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: .55rem; }
.promotion-list { display: flex; flex-direction: column; gap: .7rem; max-height: 210px; overflow: auto; }
.promotion-card { display: flex; justify-content: space-between; gap: .75rem; width: 100%; border: 1px solid #eadac5; background: #fffdf9; border-radius: 1rem; padding: .9rem; cursor: pointer; text-align: left; transition: .18s ease; }
.promotion-card:hover { transform: translateY(-1px); box-shadow: 0 8px 20px rgba(44,24,16,.06); }
.promotion-card.active { border-color: #2c1810; background: #f7efe5; }
.promotion-main { display: flex; flex-direction: column; gap: .2rem; }
.promotion-main strong { color: var(--primary); }
.promotion-main span, .promotion-side small { color: #8b7355; font-size: .84rem; }
.promotion-side { display: flex; flex-direction: column; align-items: flex-end; gap: .2rem; }
.promotion-side strong { color: var(--secondary); }
.summary-box { display: flex; flex-direction: column; gap: .55rem; }
.summary-box .total { padding-top: .5rem; border-top: 1px dashed #d1d5db; font-size: 1.05rem; }
.checkout-btn, .btn-primary, .btn-secondary { border: none; border-radius: .95rem; padding: .9rem 1rem; cursor: pointer; font-weight: 700; font-family: inherit; }
.checkout-btn { width: 100%; background: var(--primary); color: white; display: flex; justify-content: center; align-items: center; gap: .5rem; }
.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.small { padding: .8rem .9rem; }
.modal-overlay { position: fixed; inset: 0; background: rgba(34, 24, 16, .38); display: flex; justify-content: center; align-items: center; z-index: 1000; backdrop-filter: blur(7px); }
.modal-card { width: min(560px, 92vw); padding: 1.4rem; }
.product-config-modal { width: min(760px, 94vw); padding: 1.6rem; border-radius: 1.5rem; background: rgba(255,255,255,.9); }
.modal-header-lg { align-items: flex-start; gap: 1rem; margin-bottom: 1rem; }
.modal-subtitle { margin-top: .35rem; color: #6b7280; font-size: .92rem; }
.close-modal-btn { width: 38px; height: 38px; border-radius: 999px; background: #f5f5f4; flex-shrink: 0; }
.config-section { display: flex; flex-direction: column; gap: .8rem; margin-bottom: 1.15rem; }
.section-header { display: flex; justify-content: space-between; align-items: center; gap: 1rem; }
.section-header label { font-size: 1rem; font-weight: 700; color: var(--primary); }
.section-meta { font-size: .84rem; color: #8b7355; background: #f7efe5; padding: .3rem .7rem; border-radius: 999px; }
.size-segment { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: .75rem; }
.size-pill { border: 1px solid #dcc4a4; background: white; color: var(--primary); border-radius: 1rem; padding: 1rem; cursor: pointer; font-weight: 700; transition: .2s ease; }
.size-pill.active { background: #2c1810; color: white; box-shadow: 0 8px 20px rgba(44,24,16,.18); }
.size-pill-label { font-size: 1rem; }
.topping-list { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: .85rem; }
.topping-card { display: grid; grid-template-columns: auto 1fr auto; align-items: center; gap: .85rem; padding: .95rem 1rem; border: 1px solid #e5d5be; border-radius: 1rem; background: #fffdf9; cursor: pointer; transition: .2s ease; }
.topping-card:hover { transform: translateY(-1px); box-shadow: 0 8px 20px rgba(44,24,16,.08); }
.topping-card.active { border-color: #2c1810; background: #f7efe5; }
.topping-card input { width: 18px; height: 18px; accent-color: #2c1810; }
.topping-copy { display: flex; flex-direction: column; gap: .15rem; }
.topping-copy strong { font-size: .95rem; color: var(--primary); }
.topping-copy span { font-size: .88rem; color: #8b7355; }
.topping-check { font-size: .78rem; font-weight: 700; color: #8b7355; background: #fff; padding: .3rem .55rem; border-radius: 999px; border: 1px solid #eadac5; }
.config-notes { min-height: 96px; resize: vertical; background: white; }
.config-footer { margin-top: .4rem; padding-top: 1rem; border-top: 1px solid #eee1d0; }
.price-block { display: flex; flex-direction: column; gap: .2rem; }
.price-block span { font-size: .86rem; color: #8b7355; }
.price-block strong { font-size: 1.7rem; color: var(--secondary); line-height: 1; }
.add-cart-btn { min-width: 180px; padding: 1rem 1.25rem; }
.form-group { display: flex; flex-direction: column; gap: .45rem; margin-bottom: 1rem; }
@media (max-width: 1200px) { .cashier-page { grid-template-columns: 220px 1fr; } .cart-panel { grid-column: 1 / -1; } }
@media (max-width: 760px) { .product-config-modal { width: min(94vw, 94vw); padding: 1.1rem; } .size-segment, .topping-list { grid-template-columns: 1fr; } .config-footer { flex-direction: column; align-items: stretch; gap: .85rem; } .add-cart-btn { width: 100%; } .promotion-card { flex-direction: column; } .promotion-side { align-items: flex-start; } }
</style>
