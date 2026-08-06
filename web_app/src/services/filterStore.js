import { reactive, watch } from 'vue'

const PREFIX = 'machinedetails.filter.'

const today = () => new Date().toISOString().slice(0, 10)
const daysAgo = (n) => new Date(Date.now() - n * 86400000).toISOString().slice(0, 10)

function load(key, fallback) {
  try {
    const raw = localStorage.getItem(PREFIX + key)
    return raw ? JSON.parse(raw) : fallback
  } catch {
    return fallback
  }
}

function save(key, value) {
  localStorage.setItem(PREFIX + key, JSON.stringify(value))
}

// 全站统一的时间段筛选（默认近 7 天），刷新后从 localStorage 恢复
export const filters = {
  range: reactive(load('range', { start: daysAgo(6), end: today() }))
}

export function initFilterStore() {
  watch(
    filters.range,
    (value) => save('range', { start: value.start, end: value.end }),
    { deep: true }
  )
  save('range', { start: filters.range.start, end: filters.range.end })
}
