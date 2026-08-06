import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import DashboardView from './DashboardView.vue'
import { dashboardApi } from '../services/api'

vi.mock('../services/api', () => ({
  dashboardApi: {
    getSummary: vi.fn()
  }
}))

vi.mock('../services/auth', () => ({
  logout: vi.fn()
}))

function makeSummary() {
  return {
    totalQuantity: 100,
    roll: {
      totalQuantity: 60,
      machineCount: 2,
      machines: [
        {
          machineName: 'RFID-601',
          productQuantity: 60,
          activeSeconds: 600,
          state: 1,
          timestamp: '2026-08-03T10:00:00'
        }
      ]
    },
    single: { totalQuantity: 30, machineCount: 1, machines: [] },
    zpl: {
      totalQuantity: 10,
      userCount: 2,
      users: [
        {
          userName: 'U1',
          quantity: 10,
          orderNo: 'O1',
          region: null,
          timestamp: '2026-08-03T09:00:00'
        }
      ]
    }
  }
}

describe('DashboardView', () => {
  let wrapper

  beforeEach(() => {
    dashboardApi.getSummary.mockResolvedValue(makeSummary())
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [{ path: '/dashboard', component: DashboardView }]
    })
    wrapper = mount(DashboardView, { global: { plugins: [router] } })
  })

  afterEach(() => {
    wrapper?.unmount()
    vi.clearAllMocks()
  })

  it('renders total and three vertically ordered category sections', async () => {
    await flushPromises()

    expect(wrapper.findAll('[data-test="category-section"]')).toHaveLength(3)
    expect(wrapper.text()).toContain('总产量')
    expect(wrapper.findAll('[data-test="category-section"]')[0].attributes('data-category')).toBe('roll')
  })
})
