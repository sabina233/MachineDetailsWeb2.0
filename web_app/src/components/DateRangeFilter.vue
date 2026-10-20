<script setup>
import { ref, watch } from 'vue'
import { getLocalDaysAgo, getLocalToday } from '../services/filterStore'

const props = defineProps({
  start: { type: String, default: '' },
  end: { type: String, default: '' },
  showQuickReset: { type: Boolean, default: true }
})

const emit = defineEmits(['query'])

const start = ref(props.start || getLocalDaysAgo(6))
const end = ref(props.end || getLocalToday())

// 监听 props 变化，保证外部筛选状态变更时输入框即时同步
watch(
  () => [props.start, props.end],
  ([newStart, newEnd]) => {
    if (newStart) start.value = newStart
    if (newEnd) end.value = newEnd
  }
)

function submit() {
  emit('query', { start: start.value, end: end.value })
}

// 一键切回当前日期的前一周
function setWeek() {
  start.value = getLocalDaysAgo(6)
  end.value = getLocalToday()
  submit()
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
    <button
      v-if="showQuickReset"
      type="button"
      class="btn btn-ghost"
      style="padding: 6px 12px; font-size: 13px;"
      title="一键设定为当前日期的前一周（近7天）"
      @click="setWeek"
    >
      近一周
    </button>
  </div>
</template>
