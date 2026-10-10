<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '../services/api'
import { filters } from '../services/filterStore'
import { deduplicateUsers } from '../utils/userUtils'
import { resolveZplRegion } from '../config/regionImages'
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

// 2. 按 zplRegionMapping 映射的国家/区域进行聚合分组
const allRegions = computed(() => {
  const map = new Map()

  for (const user of distinctUsers.value) {
    const info = resolveZplRegion(user.region)
    const countryKey = info.country || '未知区域'

    if (!map.has(countryKey)) {
      map.set(countryKey, {
        name: countryKey,
        flag: info.flag,
        image: info.image,
        rawRegions: new Set(),
        totalQuantity: 0,
        users: []
      })
    }

    const regionObj = map.get(countryKey)
    if (user.region) {
      regionObj.rawRegions.add(String(user.region).trim())
    }
    regionObj.totalQuantity += Number(user.quantity) || 0
    regionObj.users.push(user)
  }

  // 区域内用户按产量排序，并转换为数组
  for (const regionObj of map.values()) {
    regionObj.users.sort((a, b) => b.quantity - a.quantity)
    regionObj.rawRegionList = [...regionObj.rawRegions]
  }

  // 区域按结案总量从大到小排序
  return [...map.values()].sort((a, b) => b.totalQuantity - a.totalQuantity)
})

// 3. 搜索与筛选过滤
const filteredRegions = computed(() => {
  return allRegions.value
    .filter((reg) => {
      // 区域筛选
      if (selectedRegion.value !== 'all' && reg.name !== selectedRegion.value) {
        return false
      }
      // 关键词搜索（区域名、原始厂区名或用户名）
      if (searchQuery.value.trim()) {
        const q = searchQuery.value.trim().toLowerCase()
        const matchName = reg.name.toLowerCase().includes(q)
        const matchRaw = reg.rawRegionList.some((r) => r.toLowerCase().includes(q))
        const matchUser = reg.users.some((u) => u.userName.toLowerCase().includes(q))
        if (!matchName && !matchRaw && !matchUser) return false
      }
      return true
    })
})

// 总量统计
const totalQuantity = computed(() => {
  return distinctUsers.value.reduce((sum, u) => sum + (Number(u.quantity) || 0), 0)
})

// 点击具体用户：查看该用户的详情数据
function openUserDetail(userName, region) {
  if (!userName) return
  router.push({
    path: '/zpl/detail',
    query: {
      mode: 'user',
      userName,
      region: region?.name,
      flag: region?.flag
    }
  })
}

// 点击非详情用户区域（区域块本身）：合并显示该区域所有数据
function openRegionMerged(region) {
  if (!region) return
  router.push({
    path: '/zpl/detail',
    query: {
      mode: 'merged',
      region: region.name,
      flag: region.flag,
      rawRegions: region.rawRegionList?.join(','),
      userNames: region.users.map((u) => u.userName).join(',')
    }
  })
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
        <span class="stat-card__sub">对应全球 {{ allRegions.length }} 个国家及厂区</span>
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
            placeholder="搜索区域、厂区或用户名称…"
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
            :key="r.name"
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': selectedRegion === r.name }"
            @click="selectedRegion = r.name"
          >
            {{ r.name }} ({{ r.users.length }})
          </button>
        </div>
      </div>
    </div>

    <!-- 区域卡片网格 -->
    <div v-if="filteredRegions.length" class="region-grid fade-up">
      <!-- 区域块：点击非详情用户区域，合并显示该区域所有数据 -->
      <div
        v-for="(region, index) in filteredRegions"
        :key="region.name"
        class="region-card card fade-up"
        :style="{ animationDelay: `${index * 50}ms` }"
        :title="`点击合并查看【${region.name}】区域所有数据`"
        @click="openRegionMerged(region)"
      >
        <div class="region-card__top">
          <!-- 自定义/对应地区国旗图片展示 -->
          <RegionImage
            :code="region.flag"
            :name="region.name"
            :custom-image="region.image"
            size="md"
          />
          <span v-if="region.rawRegionList.length" class="region-card__code" :title="region.rawRegionList.join('、')">
            {{ region.rawRegionList.slice(0, 2).join(' / ') }}{{ region.rawRegionList.length > 2 ? ` 等${region.rawRegionList.length}厂区` : '' }}
          </span>
        </div>

        <span class="region-card__name">{{ region.name }}</span>

        <span class="region-card__machines">
          <span class="region-card__count">
            {{ region.users.length }} 名用户 · 累计结案 {{ region.totalQuantity }} 件
          </span>

          <div class="region-card__group">
            <div class="region-card__group-header">
              <span class="region-card__group-label">区域用户列表</span>
              <span class="region-card__hint">点击用户查看个人详情</span>
            </div>
            <!-- 用户气泡列表：点击具体用户查看对应用户数据 -->
            <div class="user-chips">
              <button
                v-for="u in region.users"
                :key="u.userName"
                type="button"
                class="user-chip"
                :title="`点击查看【${u.userName}】用户专属详情`"
                @click.stop="openUserDetail(u.userName, region)"
              >
                <span class="user-chip__avatar">{{ u.userName.slice(0, 1).toUpperCase() }}</span>
                <span class="user-chip__name">{{ u.userName }}</span>
                <span class="user-chip__qty">{{ u.quantity }}</span>
                <span class="user-chip__action">详情 →</span>
              </button>
            </div>
          </div>
        </span>

        <!-- 卡片底部合并查看引导 -->
        <div class="region-card__bottom-hint">
          <span>点击空白区域 · 合并查看本区域所有数据</span>
          <span class="region-card__arrow">→</span>
        </div>
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
