<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '../services/api'
import { logout } from '../services/auth'
import { filters } from '../services/filterStore'
import SummaryTotal from '../components/SummaryTotal.vue'
import CategorySection from '../components/CategorySection.vue'
import DateRangeFilter from '../components/DateRangeFilter.vue'

const router = useRouter()
const summary = ref(null)
const error = ref('')
let timer = null

async function load() {
  error.value = ''
  try {
    summary.value = await dashboardApi.getSummary(filters.range.start, filters.range.end)
  } catch {
    error.value = '加载失败，请重试'
  }
}

function onRange({ start, end }) {
  filters.range.start = start
  filters.range.end = end
  load()
}

function signOut() {
  logout()
  router.push('/login')
}

function goTo(category) {
  if (category === 'zpl') {
    router.push('/zpl')
  } else {
    router.push(`/liandian/${category}`)
  }
}

const miniStats = computed(() => [
  {
    label: '卷状机台',
    value: summary.value?.roll?.totalQuantity ?? 0,
    sub: `${summary.value?.roll?.machineCount ?? 0} 台`
  },
  {
    label: '单片机台',
    value: summary.value?.single?.totalQuantity ?? 0,
    sub: `${summary.value?.single?.machineCount ?? 0} 台`
  },
  {
    label: 'ZPL 结案',
    value: summary.value?.zpl?.totalQuantity ?? 0,
    sub: `${summary.value?.zpl?.userCount ?? 0} 名用户`
  }
])

onMounted(() => {
  load()
  timer = setInterval(load, 30000)
})

onBeforeUnmount(() => clearInterval(timer))
</script>

<template>
  <div class="dashboard" data-test="dashboard">
    <header class="topbar">
      <h1>生产看板</h1>
      <div class="topbar__actions">
        <DateRangeFilter :start="filters.range.start" :end="filters.range.end" @query="onRange" />
        <button type="button" class="btn" @click="load">刷新</button>
        <button type="button" class="btn btn-ghost" @click="signOut">退出登录</button>
      </div>
    </header>
    <p v-if="error" class="error" data-test="dashboard-error">{{ error }}</p>
    <template v-if="summary">
      <SummaryTotal
        :total="summary.totalQuantity"
        :start="filters.range.start"
        :end="filters.range.end"
      />
      <div class="mini-stats">
        <div v-for="stat in miniStats" :key="stat.label" class="mini-stat card">
          <span class="mini-stat__label">{{ stat.label }}</span>
          <span class="mini-stat__value">{{ stat.value }}</span>
          <span class="mini-stat__sub">{{ stat.sub }}</span>
        </div>
      </div>
      <CategorySection
        category="roll"
        title="联点 · 卷状机台"
        :summary="summary.roll"
        @open-details="goTo"
      />
      <CategorySection
        category="single"
        title="联点 · 单片机台"
        :summary="summary.single"
        @open-details="goTo"
      />
      <CategorySection
        category="zpl"
        title="ZPL"
        :summary="summary.zpl"
        @open-details="goTo"
      />
    </template>
  </div>
</template>
