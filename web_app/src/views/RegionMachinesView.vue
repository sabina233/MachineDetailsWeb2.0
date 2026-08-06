<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { machineApi } from '../services/api'

const route = useRoute()
const router = useRouter()
const countryCode = ref(route.params.countryCode || '')
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

const regionName = computed(() => regionNames[countryCode.value] || countryCode.value)

async function load() {
  error.value = ''
  try {
    const all = await machineApi.getMachines()
    machines.value = all.filter((m) => (m.countryCode || '') === countryCode.value)
  } catch {
    error.value = '加载失败，请重试'
  }
}

const totalQuantity = computed(() =>
  machines.value.reduce((sum, m) => sum + (m.productQuantity || 0), 0)
)

function formatTs(timestamp) {
  return timestamp ? String(timestamp).replace('T', ' ').slice(0, 19) : '-'
}

function openDetail(machine) {
  router.push({
    path: `/liandian/${route.params.category || 'roll'}/detail`,
    query: { machineName: machine.machineName }
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
    <header class="topbar">
      <button
        type="button"
        class="btn btn-ghost"
        @click="router.push(`/liandian/${route.params.category || 'roll'}`)"
      >
        返回区域
      </button>
      <h1>联点机台 · {{ regionName }}</h1>
    </header>
    <p v-if="error" class="error">{{ error }}</p>

    <section v-if="machines.length" class="stats-row">
      <div class="stat-card card">
        <span class="stat-card__label">机台数</span>
        <span class="stat-card__value">{{ machines.length }}</span>
      </div>
      <div class="stat-card card">
        <span class="stat-card__label">当前产量合计</span>
        <span class="stat-card__value">{{ totalQuantity }}</span>
      </div>
    </section>

    <section class="card">
      <table class="production-table">
        <thead>
          <tr>
            <th>机台</th>
            <th>状态</th>
            <th>当前任务</th>
            <th>速度</th>
            <th>产量</th>
            <th>最新时间</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="machine in machines"
            :key="machine.machineName"
            class="table-row"
            @click="openDetail(machine)"
          >
            <td>{{ machine.machineName }}</td>
            <td>
              <span class="status" :class="machine.state === 1 ? 'status--on' : 'status--off'">
                {{ machine.state === 1 ? '开启' : '关闭' }}
              </span>
            </td>
            <td>{{ machine.taskName || '—' }}</td>
            <td>{{ machine.speed ?? '—' }}</td>
            <td>{{ machine.productQuantity ?? '—' }}</td>
            <td>{{ formatTs(machine.timestamp) }}</td>
          </tr>
          <tr v-if="!machines.length">
            <td colspan="6" class="empty">该区域暂无对应机台</td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
