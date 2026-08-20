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
const loading = ref(false)
let timer = null

async function load() {
  error.value = ''
  loading.value = true
  try {
    summary.value = await dashboardApi.getSummary(filters.range.start, filters.range.end)
  } catch {
    error.value = '加载失败，请重试'
  } finally {
    loading.value = false
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
    sub: `${summary.value?.roll?.machineCount ?? 0} 台机台`
  },
  {
    label: '单片机台',
    value: summary.value?.single?.totalQuantity ?? 0,
    sub: `${summary.value?.single?.machineCount ?? 0} 台机台`
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
  <div class="dashboard fade-up" data-test="dashboard">
    <header class="topbar topbar--stack">
      <div class="topbar__row">
        <h1>
          <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
            <rect x="3" y="3" width="7" height="9" rx="1" />
            <rect x="14" y="3" width="7" height="5" rx="1" />
            <rect x="14" y="12" width="7" height="9" rx="1" />
            <rect x="3" y="16" width="7" height="5" rx="1" />
          </svg>
          生产大盘看板
        </h1>
        <DateRangeFilter :start="filters.range.start" :end="filters.range.end" @query="onRange" />
      </div>
      <div class="topbar__row topbar__row--actions">
        <button type="button" class="btn" :disabled="loading" @click="load">
          <svg :style="{ animation: loading ? 'spin 1s linear infinite' : 'none' }" viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="23 4 23 10 17 10" />
            <polyline points="1 20 1 14 7 14" />
            <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15" />
          </svg>
          刷新数据
        </button>
        <button type="button" class="btn btn-ghost" @click="signOut">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
            <polyline points="16 17 21 12 16 7" />
            <line x1="21" y1="12" x2="9" y2="12" />
          </svg>
          退出登录
        </button>
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
        <div
          v-for="(stat, i) in miniStats"
          :key="stat.label"
          class="mini-stat card fade-up"
          :style="{ animationDelay: `${(i + 1) * 70}ms` }"
        >
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
