<script setup>
import { onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { dashboardApi } from '../services/api'
import SummaryTotal from '../components/SummaryTotal.vue'
import ProductionTable from '../components/ProductionTable.vue'
import TrendChart from '../components/TrendChart.vue'

const route = useRoute()
const router = useRouter()
const category = ref(route.params.category === 'single' ? 'single' : 'roll')
const date = ref(new Date().toISOString().slice(0, 10))
const summary = ref(null)
const error = ref('')

async function load() {
  error.value = ''
  try {
    summary.value = await dashboardApi.getLiandian(date.value, category.value)
  } catch {
    error.value = '加载失败，请重试'
  }
}

function openDetail(machine) {
  router.push({
    path: `/liandian/${category.value}/detail`,
    query: { machineName: machine.machineName }
  })
}

onMounted(load)
watch(category, load)
</script>

<template>
  <div class="page" data-test="liandian-list">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="router.push('/dashboard')">返回看板</button>
      <h1>{{ category === 'roll' ? '联点 · 卷状机台' : '联点 · 单片机台' }}</h1>
      <input type="date" v-model="date" @change="load" />
    </header>
    <p v-if="error" class="error">{{ error }}</p>
    <template v-if="summary">
      <SummaryTotal :total="summary.totalQuantity" :date="date" />
      <TrendChart
        :labels="(summary.machines || []).map((m) => m.machineName)"
        :values="(summary.machines || []).map((m) => m.productQuantity)"
        label="机台产量"
      />
      <ProductionTable
        :rows="summary.machines || []"
        category="machine"
        @row-click="openDetail"
      />
    </template>
  </div>
</template>
