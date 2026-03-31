<template>
  <div class="manage-page">
    <div class="page-header">
      <div>
        <h2>Quản lý công thức</h2>
        <p>Thiết lập định lượng nguyên liệu theo từng sản phẩm và kích thước.</p>
      </div>
      <button class="btn btn-primary" @click="saveRecipe" :disabled="saving || !selectedProductId">
        <Save :size="18" />
        {{ saving ? 'Đang lưu...' : 'Lưu công thức' }}
      </button>
    </div>

    <div class="control-card">
      <div class="form-row">
        <div class="form-group">
          <label>Sản phẩm</label>
          <select v-model.number="selectedProductId" class="select-input" @change="loadRecipe">
            <option :value="null">Chọn sản phẩm</option>
            <option v-for="product in products" :key="product.productId" :value="product.productId">
              {{ product.name }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>Tìm nguyên liệu</label>
          <input v-model="ingredientKeyword" type="text" placeholder="Lọc danh sách nguyên liệu..." />
        </div>
      </div>
    </div>

    <div v-if="loading" class="state-box card">
      <LoaderCircle class="spin" :size="28" />
      <span>Đang tải công thức...</span>
    </div>

    <div v-else-if="!selectedProductId" class="state-box card">
      Chọn một sản phẩm để bắt đầu cấu hình công thức.
    </div>

    <div v-else class="recipe-grid">
      <div v-for="size in recipeSizes" :key="size.sizeId" class="recipe-card">
        <div class="recipe-header">
          <div>
            <h3>{{ size.sizeName }}</h3>
            <p>{{ selectedProductName }}</p>
          </div>
          <button class="btn btn-secondary btn-sm" @click="addIngredientRow(size.sizeId)">
            <Plus :size="16" />
            Thêm dòng
          </button>
        </div>

        <div class="line-item header">
          <span>Nguyên liệu</span>
          <span>Đơn vị</span>
          <span>Định lượng</span>
          <span></span>
        </div>

        <div v-for="(item, index) in size.ingredients" :key="`${size.sizeId}-${index}`" class="line-item">
          <select v-model.number="item.ingredientId" class="select-input">
            <option :value="null">Chọn nguyên liệu</option>
            <option v-for="ingredient in filteredIngredientOptions" :key="ingredient.ingredientId" :value="ingredient.ingredientId">
              {{ ingredient.ingredientName }}
            </option>
          </select>
          <span class="unit-box">{{ getIngredientUnit(item.ingredientId) }}</span>
          <input v-model.number="item.quantityNeeded" type="number" min="0" step="0.01" />
          <button class="icon-btn delete" @click="removeIngredientRow(size.sizeId, index)">
            <Trash2 :size="16" />
          </button>
        </div>

        <div v-if="size.ingredients.length === 0" class="empty-row">
          Chưa có nguyên liệu cho size này.
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue';
import { LoaderCircle, Plus, Save, Trash2 } from 'lucide-vue-next';
import Swal from 'sweetalert2';
import api from '../api';
import ingredientApi from '../api/ingredient';
import recipeApi from '../api/recipe';

const loading = ref(false);
const saving = ref(false);
const products = ref([]);
const sizes = ref([]);
const ingredients = ref([]);
const selectedProductId = ref(null);
const ingredientKeyword = ref('');
const recipeSizes = ref([]);

const selectedProductName = computed(() => products.value.find((item) => item.productId === selectedProductId.value)?.name || '');
const filteredIngredientOptions = computed(() => {
  const term = ingredientKeyword.value.trim().toLowerCase();
  return ingredients.value.filter((item) => !term || item.ingredientName.toLowerCase().includes(term));
});

const loadLookups = async () => {
  loading.value = true;
  try {
    const [productData, sizeData, ingredientData] = await Promise.all([
      api.get('/Products'),
      api.get('/Lookups/sizes'),
      ingredientApi.getAll()
    ]);

    products.value = productData || [];
    sizes.value = sizeData || [];
    ingredients.value = ingredientData || [];
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể tải dữ liệu công thức.', 'error');
  } finally {
    loading.value = false;
  }
};

const buildEmptySizes = () =>
  sizes.value.map((size) => ({
    sizeId: size.sizeId,
    sizeName: size.sizeName,
    ingredients: []
  }));

const loadRecipe = async () => {
  if (!selectedProductId.value) {
    recipeSizes.value = [];
    return;
  }

  loading.value = true;
  try {
    const recipe = await recipeApi.getByProduct(selectedProductId.value);
    const mapped = buildEmptySizes();

    recipe?.sizes?.forEach((size) => {
      const target = mapped.find((item) => item.sizeId === size.sizeId);
      if (!target) return;
      target.ingredients = (size.ingredients || []).map((ingredient) => ({
        ingredientId: ingredient.ingredientId,
        quantityNeeded: ingredient.quantityNeeded
      }));
    });

    recipeSizes.value = mapped;
  } catch (error) {
    recipeSizes.value = buildEmptySizes();
    Swal.fire('Lỗi', error?.message || 'Không thể tải chi tiết công thức.', 'error');
  } finally {
    loading.value = false;
  }
};

const addIngredientRow = (sizeId) => {
  const target = recipeSizes.value.find((item) => item.sizeId === sizeId);
  if (!target) return;
  target.ingredients.push({ ingredientId: null, quantityNeeded: 0 });
};

const removeIngredientRow = (sizeId, index) => {
  const target = recipeSizes.value.find((item) => item.sizeId === sizeId);
  if (!target) return;
  target.ingredients.splice(index, 1);
};

const getIngredientUnit = (ingredientId) =>
  ingredients.value.find((item) => item.ingredientId === ingredientId)?.unit || '--';

const saveRecipe = async () => {
  saving.value = true;
  try {
    const hasDuplicateIngredient = recipeSizes.value.some((size) => {
      const ids = size.ingredients.map((item) => item.ingredientId).filter(Boolean);
      return new Set(ids).size !== ids.length;
    });

    if (hasDuplicateIngredient) {
      throw new Error('Một nguyên liệu không được lặp lại nhiều lần trong cùng một size.');
    }

    await recipeApi.saveByProduct(selectedProductId.value, {
      productId: selectedProductId.value,
      sizes: recipeSizes.value.map((size) => ({
        sizeId: size.sizeId,
        ingredients: size.ingredients
          .filter((item) => item.ingredientId && Number(item.quantityNeeded) > 0)
          .map((item) => ({
            ingredientId: item.ingredientId,
            quantityNeeded: Number(item.quantityNeeded)
          }))
      }))
    });

    await loadRecipe();
    Swal.fire('Thành công', 'Đã lưu công thức sản phẩm.', 'success');
  } catch (error) {
    Swal.fire('Lỗi', error?.message || 'Không thể lưu công thức.', 'error');
  } finally {
    saving.value = false;
  }
};

onMounted(loadLookups);
</script>

<style scoped>
.manage-page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; }
.page-header h2 { color: var(--primary); font-size: 2rem; }
.page-header p { color: #6b7280; margin-top: .25rem; }
.card, .control-card, .recipe-card { background: rgba(255,255,255,.82); border: 1px solid rgba(255,255,255,.5); border-radius: 1.25rem; box-shadow: 0 12px 32px rgba(0,0,0,.05); backdrop-filter: blur(16px); }
.control-card { padding: 1.25rem; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.form-group { display: flex; flex-direction: column; gap: .45rem; }
.form-group input, .select-input, .line-item input { width: 100%; border: 1px solid #d1d5db; border-radius: .85rem; padding: .8rem .95rem; background: white; }
.recipe-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; }
.recipe-card { padding: 1.25rem; }
.recipe-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem; }
.recipe-header h3 { color: var(--primary); }
.recipe-header p { color: #6b7280; font-size: .9rem; margin-top: .2rem; }
.line-item { display: grid; grid-template-columns: 1.6fr .7fr .8fr auto; gap: .75rem; align-items: center; margin-bottom: .75rem; }
.line-item.header { color: #6b7280; font-size: .8rem; font-weight: 700; text-transform: uppercase; margin-bottom: .6rem; }
.unit-box { background: #f9fafb; border: 1px solid #e5e7eb; border-radius: .85rem; padding: .8rem .95rem; color: #6b7280; }
.empty-row, .state-box { text-align: center; color: #6b7280; padding: 1.25rem; }
.btn { border: none; border-radius: .85rem; padding: .8rem 1rem; cursor: pointer; font-weight: 700; display: inline-flex; align-items: center; gap: .5rem; }
.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: #efe8df; color: var(--primary); }
.btn-sm { padding: .6rem .8rem; font-size: .88rem; }
.icon-btn { border: none; background: transparent; width: 36px; height: 36px; border-radius: .75rem; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; }
.icon-btn.delete { background: #fee2e2; color: #b91c1c; }
.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (max-width: 960px) {
  .page-header, .form-row { grid-template-columns: 1fr; display: grid; }
  .recipe-grid { grid-template-columns: 1fr; }
  .line-item { grid-template-columns: 1fr; }
}
</style>
