<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { machineApi } from '../services/api'
import MachineImage from '../components/MachineImage.vue'
import { getMachineCategory, getMachineCategoryName } from '../config/machineImages'

const route = useRoute()
const router = useRouter()
const countryCode = ref(route.params.countryCode || '')
const machines = ref([])
const error = ref('')
const searchQuery = ref('')
const statusFilter = ref('all') // 'all' | 'on' | 'off'
const categoryFilter = ref('all') // 'all' | 'roll' | 'single' | 'other'

const regionNames = {
  CN: '中国',
  US: '美国',
  JP: '日本',
  FR: '法国',
  KR: '韩国',
  VN: '越南',
  GD: '广东',
  HK: '香港',
  TW: '台湾',
  CB:'柬埔寨',
  ID:'印尼',
  TH:'泰国',
  IND:'印度',
  BD:'孟加拉',
  PK:'巴基斯坦',
  SLR:'斯里兰卡',
  TK:'土耳其',
  UK:'英国',
  GER:'德国',
  GT:'危地马拉'
}

const regionName = computed(() => regionNames[countryCode.value] || countryCode.value)

async function load() {
  error.value = ''
  try {
    const all = await machineApi.getMachines()
    machines.value = all
      .filter((m) => (m.countryCode || '') === countryCode.value)
      .sort((a, b) => categoryRank(a.machineName) - categoryRank(b.machineName))
  } catch {
    error.value = '加载失败，请重试'
  }
}

function categoryRank(name) {
  return /^RFID-6/i.test(name || '') ? 0 : /^RFID-8/i.test(name || '') ? 1 : 2
}

const totalQuantity = computed(() =>
  machines.value.reduce((sum, m) => sum + (m.productQuantity || 0), 0)
)

const runningCount = computed(() =>
  machines.value.filter((m) => m.state === 1).length
)

const filteredMachines = computed(() => {
  return machines.value.filter((m) => {
    // 状态过滤
    if (statusFilter.value === 'on' && m.state !== 1) return false
    if (statusFilter.value === 'off' && m.state === 1) return false

    // 类别过滤
    const cat = getMachineCategory(m.machineName)
    if (categoryFilter.value !== 'all' && cat !== categoryFilter.value) return false

    // 搜索词过滤（机台名或任务名）
    if (searchQuery.value.trim()) {
      const q = searchQuery.value.trim().toLowerCase()
      const matchName = (m.machineName || '').toLowerCase().includes(q)
      const matchTask = (m.taskName || '').toLowerCase().includes(q)
      if (!matchName && !matchTask) return false
    }

    return true
  })
})

function formatTs(timestamp) {
  return timestamp ? String(timestamp).replace('T', ' ').slice(0, 19) : '-'
}

function openDetail(machine) {
  router.push({
    path: `/liandian/${route.params.category || 'roll'}/detail`,
    query: { machineName: machine.machineName, countryCode: countryCode.value }
  })
}

onMounted(load)
watch(
  () => route.params.countryCode,
  () => {
    countryCode.value = route.params.countryCode || ''
    load()
  }
)
</script>

