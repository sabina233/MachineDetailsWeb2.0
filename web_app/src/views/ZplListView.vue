<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '../services/api'
import { filters } from '../services/filterStore'
import { deduplicateUsers } from '../utils/userUtils'
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

// 忽略大小写去重并合并产量后的用户列表
const distinctUsers = computed(() => {
  return deduplicateUsers(summary.value?.users || [])
})

// 点击用户列表行时，先进入按区域区分的 userlistview 页面
function openUserList(user) {
  router.push({
    path: '/zpl/users',
    query: user?.region ? { region: user.region } : undefined
  })
}

onMounted(load)
</script>

<template>
  <div class="page fade-up" data-test="zpl-list">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="router.push('/dashboard')">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6" />
        </svg>
        返回看板
      </button>
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

      <section class="card fade-up">
        <h2 class="card__title">
          <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
            <line x1="18" y1="20" x2="18" y2="10" />
            <line x1="12" y1="20" x2="12" y2="4" />
            <line x1="6" y1="20" x2="6" y2="14" />
          </svg>
          用户产量分布
        </h2>
        <TrendChart
          :labels="distinctUsers.map((u) => u.userName)"
          :values="distinctUsers.map((u) => u.quantity)"
          label="用户产量"
        />
      </section>

      <section class="card fade-up">
        <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; flex-wrap: wrap; gap: 8px;">
          <h2 class="card__title" style="margin-bottom: 0;">
            <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color: var(--primary);">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
              <circle cx="9" cy="7" r="4" />
              <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
              <path d="M16 3.13a4 4 0 0 1 0 7.75" />
            </svg>
            用户列表
          </h2>
          <button
            type="button"
            class="btn btn-ghost"
            style="padding: 4px 12px; font-size: 13px;"
            @click="router.push('/zpl/users')"
          >
            按区域查看用户概览 →
          </button>
        </div>
        <!-- 点击行先进入按区域区分的 userlistview 页面 -->
        <ProductionTable :rows="distinctUsers" category="zpl" @row-click="openUserList" />
      </section>
    </template>
  </div>
</template>
