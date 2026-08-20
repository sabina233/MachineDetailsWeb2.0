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
  return start && end && start !== end ? `${start} ~ ${end}` : (end || start || '今日')
}
</script>

<template>
  <section class="summary-total card fade-up" data-test="summary-total">
    <span class="summary-total__label">{{ label(start, end) }}</span>
    <span class="summary-total__value">{{ total }}</span>
    <span class="summary-total__date">
      <svg viewBox="0 0 24 24" width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="display: inline-block; vertical-align: middle; margin-right: 4px;">
        <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
        <line x1="16" y1="2" x2="16" y2="6" />
        <line x1="8" y1="2" x2="8" y2="6" />
        <line x1="3" y1="10" x2="21" y2="10" />
      </svg>
      {{ rangeText(start, end) }}
    </span>
  </section>
</template>
