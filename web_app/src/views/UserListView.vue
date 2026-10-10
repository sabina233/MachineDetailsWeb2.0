<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '../services/api'
import { filters } from '../services/filterStore'
import { deduplicateUsers } from '../utils/userUtils'
import { regionNames, getRegionName } from '../config/regionImages'
import RegionImage from '../components/RegionImage.vue'
import DateRangeFilter from '../components/DateRangeFilter.vue'

const router = useRouter()
const summary = ref(null)
const error = ref('')
const loading = ref(false)
const searchQuery = ref('')
const selectedRegion = ref('all')

async function load() {
  error.value = ''
  loading.value = true
  try {
    summary.value = await dashboardApi.getZplSummary(filters.range.start, filters.range.end)
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

// 1. 忽略大小写去重合并后的全部用户列表
const distinctUsers = computed(() => {
  return deduplicateUsers(summary.value?.users || [])
})

// 2. 按区域进行聚合分组
const allRegions = computed(() => {
  const map = new Map()

  for (const user of distinctUsers.value) {
    const code = (user.region || '未知').trim().toUpperCase()
    if (!map.has(code)) {
      map.set(code, {
        code,
        name: getRegionName(code),
        totalQuantity: 0,
        users: []
      })
    }
    const regionObj = map.get(code)
    regionObj.totalQuantity += Number(user.quantity) || 0
    regionObj.users.push(user)
  }

  // 区域内用户按产量排序
  for (const regionObj of map.values()) {
    regionObj.users.sort((a, b) => b.quantity - a.quantity)
  }

  // 区域按结案总量从大到小排序
  return [...map.values()].sort((a, b) => b.totalQuantity - a.totalQuantity)
})

// 3. 搜索与筛选过滤
const filteredRegions = computed(() => {
  return allRegions.value
    .filter((reg) => {
      // 区域筛选
      if (selectedRegion.value !== 'all' && reg.code !== selectedRegion.value) {
        return false
      }
      // 关键词搜索（区域名、区域代码或用户名）
      if (searchQuery.value.trim()) {
        const q = searchQuery.value.trim().toLowerCase()
        const matchCode = reg.code.toLowerCase().includes(q)
        const matchName = reg.name.toLowerCase().includes(q)
        const matchUser = reg.users.some((u) => u.userName.toLowerCase().includes(q))
        if (!matchCode && !matchName && !matchUser) return false
      }
      return true
    })
})

// 总量统计
const totalQuantity = computed(() => {
  return distinctUsers.value.reduce((sum, u) => sum + (Number(u.quantity) || 0), 0)
})

function openUserDetail(userName) {
  if (!userName) return
  router.push({
    path: '/zpl/detail',
    query: { userName }
  })
}

function openRegionPrimaryUser(region) {
  if (region?.users?.length) {
    openUserDetail(region.users[0].userName)
  }
}

onMounted(load)
</script>

<template>
  <div class="page fade-up" data-test="user-list-view">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="router.push('/zpl')">
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6" />
        </svg>
        返回 ZPL 看板
      </button>
      <h1>ZPL 用户 · 区域概览</h1>
      <div class="topbar__actions">
        <DateRangeFilter
          :start="filters.range.start"
          :end="filters.range.end"
          @query="onRange"
        />
        <button type="button" class="btn" :disabled="loading" @click="load">刷新</button>
      </div>
    </header>

    <p v-if="error" class="error">{{ error }}</p>

    <!-- 汇总统计卡片 -->
    <section v-if="distinctUsers.length" class="stats-row fade-up">
      <div class="stat-card card">
        <span class="stat-card__label">有效用户数</span>
        <span class="stat-card__value">{{ distinctUsers.length }}</span>
        <span class="stat-card__sub">已忽略大小写去重合并</span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">覆盖区域数</span>
        <span class="stat-card__value">{{ allRegions.length }}</span>
        <span class="stat-card__sub">包含所有已上报国家及厂区</span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">区间总结案量</span>
        <span class="stat-card__value stat-card__value--accent">{{ totalQuantity }}</span>
        <span class="stat-card__sub">总生产标签结案件数</span>
      </div>
    </section>

    <!-- 工具栏（搜索与区域快速筛选） -->
    <div v-if="allRegions.length" class="toolbar fade-up">
      <div class="toolbar__left">
        <div class="search-input">
          <svg class="search-input__icon" viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            class="input"
            placeholder="搜索区域或用户名称…"
          />
        </div>
      </div>
      <div class="toolbar__right">
        <div class="filter-pills">
          <button
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': selectedRegion === 'all' }"
            @click="selectedRegion = 'all'"
          >
            全部区域 ({{ allRegions.length }})
          </button>
          <button
            v-for="r in allRegions"
            :key="r.code"
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': selectedRegion === r.code }"
            @click="selectedRegion = r.code"
          >
            {{ r.name }} ({{ r.users.length }})
          </button>
        </div>
      </div>
    </div>

    <!-- 区域卡片网格 -->
    <div v-if="filteredRegions.length" class="region-grid fade-up">
      <div
        v-for="(region, index) in filteredRegions"
        :key="region.code"
        class="region-card card fade-up"
        :style="{ animationDelay: `${index * 50}ms` }"
        @click="openRegionPrimaryUser(region)"
      >
        <div class="region-card__top">
          <!-- 自定义/默认国旗图片展示 -->
          <RegionImage :code="region.code" :name="region.name" size="md" />
          <span class="region-card__code">{{ region.code }}</span>
        </div>

        <span class="region-card__name">{{ region.name }}</span>

        <span class="region-card__machines">
          <span class="region-card__count">
            {{ region.users.length }} 名用户 · 累计结案 {{ region.totalQuantity }} 件
          </span>

          <div class="region-card__group">
            <span class="region-card__group-label">区域用户列表 (点击查看详情)</span>
            <div class="user-chips">
              <button
                v-for="u in region.users"
                :key="u.userName"
                type="button"
                class="user-chip"
                :title="`查看用户 ${u.userName} 的结案详情 (产量: ${u.quantity})`"
                @click.stop="openUserDetail(u.userName)"
              >
                <span class="user-chip__avatar">{{ u.userName.slice(0, 1).toUpperCase() }}</span>
                <span class="user-chip__name">{{ u.userName }}</span>
                <span class="user-chip__qty">{{ u.quantity }}</span>
              </button>
            </div>
          </div>
        </span>

        <span class="region-card__arrow">→</span>
      </div>
    </div>

    <p v-else-if="!error && allRegions.length" class="empty-note card">
      未找到符合筛选条件的区域或用户
    </p>
    <p v-else-if="!error && !loading" class="empty-note card">
      暂无 ZPL 用户数据
    </p>
  </div>
</template>
