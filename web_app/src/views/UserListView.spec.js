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
      totalQuantity: 470,
      userCount: 4,
      users: [
        { userName: 'ADMIN', quantity: 200, region: 'Shanghai Maxim', orderNo: 'ORD-1' },
        { userName: 'admin', quantity: 50, region: 'Shanghai Maxim', orderNo: 'ORD-2' },
        { userName: 'Linda', quantity: 120, region: 'Vietnam Maxim', orderNo: 'ORD-3' },
        { userName: 'Huy', quantity: 100, region: 'IPPS-VN', orderNo: 'ORD-4' }
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

  it('renders region cards with zplRegionMapping, aggregating multiple factories under same country', () => {
    expect(dashboardApi.getZplSummary).toHaveBeenCalled()
    const cards = wrapper.findAll('.region-card')
    // Shanghai Maxim -> 中国, Vietnam Maxim & IPPS-VN -> 越南 (2 regions)
    expect(cards).toHaveLength(2)

    // 中国 & 越南
    expect(wrapper.text()).toContain('中国')
    expect(wrapper.text()).toContain('越南')

    // ADMIN and admin merged into 1 user with 250 quantity under 中国
    expect(wrapper.text()).toContain('ADMIN')
    expect(wrapper.text()).toContain('250')

    // Linda (120) and Huy (100) merged under 越南 (total 220)
    expect(wrapper.text()).toContain('Linda')
    expect(wrapper.text()).toContain('Huy')
    expect(wrapper.text()).toContain('220 件')
  })

  it('navigates to single user detail on clicking user chip', async () => {
    const userChip = wrapper.find('.user-chip')
    expect(userChip.exists()).toBe(true)

    await userChip.trigger('click')
    await flushPromises()

    expect(router.currentRoute.value.path).toBe('/zpl/detail')
    expect(router.currentRoute.value.query.mode).toBe('user')
    expect(router.currentRoute.value.query.userName).toBe('ADMIN')
  })

  it('navigates to merged region detail on clicking region card outside user chips', async () => {
    const cards = wrapper.findAll('.region-card')
    // Click the card (non-user area)
    await cards[0].trigger('click')
    await flushPromises()

    expect(router.currentRoute.value.path).toBe('/zpl/detail')
    expect(router.currentRoute.value.query.mode).toBe('merged')
    expect(router.currentRoute.value.query.region).toBe('中国')
    expect(router.currentRoute.value.query.userNames).toBe('ADMIN')
  })
})
