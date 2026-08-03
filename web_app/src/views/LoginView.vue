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
    <form class="login-card card" data-test="login-form" @submit.prevent="submit">
      <h1>生产看板登录</h1>
      <input v-model="username" type="text" placeholder="用户名" required />
      <input v-model="password" type="password" placeholder="密码" required />
      <p v-if="error" class="error" data-test="login-error">{{ error }}</p>
      <button type="submit" class="btn" :disabled="loading">
        {{ loading ? '登录中…' : '登录' }}
      </button>
    </form>
  </div>
</template>
