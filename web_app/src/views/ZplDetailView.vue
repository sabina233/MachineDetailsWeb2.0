<script setup>
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { dashboardApi, zplApi } from '../services/api'
import { filters } from '../services/filterStore'
import TrendChart from '../components/TrendChart.vue'
import DateRangeFilter from '../components/DateRangeFilter.vue'

const route = useRoute()
const router = useRouter()
const userName = ref(route.query.userName || '')
const rows = ref([])
const totalPages = ref(1)
const page = ref(1)
const pageSize = 10
const dailyTotal = ref(0)
const trend = ref({ labels: [], values: [] })
const error = ref('')

function formatTs(timestamp) {
  return timestamp ? String(timestamp).replace('T', ' ').slice(0, 19) : '-'
}

async function load({ start, end } = {}) {
  error.value = ''
  const s = start || filters.range.start
  const e = end || filters.range.end
  try {
    const [history, daily] = await Promise.all([
      zplApi.getHistory(userName.value, s, e, page.value, pageSize),
      dashboardApi.getZplSummary(s, e)
    ])
    rows.value = history.data || []
    totalPages.value = history.totalPages || 1
    const user = (daily.users || []).find((u) => u.userName === userName.value)
    dailyTotal.value = user ? user.quantity : 0
    trend.value = buildTrend(rows.value)
  } catch {
    error.value = '加载失败，请重试'
  }
}

function onRange({ start, end }) {
  filters.range.start = start
  filters.range.end = end
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
  load()
}

onMounted(load)
</script>

<template>
  <div class="page fade-up" data-test="zpl-detail">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="router.push('/zpl')">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6" />
        </svg>
        返回列表
      </button>
      <h1>ZPL 用户详情 · {{ userName }}</h1>
      <DateRangeFilter
        :start="filters.range.start"
        :end="filters.range.end"
        @query="onRange"
      />
    </header>

    <p v-if="error" class="error">{{ error }}</p>

    <!-- 日结案总产量 -->
    <section class="card fade-up">
      <h2 class="card__title">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
          <polyline points="22 4 12 14.01 9 11.01" />
        </svg>
        日结案总产量
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
        每日订单趋势
      </h2>
      <TrendChart :labels="trend.labels" :values="trend.values" label="订单数" />
    </section>

    <!-- 历史记录 -->
    <section class="card fade-up">
      <h2 class="card__title">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
          <circle cx="12" cy="12" r="10" />
          <polyline points="12 6 12 12 16 14" />
        </svg>
        历史记录
      </h2>
      <div class="table-wrap">
        <table class="production-table" data-test="zpl-history-table">
          <thead>
            <tr>
              <th>订单号</th>
              <th>区域</th>
              <th>开始时间</th>
              <th>结束时间</th>
              <th>时长</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in rows" :key="row.orderNo">
              <td style="font-weight: 650; color: var(--navy);">{{ row.orderNo }}</td>
              <td>{{ row.region || '-' }}</td>
              <td>{{ formatTs(row.startTime) }}</td>
              <td>{{ formatTs(row.endTime) }}</td>
              <td>{{ row.duration }}</td>
            </tr>
            <tr v-if="!rows.length">
              <td colspan="5" class="empty">暂无数据</td>
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
