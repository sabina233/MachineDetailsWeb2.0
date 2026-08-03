<script setup>
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { machineApi } from '../services/api'
import ProductionTable from '../components/ProductionTable.vue'
import TrendChart from '../components/TrendChart.vue'
import DateRangeFilter from '../components/DateRangeFilter.vue'

const route = useRoute()
const router = useRouter()
const machineName = ref(route.query.machineName || '')
const realtime = ref([])
const history = ref([])
const orderChart = ref({ labels: [], values: [] })
const brandChart = ref({ labels: [], values: [] })
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
    const [rt, h, oc, bc] = await Promise.all([
      machineApi.getRealTime(machineName.value),
      machineApi.getHistory(machineName.value, s, e),
      machineApi.getOrderChart(machineName.value, s, e),
      machineApi.getBrandChart(machineName.value, s, e)
    ])
    realtime.value = rt || []
    history.value = h || []
    orderChart.value = toChart(oc)
    brandChart.value = toChart(bc, 'name')
  } catch {
    error.value = '加载失败，请重试'
  }
}

function toChart(rows, nameKey = 'date') {
  const list = rows || []
  return {
    labels: list.map((r) => r[nameKey] ?? r.date ?? ''),
    values: list.map((r) => r.productQuantity ?? r.quantity ?? 0)
  }
}

onMounted(load)
</script>

<template>
  <div class="page" data-test="liandian-detail">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="router.push(`/liandian/${route.params.category}`)">
        返回列表
      </button>
      <h1>机台详情 · {{ machineName }}</h1>
      <DateRangeFilter @query="load" />
    </header>
    <p v-if="error" class="error">{{ error }}</p>

    <section class="card">
      <h2>实时状态</h2>
      <ProductionTable :rows="realtime" category="machine" />
    </section>

    <section class="card">
      <h2>订单产量趋势</h2>
      <TrendChart :labels="orderChart.labels" :values="orderChart.values" label="产量" />
    </section>

    <section class="card">
      <h2>品牌产量</h2>
      <TrendChart :labels="brandChart.labels" :values="brandChart.values" label="产量" />
    </section>

    <section class="card">
      <h2>历史记录</h2>
      <table class="production-table" data-test="history-table">
        <thead>
          <tr>
            <th>任务</th>
            <th>开始时间</th>
            <th>结束时间</th>
            <th>产量</th>
            <th>单位小时产量</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in history" :key="row.taskName">
            <td>{{ row.taskName }}</td>
            <td>{{ formatTs(row.startTime) }}</td>
            <td>{{ formatTs(row.endTime) }}</td>
            <td>{{ row.productQuantity }}</td>
            <td>{{ row.unitsPerHour }}</td>
          </tr>
          <tr v-if="!history.length">
            <td colspan="5" class="empty">暂无数据</td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
