<script setup>
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { dashboardApi } from '../services/api'
import { logout } from '../services/auth'
import SummaryTotal from '../components/SummaryTotal.vue'
import CategorySection from '../components/CategorySection.vue'

const router = useRouter()
const summary = ref(null)
const date = ref(new Date().toISOString().slice(0, 10))
const error = ref('')
let timer = null

async function load() {
  error.value = ''
  try {
    summary.value = await dashboardApi.getSummary(date.value)
  } catch {
    error.value = '加载失败，请重试'
  }
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
        <input type="date" v-model="date" @change="load" />
        <button type="button" class="btn" @click="load">刷新</button>
        <button type="button" class="btn btn-ghost" @click="signOut">退出登录</button>
      </div>
    </header>
    <p v-if="error" class="error" data-test="dashboard-error">{{ error }}</p>
    <template v-if="summary">
      <SummaryTotal :total="summary.totalQuantity" :date="date" />
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