<template>
  <div class="page fade-up" data-test="region-machines">
    <!-- 顶部导航栏 -->
    <header class="topbar">
      <button
        type="button"
        class="btn btn-ghost"
        @click="router.push(`/liandian/${route.params.category || 'roll'}`)"
      >
        <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="15 18 9 12 15 6" />
        </svg>
        返回区域
      </button>
      <h1>联点机台 · {{ regionName }} <span v-if="countryCode" class="region-card__code">{{ countryCode }}</span></h1>
      <button type="button" class="btn" @click="load">刷新状态</button>
    </header>

    <p v-if="error" class="error">{{ error }}</p>

    <!-- 统计卡片区 -->
    <section v-if="machines.length" class="stats-row">
      <div class="stat-card card">
        <span class="stat-card__label">总机台数</span>
        <span class="stat-card__value">{{ machines.length }}</span>
        <span class="stat-card__sub">区域内全部已接入机台</span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">运行中机台</span>
        <span class="stat-card__value" style="color: var(--on)">
          {{ runningCount }}
          <span style="font-size: 14px; font-weight: normal; color: var(--muted)">/ {{ machines.length }} 台</span>
        </span>
        <span class="stat-card__sub">开机运行率 {{ machines.length ? Math.round((runningCount / machines.length) * 100) : 0 }}%</span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">当前产量合计</span>
        <span class="stat-card__value">{{ totalQuantity }}</span>
        <span class="stat-card__sub">实时生产累计件数</span>
      </div>
    </section>

    <!-- 筛选与搜索工具栏 -->
    <section v-if="machines.length" class="toolbar">
      <div class="toolbar__left">
        <!-- 类别筛选 -->
        <div class="filter-pills">
          <button
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': categoryFilter === 'all' }"
            @click="categoryFilter = 'all'"
          >
            全部类型
          </button>
          <button
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': categoryFilter === 'roll' }"
            @click="categoryFilter = 'roll'"
          >
            卷状机台
          </button>
          <button
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': categoryFilter === 'single' }"
            @click="categoryFilter = 'single'"
          >
            单片机台
          </button>
        </div>

        <!-- 状态筛选 -->
        <div class="filter-pills">
          <button
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': statusFilter === 'all' }"
            @click="statusFilter = 'all'"
          >
            全部状态
          </button>
          <button
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': statusFilter === 'on' }"
            @click="statusFilter = 'on'"
          >
            运行中
          </button>
          <button
            type="button"
            class="filter-pill"
            :class="{ 'filter-pill--active': statusFilter === 'off' }"
            @click="statusFilter = 'off'"
          >
            待机/停机
          </button>
        </div>
      </div>

      <div class="toolbar__right">
        <!-- 搜索输入框 -->
        <div class="search-input">
          <svg class="search-input__icon" viewBox="0 0 24 24" width="15" height="15" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="搜索机台名称 / 任务号..."
          />
        </div>
      </div>
    </section>

    <!-- 独立块状机台卡片网格 -->
    <section v-if="filteredMachines.length" class="machine-grid">
      <div
        v-for="(machine, idx) in filteredMachines"
        :key="machine.machineName"
        class="machine-card fade-up"
        :class="{ 'machine-card--active': machine.state === 1 }"
        :style="{ animationDelay: `${(idx % 12) * 45}ms` }"
        data-test="machine-card"
        @click="openDetail(machine)"
      >
        <!-- 卡片顶部：机台名、类型标签与状态指示 -->
        <div class="machine-card__top">
          <div class="machine-card__title-wrap">
            <span class="machine-card__name">{{ machine.machineName }}</span>
            <span class="category-tag">{{ getMachineCategoryName(machine.machineName) }}</span>
          </div>

          <span
            class="status-pill"
            :class="machine.state === 1 ? 'status-pill--on' : 'status-pill--off'"
          >
            <span
              class="status-dot"
              :class="{ 'status-dot--pulse': machine.state === 1 }"
            ></span>
            {{ machine.state === 1 ? '生产中' : '已停机' }}
          </span>
        </div>

        <!-- 机台图片/图标展示区 (可自定义图片或自动 fallback 矢量图标) -->
        <MachineImage
          :machine-name="machine.machineName"
          :alt="machine.machineName"
        />

        <!-- 当前任务展示 -->
        <div class="machine-card__task">
          <span class="machine-card__task-label">当前任务:</span>
          <span class="machine-card__task-name" :title="machine.taskName || '无任务'">
            {{ machine.taskName || '—' }}
          </span>
        </div>

        <!-- 关键数据指标 (产量与速度) -->
        <div class="machine-card__metrics">
          <div class="machine-card__metric-item">
            <span class="machine-card__metric-label">当前产量</span>
            <span class="machine-card__metric-val">{{ machine.productQuantity ?? 0 }}</span>
          </div>
          <div class="machine-card__metric-item">
            <span class="machine-card__metric-label">运行速度</span>
            <span class="machine-card__metric-val">{{ machine.speed ?? '—' }}</span>
          </div>
        </div>

        <!-- 底部信息与交互入口 -->
        <div class="machine-card__footer">
          <span class="machine-card__time">
            <svg viewBox="0 0 24 24" width="13" height="13" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10" />
              <polyline points="12 6 12 12 16 14" />
            </svg>
            {{ formatTs(machine.timestamp) }}
          </span>
          <span class="machine-card__action">
            查看详情
            <span class="machine-card__arrow">→</span>
          </span>
        </div>
      </div>
    </section>

    <!-- 空数据状态 -->
    <div v-else-if="!error && machines.length" class="empty-note card">
      未找到符合筛选条件的机台
    </div>
    <div v-else-if="!error && !machines.length" class="empty-note card">
      该区域暂无机台接入数据
    </div>
  </div>
</template>
