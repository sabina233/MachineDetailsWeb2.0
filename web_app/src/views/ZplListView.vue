<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '../services/api'
import SummaryTotal from '../components/SummaryTotal.vue'
import ProductionTable from '../components/ProductionTable.vue'
import TrendChart from '../components/TrendChart.vue'

const router = useRouter()
const date = ref(new Date().toISOString().slice(0, 10))
const summary = ref(null)
const error = ref('')

async function load() {
  error.value = ''
  try {
    summary.value = await dashboardApi.getZplSummary(date.value)
  } catch {
    error.value = '加载失败，请重试'
  }
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
      <input type="date" v-model="date" @change="load" />
    </header>
    <p v-if="error" class="error">{{ error }}</p>
    <template v-if="summary">
      <SummaryTotal :total="summary.totalQuantity" :date="date" />
      <TrendChart
        :labels="(summary.users || []).map((u) => u.userName)"
        :values="(summary.users || []).map((u) => u.quantity)"
        label="用户产量"
      />
      <ProductionTable :rows="summary.users || []" category="zpl" @row-click="openDetail" />
    </template>
  </div>
</template>
