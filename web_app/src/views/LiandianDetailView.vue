<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { machineApi } from '../services/api'
import { filters } from '../services/filterStore'
import TrendChart from '../components/TrendChart.vue'
import DateRangeFilter from '../components/DateRangeFilter.vue'
import MachineImage from '../components/MachineImage.vue'
import { getMachineCategoryName } from '../config/machineImages'

const route = useRoute()
const router = useRouter()
const machineName = ref(route.query.machineName || '')
const current = ref(null)
const history = ref([])
const orderChart = ref({ labels: [], values: [] })
const brandChart = ref({ labels: [], values: [] })
const error = ref('')

// 表格分页状态
const page = ref(1)
const pageSize = 10
const totalPages = computed(() => Math.max(1, Math.ceil(history.value.length / pageSize)))
const pagedHistory = computed(() =>
  history.value.slice((page.value - 1) * pageSize, page.value * pageSize)
)

function changePage(next) {
  if (next < 1 || next > totalPages.value || next === page.value) return
  page.value = next
}

async function load({ start, end } = {}) {
  error.value = ''
  const s = start || filters.range.start
  const e = end || filters.range.end
  try {
    const [rt, h, oc, bc] = await Promise.all([
      machineApi.getRealTime(machineName.value),
      machineApi.getHistory(machineName.value, s, e),
      machineApi.getOrderChart(machineName.value, s, e),
      machineApi.getBrandChart(machineName.value, s, e)
    ])
    current.value = (rt || [])[0] || null
    history.value = h || []
    orderChart.value = toChart(oc)
    brandChart.value = toChart(bc, 'name')
    page.value = 1
  } catch {
    error.value = '加载失败，请重试'
  }
}

function onRange({ start, end }) {
  filters.range.start = start
  filters.range.end = end
  load()
}

function goBack() {
  if (route.query.countryCode) {
    router.push(
      `/liandian/${route.params.category || 'roll'}/region/${encodeURIComponent(route.query.countryCode)}`
    )
  } else {
    router.push(`/liandian/${route.params.category || 'roll'}`)
  }
}

const totalQuantity = computed(() =>
  history.value.reduce((sum, row) => sum + (row.productQuantity || 0), 0)
)

const totalHours = computed(() =>
  history.value.reduce((sum, row) => sum + (row.durationHours || 0), 0)
)

const capacity = computed(() =>
  totalHours.value > 0 ? totalQuantity.value / totalHours.value : null
)

function toChart(rows, nameKey = 'date') {
  const list = rows || []
  return {
    labels: list.map((r) => r[nameKey] ?? r.date ?? ''),
    values: list.map((r) => r.productQuantity ?? r.quantity ?? 0)
  }
}

function formatTs(timestamp) {
  return timestamp ? String(timestamp).replace('T', ' ').slice(0, 19) : '-'
}

function fmtHours(hours) {
  return hours > 0 ? `${hours.toFixed(1)} 小时` : '—'
}

function fmtCapacity(value) {
  return value == null ? '—' : `${value.toFixed(1)} /时`
}

onMounted(load)
</script>

<template>
  <div class="page fade-up" data-test="liandian-detail">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="goBack">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6" />
        </svg>
        返回机台
      </button>
      <h1>
        机台详情 · {{ machineName }}
        <span class="category-tag">{{ getMachineCategoryName(machineName) }}</span>
      </h1>
      <DateRangeFilter
        :start="filters.range.start"
        :end="filters.range.end"
        @query="onRange"
      />
    </header>

    <p v-if="error" class="error">{{ error }}</p>

    <!-- 机台状态与指标卡片 -->
    <section class="stats-row">
      <div class="stat-card card">
        <span class="stat-card__label">当前任务</span>
        <span class="stat-card__value stat-card__value--sm">
          {{ current?.taskName || '—' }}
        </span>
        <span
          v-if="current"
          class="status-pill"
          :class="current.state === 1 ? 'status-pill--on' : 'status-pill--off'"
          style="align-self: flex-start;"
        >
          <span class="status-dot" :class="{ 'status-dot--pulse': current.state === 1 }"></span>
          {{ current.state === 1 ? '生产中' : '已停机' }}
        </span>
        <span class="stat-card__sub">
          速度 {{ current?.speed ?? '—' }} / 设定 {{ current?.setSpeed ?? '—' }} · {{ formatTs(current?.timestamp) }}
        </span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">总产量</span>
        <span class="stat-card__value">{{ totalQuantity }}</span>
        <span class="stat-card__sub">{{ history.length }} 个订单任务累计</span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">平均产能</span>
        <span class="stat-card__value">{{ fmtCapacity(capacity) }}</span>
        <span class="stat-card__sub">有效时长 {{ fmtHours(totalHours) }}</span>
      </div>
    </section>

    <!-- 订单产量趋势图 -->
    <section class="card fade-up">
      <h2 class="card__title">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
          <line x1="18" y1="20" x2="18" y2="10" />
          <line x1="12" y1="20" x2="12" y2="4" />
          <line x1="6" y1="20" x2="6" y2="14" />
        </svg>
        订单产量趋势
      </h2>
      <TrendChart :labels="orderChart.labels" :values="orderChart.values" label="产量" />
    </section>

    <!-- 品牌产量趋势图 -->
    <section class="card fade-up">
      <h2 class="card__title">
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
          <rect x="2" y="7" width="20" height="14" rx="2" ry="2" />
          <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16" />
        </svg>
        品牌产量
      </h2>
      <TrendChart :labels="brandChart.labels" :values="brandChart.values" label="产量" />
    </section>

    <!-- 订单历史表格 -->
    <section class="card fade-up">
      <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; flex-wrap: wrap; gap: 8px;">
        <h2 class="card__title" style="margin-bottom: 0;">
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
            <circle cx="12" cy="12" r="10" />
            <polyline points="12 6 12 12 16 14" />
          </svg>
          历史订单记录
        </h2>
        <span style="font-size: 12px; color: var(--muted); font-weight: 600;">
          共 {{ history.length }} 笔订单任务
        </span>
      </div>

      <div class="table-wrap">
        <table class="production-table" data-test="history-table">
          <thead>
            <tr>
              <th>任务</th>
              <th>开始时间</th>
              <th>结束时间</th>
              <th>产量</th>
              <th>产能</th>
              <th>时长</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in pagedHistory" :key="row.taskName + (row.startTime || '')">
              <td style="font-weight: 650; color: var(--navy);">{{ row.taskName }}</td>
              <td>{{ formatTs(row.startTime) }}</td>
              <td>{{ formatTs(row.endTime) }}</td>
              <td style="font-weight: 750; color: var(--navy);">{{ row.productQuantity }}</td>
              <td>{{ row.unitsPerHour ?? '—' }}</td>
              <td>{{ row.durationHours ? `${row.durationHours.toFixed(1)} 小时` : '—' }}</td>
            </tr>
            <tr v-if="!history.length">
              <td colspan="6" class="empty">暂无历史订单数据</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 分页栏 -->
      <div v-if="history.length > pageSize" class="pager">
        <button type="button" class="btn btn-ghost" :disabled="page <= 1" @click="changePage(page - 1)">
          上一页
        </button>
        <span style="font-weight: 600;">第 {{ page }} / {{ totalPages }} 页 (共 {{ history.length }} 笔订单)</span>
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
