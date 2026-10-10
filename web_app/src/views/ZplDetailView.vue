<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { dashboardApi, zplApi } from '../services/api'
import { filters } from '../services/filterStore'
import { deduplicateUsers } from '../utils/userUtils'
import { resolveZplRegion } from '../config/regionImages'
import RegionImage from '../components/RegionImage.vue'
import TrendChart from '../components/TrendChart.vue'
import DateRangeFilter from '../components/DateRangeFilter.vue'

const route = useRoute()
const router = useRouter()

// 模式判断：'merged' (区域合并) | 'user' (单用户)
const currentMode = ref(route.query.mode === 'merged' ? 'merged' : (route.query.userName ? 'user' : 'merged'))
const selectedUser = ref(route.query.userName || '')
const region = ref(route.query.region || '')
const regionFlag = ref(route.query.flag || '')

const rawRegionsList = computed(() => {
  return route.query.rawRegions ? String(route.query.rawRegions).split(',').filter(Boolean) : []
})

const queryUserNames = computed(() => {
  return route.query.userNames ? String(route.query.userNames).split(',').filter(Boolean) : []
})

const regionUsers = ref([])
const rows = ref([])
const totalPages = ref(1)
const page = ref(1)
const pageSize = 10
const dailyTotal = ref(0)
const trend = ref({ labels: [], values: [] })
const error = ref('')
const loading = ref(false)

// 缓存区域合并模式下的全量历史数据
const mergedAllRows = ref([])

function formatTs(timestamp) {
  return timestamp ? String(timestamp).replace('T', ' ').slice(0, 19) : '-'
}

async function load({ start, end } = {}) {
  error.value = ''
  loading.value = true
  const s = start || filters.range.start
  const e = end || filters.range.end

  try {
    const daily = await dashboardApi.getZplSummary(s, e)
    const allSummaryUsers = deduplicateUsers(daily.users || [])

    // 匹配属于当前区域的所有用户
    if (region.value) {
      regionUsers.value = allSummaryUsers.filter((u) => {
        const info = resolveZplRegion(u.region)
        const matchCountry = info.country === region.value
        const matchRaw = rawRegionsList.value.some((r) => String(u.region || '').trim().toLowerCase() === r.toLowerCase())
        const matchName = queryUserNames.value.some((name) => name.toLowerCase() === u.userName.toLowerCase())
        return matchCountry || matchRaw || matchName
      })

      // 如果未传入 flag，自动解析补充
      if (!regionFlag.value && regionUsers.value.length) {
        regionFlag.value = resolveZplRegion(regionUsers.value[0].region).flag
      }
    } else if (selectedUser.value) {
      const found = allSummaryUsers.find((u) => u.userName.toLowerCase() === selectedUser.value.toLowerCase())
      if (found) {
        const info = resolveZplRegion(found.region)
        region.value = info.country
        regionFlag.value = info.flag
        regionUsers.value = [found]
      }
    }

    if (currentMode.value === 'merged') {
      await loadMergedData(s, e)
    } else {
      await loadSingleUserData(s, e)
    }
  } catch (err) {
    console.error(err)
    error.value = '加载失败，请重试'
  } finally {
    loading.value = false
  }
}

// 1. 加载单用户数据
async function loadSingleUserData(s, e) {
  const targetName = selectedUser.value || (regionUsers.value[0]?.userName ?? '')
  if (!targetName) return

  const [history, daily] = await Promise.all([
    zplApi.getHistory(targetName, s, e, page.value, pageSize),
    dashboardApi.getZplSummary(s, e)
  ])

  // 大小写不敏感汇总该用户的结案总量
  const targetLower = targetName.trim().toLowerCase()
  const matchedUsers = (daily.users || []).filter(
    (u) => (u.userName || '').trim().toLowerCase() === targetLower
  )
  dailyTotal.value = matchedUsers.reduce((sum, u) => sum + (Number(u.quantity) || 0), 0)

  rows.value = history.data || []
  totalPages.value = history.totalPages || 1
  trend.value = buildTrend(rows.value)
}

