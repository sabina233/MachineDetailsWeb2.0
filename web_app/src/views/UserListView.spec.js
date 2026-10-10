import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import UserListView from './UserListView.vue'
import { dashboardApi } from '../services/api'

vi.mock('../services/api', () => ({
  dashboardApi: {
    getZplSummary: vi.fn()
  }
}))

const routes = [
  { path: '/zpl/users', component: UserListView },
  { path: '/zpl/detail', component: { template: '<div>detail</div>' } },
  { path: '/zpl', component: { template: '<div>zpl</div>' } }
]

describe('UserListView', () => {
  let wrapper
  let router

  beforeEach(async () => {
    dashboardApi.getZplSummary.mockResolvedValue({
      totalQuantity: 370,
      userCount: 3,
      users: [
        { userName: 'ADMIN', quantity: 200, region: 'CN', orderNo: 'ORD-1' },
        { userName: 'admin', quantity: 50, region: 'CN', orderNo: 'ORD-2' },
        { userName: 'Linda', quantity: 120, region: 'VN', orderNo: 'ORD-3' }
      ]
    })

    router = createRouter({ history: createMemoryHistory(), routes })
    await router.push('/zpl/users')
    await router.isReady()
    wrapper = mount(UserListView, { global: { plugins: [router] } })
    await flushPromises()
  })

  afterEach(() => {
    wrapper?.unmount()
    vi.clearAllMocks()
  })

  it('renders region cards with deduplicated user counts and quantities', () => {
    expect(dashboardApi.getZplSummary).toHaveBeenCalled()
    const cards = wrapper.findAll('.region-card')
    // CN and VN = 2 regions
    expect(cards).toHaveLength(2)

    // 中国 CN
    expect(wrapper.text()).toContain('中国')
    expect(wrapper.text()).toContain('越南')

    // ADMIN and admin merged into 1 user with 250 quantity
    expect(wrapper.text()).toContain('ADMIN')
    expect(wrapper.text()).toContain('250')

    // Linda has 120
    expect(wrapper.text()).toContain('Linda')
    expect(wrapper.text()).toContain('120')
  })

  it('navigates to ZplDetailView on clicking user chip', async () => {
    const userChip = wrapper.find('.user-chip')
    expect(userChip.exists()).toBe(true)

    await userChip.trigger('click')
    await flushPromises()

    expect(router.currentRoute.value.path).toBe('/zpl/detail')
    expect(router.currentRoute.value.query.userName).toBe('ADMIN')
  })
})
