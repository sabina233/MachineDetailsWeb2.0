<script setup>
import { ref } from 'vue'

const props = defineProps({
  start: { type: String, default: '' },
  end: { type: String, default: '' }
})

const emit = defineEmits(['query'])

const today = () => new Date().toISOString().slice(0, 10)
const start = ref(props.start || today())
const end = ref(props.end || today())

function submit() {
  emit('query', { start: start.value, end: end.value })
}
</script>

<template>
  <div class="date-filter" data-test="date-filter">
    <label>
      <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
        <line x1="16" y1="2" x2="16" y2="6" />
        <line x1="8" y1="2" x2="8" y2="6" />
        <line x1="3" y1="10" x2="21" y2="10" />
      </svg>
      开始
      <input v-model="start" type="date" />
    </label>
    <label>
      <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
        <line x1="16" y1="2" x2="16" y2="6" />
        <line x1="8" y1="2" x2="8" y2="6" />
        <line x1="3" y1="10" x2="21" y2="10" />
      </svg>
      结束
      <input v-model="end" type="date" />
    </label>
    <button type="button" class="btn" @click="submit">
      <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="11" cy="11" r="8" />
        <line x1="21" y1="21" x2="16.65" y2="16.65" />
      </svg>
      查询
    </button>
  </div>
</template>
