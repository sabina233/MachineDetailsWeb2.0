<script setup>
defineProps({
  total: { type: Number, default: 0 },
  start: { type: String, default: '' },
  end: { type: String, default: '' }
})

function label(start, end) {
  return start && start === end ? '今日总产量' : '时段总产量'
}

function rangeText(start, end) {
  return start && end && start !== end ? `${start} ~ ${end}` : end
}
</script>

<template>
  <section class="summary-total card" data-test="summary-total">
    <span class="summary-total__label">{{ label(start, end) }}</span>
    <span class="summary-total__value">{{ total }}</span>
    <span class="summary-total__date">{{ rangeText(start, end) }}</span>
  </section>
</template>
