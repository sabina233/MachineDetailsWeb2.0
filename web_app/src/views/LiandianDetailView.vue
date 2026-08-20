<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { machineApi } from '../services/api'
import { filters } from '../services/filterStore'
import TrendChart from '../components/TrendChart.vue'
import DateRangeFilter from '../components/DateRangeFilter.vue'

const route = useRoute()
const router = useRouter()
const machineName = ref(route.query.machineName || '')
const current = ref(null)
const history = ref([])
const orderChart = ref({ labels: [], values: [] })
const brandChart = ref({ labels: [], values: [] })
const error = ref('')

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
      <button type="button" class="btn btn-ghost" @click="goBack">返回机台</button>
      <h1>机台详情 · {{ machineName }}</h1>
      <DateRangeFilter
        :start="filters.range.start"
        :end="filters.range.end"
        @query="onRange"
      />
    </header>
    <p v-if="error" class="error">{{ error }}</p>

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
        >
          {{ current.state === 1 ? '生产中' : '已停机' }}
        </span>
        <span class="stat-card__sub">
          速度 {{ current?.speed ?? '—' }} / 设定 {{ current?.setSpeed ?? '—' }} · {{ formatTs(current?.timestamp) }}
        </span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">总产量</span>
        <span class="stat-card__value">{{ totalQuantity }}</span>
        <span class="stat-card__sub">{{ history.length }} 个订单任务</span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">平均产能</span>
        <span class="stat-card__value">{{ fmtCapacity(capacity) }}</span>
        <span class="stat-card__sub">有效时长 {{ fmtHours(totalHours) }}</span>
      </div>
    </section>

    <section class="card">
      <h2 class="card__title">订单产量趋势</h2>
      <TrendChart :labels="orderChart.labels" :values="orderChart.values" label="产量" />
    </section>

    <section class="card">
      <h2 class="card__title">品牌产量</h2>
      <TrendChart :labels="brandChart.labels" :values="brandChart.values" label="产量" />
    </section>

    <section class="card">
      <h2 class="card__title">订单历史</h2>
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
          <tr v-for="row in history" :key="row.taskName">
            <td>{{ row.taskName }}</td>
            <td>{{ formatTs(row.startTime) }}</td>
            <td>{{ formatTs(row.endTime) }}</td>
            <td>{{ row.productQuantity }}</td>
            <td>{{ row.unitsPerHour ?? '—' }}</td>
            <td>{{ row.durationHours ? `${row.durationHours.toFixed(1)} 小时` : '—' }}</td>
          </tr>
          <tr v-if="!history.length">
            <td colspan="6" class="empty">暂无数据</td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