// 2. 加载区域合并数据
async function loadMergedData(s, e) {
  // 合并计算该区域内所有用户的总结案量
  dailyTotal.value = regionUsers.value.reduce((sum, u) => sum + (Number(u.quantity) || 0), 0)

  // 批量拉取区域内所有用户的历史订单记录并前端合并
  const usersToFetch = regionUsers.value.length ? regionUsers.value : (queryUserNames.value.map(name => ({ userName: name })))
  
  if (!usersToFetch.length) {
    rows.value = []
    totalPages.value = 1
    trend.value = { labels: [], values: [] }
    return
  }

  const historyResults = await Promise.all(
    usersToFetch.map((u) =>
      zplApi.getHistory(u.userName, s, e, 1, 50).catch(() => ({ data: [] }))
    )
  )

  const combined = []
  for (let i = 0; i < historyResults.length; i++) {
    const u = usersToFetch[i]
    const data = historyResults[i]?.data || []
    for (const item of data) {
      combined.push({
        ...item,
        userName: u.userName
      })
    }
  }

  // 按订单最新时间降序排列
  combined.sort((a, b) => new Date(b.endTime || b.startTime || 0) - new Date(a.endTime || a.startTime || 0))
  mergedAllRows.value = combined

  trend.value = buildTrend(combined)
  totalPages.value = Math.max(1, Math.ceil(combined.length / pageSize))
  rows.value = combined.slice((page.value - 1) * pageSize, page.value * pageSize)
}

function onRange({ start, end }) {
  filters.range.start = start
  filters.range.end = end
  page.value = 1
  load()
}

function buildTrend(historyRows) {
  const byDay = new Map()
  for (const row of historyRows) {
    const day = String(row.startTime || '').slice(0, 10)
    if (!day) continue
    byDay.set(day, (byDay.get(day) || 0) + 1)
  }
  return {
    labels: [...byDay.keys()].sort(),
    values: [...byDay.keys()].sort().map((d) => byDay.get(d))
  }
}

function changePage(next) {
  if (next < 1 || next > totalPages.value || next === page.value) return
  page.value = next
  if (currentMode.value === 'merged') {
    rows.value = mergedAllRows.value.slice((page.value - 1) * pageSize, page.value * pageSize)
  } else {
    load()
  }
}

// 切换到区域合并展示模式
function switchToMerged() {
  currentMode.value = 'merged'
  page.value = 1
  load()
}

// 切换到特定用户展示模式
function switchToUser(userName) {
  currentMode.value = 'user'
  selectedUser.value = userName
  page.value = 1
  load()
}

function goBack() {
  if (window.history.state?.back) {
    router.back()
  } else {
    router.push('/zpl/users')
  }
}

onMounted(load)
</script>

