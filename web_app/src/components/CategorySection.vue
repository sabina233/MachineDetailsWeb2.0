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
  <section class="category-section card" data-test="category-section" :data-category="category">
    <header class="category-section__header">
      <h2>{{ title }}</h2>
      <div class="category-section__meta">
        <span>总产量 <strong data-test="category-total">{{ summary.totalQuantity ?? 0 }}</strong></span>
        <span v-if="category === 'zpl'">{{ summary.userCount ?? 0 }} 名用户</span>
        <span v-else>{{ summary.machineCount ?? 0 }} 台机台</span>
      </div>
      <button type="button" class="btn" data-test="open-details" @click="emit('open-details', category)">
        查看详情
      </button>
    </header>
    <ProductionTable
      :rows="category === 'zpl' ? summary.users || [] : summary.machines || []"
      :category="category"
    />
  </section>
</template>
