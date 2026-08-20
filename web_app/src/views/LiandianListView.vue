<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { machineApi } from '../services/api'

const route = useRoute()
const router = useRouter()
const machines = ref([])
const error = ref('')

const regionNames = {
  CN: '中国',
  US: '美国',
  JP: '日本',
  DE: '德国',
  GB: '英国',
  FR: '法国',
  KR: '韩国',
  IN: '印度',
  VN: '越南'
}

async function load() {
  error.value = ''
  try {
    machines.value = await machineApi.getMachines()
  } catch {
    error.value = '加载失败，请重试'
  }
}

function classify(name) {
  if (/^RFID-6/i.test(name || '')) {
    return 'roll'
  }
  if (/^RFID-8/i.test(name || '')) {
    return 'single'
  }
  return 'other'
}

const regions = computed(() => {
  const map = new Map()
  for (const machine of machines.value) {
    const code = machine.countryCode || '未知'
    if (!map.has(code)) {
      map.set(code, { code, roll: [], single: [], other: [] })
    }
    map.get(code)[classify(machine.machineName)].push(machine)
  }
  return [...map.values()].sort(
    (a, b) => b.roll.length + b.single.length + b.other.length - (a.roll.length + a.single.length + a.other.length)
  )
})

function openRegion(code) {
  router.push(`/liandian/${route.params.category || 'roll'}/region/${encodeURIComponent(code)}`)
}

onMounted(load)
</script>

<template>
  <div class="page fade-up" data-test="liandian-list">
    <header class="topbar">
      <button type="button" class="btn btn-ghost" @click="router.push('/dashboard')">返回看板</button>
      <h1>联点机台 · 区域</h1>
    </header>
    <p v-if="error" class="error">{{ error }}</p>

    <div v-if="regions.length" class="region-grid">
      <button
        v-for="(region, index) in regions"
        :key="region.code"
        type="button"
        class="region-card card fade-up"
        :style="{ animationDelay: index * 60 + 'ms' }"
        @click="openRegion(region.code)"
      >
        <span class="region-card__icon">
          <svg viewBox="0 0 24 24" width="26" height="26" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 1 1 16 0Z" />
            <circle cx="12" cy="10" r="3" />
          </svg>
        </span>
        <span class="region-card__name">{{ regionNames[region.code] || region.code }}</span>
        <span class="region-card__code">{{ region.code }}</span>
        <span class="region-card__machines">
          <span class="region-card__count">
            {{ region.roll.length + region.single.length + region.other.length }} 台机台
          </span>
          <span v-if="region.roll.length" class="region-card__group">
            <span class="region-card__group-label">卷状</span>
            <span class="region-card__names">{{ region.roll.map((m) => m.machineName).join('、') }}</span>
          </span>
          <span v-if="region.single.length" class="region-card__group">
            <span class="region-card__group-label">单片</span>
            <span class="region-card__names">{{ region.single.map((m) => m.machineName).join('、') }}</span>
          </span>
          <span v-if="region.other.length" class="region-card__group">
            <span class="region-card__group-label">其他</span>
            <span class="region-card__names">{{ region.other.map((m) => m.machineName).join('、') }}</span>
          </span>
        </span>
        <span class="region-card__arrow">→</span>
      </button>
    </div>
    <p v-else-if="!error" class="empty-note">暂无区域数据</p>
  </div>
</template>
