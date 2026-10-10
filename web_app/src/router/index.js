import { createRouter, createWebHistory } from 'vue-router'
import { isAuthenticated } from '../services/auth'

const routes = [
  { path: '/', redirect: '/dashboard' },
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue')
  },
  {
    path: '/dashboard',
    name: 'dashboard',
    component: () => import('../views/DashboardView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/liandian/:category',
    name: 'liandian-list',
    component: () => import('../views/LiandianListView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/liandian/:category/region/:countryCode',
    name: 'liandian-region',
    component: () => import('../views/RegionMachinesView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/liandian/:category/detail',
    name: 'liandian-detail',
    component: () => import('../views/LiandianDetailView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/zpl',
    name: 'zpl-list',
    component: () => import('../views/ZplListView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/zpl/users',
    name: 'zpl-users',
    component: () => import('../views/UserListView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/zpl/detail',
    name: 'zpl-detail',
    component: () => import('../views/ZplDetailView.vue'),
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory('/app/'),
  routes
})

router.beforeEach((to) => {
  if (to.meta.requiresAuth && !isAuthenticated()) {
    return { path: '/login', query: to.fullPath ? { redirect: to.fullPath } : undefined }
  }
  return true
})

export default router
