<script setup>
import ProductionTable from './ProductionTable.vue'

defineProps({
  category: { type: String, required: true },
  title: { type: String, required: true },
  summary: { type: Object, required: true }
})

const emit = defineEmits(['open-details'])
</script>

<template>
  <section class="category-section card fade-up" data-test="category-section" :data-category="category">
    <header class="category-section__header">
      <h2>{{ title }}</h2>
      <div class="category-section__meta">
        <span>总产量 <strong data-test="category-total">{{ summary.totalQuantity ?? 0 }}</strong></span>
        <span v-if="category === 'zpl'" class="category-tag">{{ summary.userCount ?? 0 }} 名用户</span>
        <span v-else class="category-tag">{{ summary.machineCount ?? 0 }} 台机台</span>
      </div>
      <button type="button" class="btn" data-test="open-details" @click="emit('open-details', category)">
        查看详情
        <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="9 18 15 12 9 6" />
        </svg>
      </button>
    </header>
    <ProductionTable
      :rows="category === 'zpl' ? summary.users || [] : summary.machines || []"
      :category="category"
    />
  </section>
</template>
