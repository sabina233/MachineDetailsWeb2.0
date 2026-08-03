<script setup>
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { dashboardApi, zplApi } from '../services/api'
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

const defaultStart = () => new Date(Date.now() - 6 * 86400000).toISOString().slice(0, 10)
const defaultEnd = () => new Date().toISOString().slice(0, 10)

function formatTs(timestamp) {
  return timestamp ? String(timestamp).replace('T', ' ').slice(0, 19) : '-'
}

async function load({ start, end } = {}) {
  error.value = ''
  const s = start || defaultStart()
  const e = end || defaultEnd()
  try {
    const [history, daily] = await Promise.all([
      zplApi.getHistory(userName.value, s, e, page.value, pageSize),
      dashboardApi.getZplSummary(e)
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
  <div class="page" data-test="zpl-detail">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="router.push('/zpl')">返回列表</button>
      <h1>ZPL 用户详情 · {{ userName }}</h1>
      <DateRangeFilter @query="load" />
    </header>
    <p v-if="error" class="error">{{ error }}</p>

    <section class="card">
      <h2>日结案总产量</h2>
      <div class="summary-total__value">{{ dailyTotal }}</div>
    </section>

    <section class="card">
      <h2>每日订单趋势</h2>
      <TrendChart :labels="trend.labels" :values="trend.values" label="订单数" />
    </section>

    <section class="card">
      <h2>历史记录</h2>
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
            <td>{{ row.orderNo }}</td>
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
      <div class="pager">
        <button type="button" class="btn btn-ghost" :disabled="page <= 1" @click="changePage(page - 1)">
          上一页
        </button>
        <span>第 {{ page }} / {{ totalPages }} 页</span>
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
