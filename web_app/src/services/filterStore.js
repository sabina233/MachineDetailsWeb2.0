import { reactive, watch } from 'vue'

const STORAGE_KEY = 'machinedetails.filter.range'

/**
 * 获取本地时区的 YYYY-MM-DD 日期字符串（避免 toISOString 的 UTC 0时区偏移导致早上变成昨天）
 * @param {Date} date
 * @returns {string}
 */
export function formatLocalDate(date = new Date()) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

/**
 * 获取当前本地日期
 * @returns {string} YYYY-MM-DD
 */
export function getLocalToday() {
  return formatLocalDate(new Date())
}

/**
 * 获取 N 天前的本地日期
 * @param {number} n
 * @returns {string} YYYY-MM-DD
 */
export function getLocalDaysAgo(n) {
  const d = new Date()
  d.setDate(d.getDate() - n)
  return formatLocalDate(d)
}

/**
 * 默认日期范围：当前本地日期的前一周（包含今天在内的近 7 天）
 * 例如今天为 2026-10-20，则范围为 2026-10-14 ~ 2026-10-20
 */
export function getDefaultWeekRange() {
  return {
    start: getLocalDaysAgo(6),
    end: getLocalToday()
  }
}

/**
 * 加载初始日期范围
 * 机制：
 * 1. 优先使用 sessionStorage（当前会话内页面跳转保持自定义日期）
 * 2. 如果存在旧的 localStorage 缓存：
 *    - 若缓存中的结束日期不是今天（即属于历史某天的陈旧缓存），自动废弃并重置为当前日期的最新近一周范围！
 *    - 避免用户数天或数周后打开网页时，日期被死死锁定在过去的某个时间段。
 */
function loadInitialRange() {
  const defaultRange = getDefaultWeekRange()
  const todayStr = getLocalToday()

  try {
    // 1. 优先检查当前会话 sessionStorage
    const sessionRaw = sessionStorage.getItem(STORAGE_KEY)
    if (sessionRaw) {
      const parsed = JSON.parse(sessionRaw)
      if (parsed?.start && parsed?.end) {
        return { start: parsed.start, end: parsed.end }
      }
    }

    // 2. 检查 localStorage
    const localRaw = localStorage.getItem(STORAGE_KEY)
    if (localRaw) {
      const parsed = JSON.parse(localRaw)
      // 如果缓存的结束日期等于今天，且是今天设定的，则可继续使用
      if (parsed?.savedDate === todayStr && parsed?.start && parsed?.end) {
        return { start: parsed.start, end: parsed.end }
      }
    }
  } catch {
    // 解析失败直接回退
  }

  return defaultRange
}

function persistRange(val) {
  try {
    const data = {
      start: val.start,
      end: val.end,
      savedDate: getLocalToday()
    }
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(data))
    localStorage.setItem(STORAGE_KEY, JSON.stringify(data))
  } catch {
    // 忽略存储异常
  }
}

// 全站响应式时间段筛选状态
export const filters = {
  range: reactive(loadInitialRange())
}

/**
 * 重置为当前日期的前一周范围（近 7 天）
 */
export function resetToCurrentWeek() {
  const def = getDefaultWeekRange()
  filters.range.start = def.start
  filters.range.end = def.end
}

/**
 * 初始化监听：当用户在界面上手动查询或修改日期时持久化保存
 */
export function initFilterStore() {
  watch(
    filters.range,
    (value) => {
      persistRange(value)
    },
    { deep: true }
  )
}