<template>
  <div class="page fade-up" data-test="zpl-detail">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="goBack">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6" />
        </svg>
        返回区域列表
      </button>

      <!-- 顶部标题：区分区域合并看板与单用户详情 -->
      <h1 v-if="currentMode === 'merged'" style="display: flex; align-items: center; gap: 8px;">
        <RegionImage v-if="regionFlag" :code="regionFlag" :name="region" size="sm" />
        ZPL 区域合并看板 · {{ region }}
        <span class="region-card__code">合并显示该区域所有数据 (共 {{ regionUsers.length }} 人)</span>
      </h1>
      <h1 v-else style="display: flex; align-items: center; gap: 8px;">
        <RegionImage v-if="regionFlag" :code="regionFlag" :name="region" size="sm" />
        ZPL 用户详情 · {{ selectedUser }}
        <span v-if="region" class="region-card__code">{{ region }}</span>
      </h1>

      <div class="topbar__actions">
        <DateRangeFilter
          :start="filters.range.start"
          :end="filters.range.end"
          @query="onRange"
        />
        <button type="button" class="btn" :disabled="loading" @click="load">刷新</button>
      </div>
    </header>

    <p v-if="error" class="error">{{ error }}</p>

    <!-- 区域内模式切换工具栏：在“合并全部”与具体用户之间随时切换 -->
    <div v-if="regionUsers.length > 1" class="toolbar fade-up">
      <div class="toolbar__left">
        <span style="font-size: 13px; font-weight: 700; color: var(--navy);">查看维度：</span>
        <div class="filter-pills">
          <button
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': currentMode === 'merged' }"
            @click="switchToMerged"
          >
            合并全部 ({{ regionUsers.length }} 人 · {{ dailyTotal }} 件)
          </button>
          <button
            v-for="u in regionUsers"
            :key="u.userName"
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': currentMode === 'user' && selectedUser.toLowerCase() === u.userName.toLowerCase() }"
            @click="switchToUser(u.userName)"
          >
            {{ u.userName }} ({{ u.quantity }})
          </button>
        </div>
      </div>
    </div>

    <!-- 日结案总产量 -->
    <section class="card fade-up">
      <h2 class="card__title">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
          <polyline points="22 4 12 14.01 9 11.01" />
        </svg>
        {{ currentMode === 'merged' ? `${region} 区域结案总产量 (已合并全部用户)` : `${selectedUser} 个人结案总产量` }}
      </h2>
      <div class="summary-total__value">{{ dailyTotal }}</div>
    </section>

    <!-- 每日订单趋势 -->
    <section class="card fade-up">
      <h2 class="card__title">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
          <line x1="18" y1="20" x2="18" y2="10" />
          <line x1="12" y1="20" x2="12" y2="4" />
          <line x1="6" y1="20" x2="6" y2="14" />
        </svg>
        {{ currentMode === 'merged' ? `${region} 区域每日订单趋势 (全员合并)` : `${selectedUser} 每日订单趋势` }}
      </h2>
      <TrendChart :labels="trend.labels" :values="trend.values" label="订单数" />
    </section>

    <!-- 历史记录 -->
    <section class="card fade-up">
      <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; flex-wrap: wrap; gap: 8px;">
        <h2 class="card__title" style="margin-bottom: 0;">
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
            <circle cx="12" cy="12" r="10" />
            <polyline points="12 6 12 12 16 14" />
          </svg>
          历史订单记录
        </h2>
        <span v-if="currentMode === 'merged'" style="font-size: 12px; color: var(--muted); font-weight: 600;">
          已汇集 {{ region }} 区域全部 {{ regionUsers.length }} 名用户的结案订单
        </span>
      </div>

      <div class="table-wrap">
        <table class="production-table" data-test="zpl-history-table">
          <thead>
            <tr>
              <th>订单号</th>
              <th v-if="currentMode === 'merged'">用户</th>
              <th>区域</th>
              <th>开始时间</th>
              <th>结束时间</th>
              <th>时长</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in rows" :key="row.orderNo + (row.userName || '')">
              <td style="font-weight: 650; color: var(--navy);">{{ row.orderNo }}</td>
              <td v-if="currentMode === 'merged'" style="font-weight: 600; color: var(--primary);">
                <span class="user-chip" style="cursor: default; padding: 2px 7px;">
                  {{ row.userName || '-' }}
                </span>
              </td>
              <td>{{ row.region || '-' }}</td>
              <td>{{ formatTs(row.startTime) }}</td>
              <td>{{ formatTs(row.endTime) }}</td>
              <td>{{ row.duration }}</td>
            </tr>
            <tr v-if="!rows.length">
              <td :colspan="currentMode === 'merged' ? 6 : 5" class="empty">暂无数据</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="pager">
        <button type="button" class="btn btn-ghost" :disabled="page <= 1" @click="changePage(page - 1)">
          上一页
        </button>
        <span style="font-weight: 600;">第 {{ page }} / {{ totalPages }} 页</span>
        <button
          type="button"
          class="btn btn-ghost"
          :disabled="page >= totalPages"
          @click="changePage(page + 1)"
        >
          下一页
        </button>
      </div>
    </section>
  </div>
</template>
