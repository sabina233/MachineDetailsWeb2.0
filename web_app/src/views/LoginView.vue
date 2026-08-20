<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '../services/api'
import { login } from '../services/auth'

const router = useRouter()
const username = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  loading.value = true
  error.value = ''
  try {
    const res = await authApi.login(username.value, password.value)
    if (res.token) {
      login(res.token)
      router.push('/dashboard')
    } else {
      error.value = res.message || '用户名或密码错误'
    }
  } catch {
    error.value = '登录失败，请稍后重试'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <form class="login-card card fade-up" data-test="login-form" @submit.prevent="submit">
      <div style="text-align: center; margin-bottom: 4px;">
        <div style="width: 56px; height: 56px; margin: 0 auto 12px; border-radius: 16px; background: linear-gradient(135deg, #0A84FF 0%, #38BDF8 100%); display: flex; align-items: center; justify-content: center; color: white; box-shadow: 0 8px 20px rgba(10, 132, 255, 0.3);">
          <svg viewBox="0 0 24 24" width="30" height="30" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="3" width="7" height="9" rx="1" />
            <rect x="14" y="3" width="7" height="5" rx="1" />
            <rect x="14" y="12" width="7" height="9" rx="1" />
            <rect x="3" y="16" width="7" height="5" rx="1" />
          </svg>
        </div>
        <h1>生产看板管理系统</h1>
        <p style="margin: 0; color: var(--muted); font-size: 13px;">请登录以查看机台实时数据与看板</p>
      </div>

      <div style="display: flex; flex-direction: column; gap: 12px; margin-top: 10px;">
        <input v-model="username" type="text" placeholder="用户名" required />
        <input v-model="password" type="password" placeholder="密码" required />
      </div>

      <p v-if="error" class="error" data-test="login-error">{{ error }}</p>

      <button type="submit" class="btn" :disabled="loading" style="padding: 11px; font-size: 15px; margin-top: 8px;">
        <svg v-if="loading" style="animation: spin 1s linear infinite;" viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="23 4 23 10 17 10" />
          <polyline points="1 20 1 14 7 14" />
          <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15" />
        </svg>
        {{ loading ? '正在登录…' : '立即登录' }}
      </button>
    </form>
  </div>
</template>
