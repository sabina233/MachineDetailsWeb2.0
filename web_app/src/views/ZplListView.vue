<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '../services/api'
import { filters } from '../services/filterStore'
import SummaryTotal from '../components/SummaryTotal.vue'
import ProductionTable from '../components/ProductionTable.vue'
import TrendChart from '../components/TrendChart.vue'
import DateRangeFilter from '../components/DateRangeFilter.vue'

const router = useRouter()
const summary = ref(null)
const error = ref('')

async function load() {
  error.value = ''
  try {
    summary.value = await dashboardApi.getZplSummary(filters.range.start, filters.range.end)
  } catch {
    error.value = '加载失败，请重试'
  }
}

function onRange({ start, end }) {
  filters.range.start = start
  filters.range.end = end
  load()
}

function openDetail(user) {
  router.push({ path: '/zpl/detail', query: { userName: user.userName } })
}

onMounted(load)
</script>

<template>
  <div class="page" data-test="zpl-list">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="router.push('/dashboard')">返回看板</button>
      <h1>ZPL 日结案产量</h1>
      <DateRangeFilter
        :start="filters.range.start"
        :end="filters.range.end"
        @query="onRange"
      />
    </header>
    <p v-if="error" class="error">{{ error }}</p>
    <template v-if="summary">
      <SummaryTotal
        :total="summary.totalQuantity"
        :start="filters.range.start"
        :end="filters.range.end"
      />
      <TrendChart
        :labels="(summary.users || []).map((u) => u.userName)"
        :values="(summary.users || []).map((u) => u.quantity)"
        label="用户产量"
      />
      <ProductionTable :rows="summary.users || []" category="zpl" @row-click="openDetail" />
    </template>
  </div>
</template>
