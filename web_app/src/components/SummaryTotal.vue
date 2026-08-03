<script setup>
defineProps({
  total: { type: Number, default: 0 },
  date: { type: String, default: '' }
})
</script>

<template>
  <section class="summary-total card" data-test="summary-total">
    <span class="summary-total__label">今日总产量</span>
    <span class="summary-total__value">{{ total }}</span>
    <span class="summary-total__date">{{ date }}</span>
  </section>
</template>
