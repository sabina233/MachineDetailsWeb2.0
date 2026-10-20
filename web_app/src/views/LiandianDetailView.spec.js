import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import LiandianDetailView from './LiandianDetailView.vue'
import { machineApi } from '../services/api'

vi.mock('../services/api', () => ({
  machineApi: {
    getRealTime: vi.fn(),
    getHistory: vi.fn(),
    getOrderChart: vi.fn(),
    getBrandChart: vi.fn()
  }
}))

const routes = [
  { path: '/liandian/:category/detail', component: LiandianDetailView },
  { path: '/liandian/:category', component: { template: '<div>list</div>' } }
]

describe('LiandianDetailView', () => {
  let wrapper
  let router

  beforeEach(async () => {
    machineApi.getRealTime.mockResolvedValue([
      { machineName: 'RFID-601', state: 1, taskName: 'TASK-1', speed: 100, setSpeed: 120 }
    ])

    // Mock 15 history rows to test pagination (pageSize = 10)
    const mockHistory = Array.from({ length: 15 }, (_, i) => ({
      taskName: `TASK-${i + 1}`,
      productQuantity: 100 * (i + 1),
      durationHours: 1.5,
      unitsPerHour: 100,
      startTime: '2026-10-15T10:00:00',
      endTime: '2026-10-15T11:30:00'
    }))
    machineApi.getHistory.mockResolvedValue(mockHistory)
    machineApi.getOrderChart.mockResolvedValue([])
    machineApi.getBrandChart.mockResolvedValue([])

    router = createRouter({ history: createMemoryHistory(), routes })
    await router.push('/liandian/roll/detail?machineName=RFID-601')
    await router.isReady()
    wrapper = mount(LiandianDetailView, {
      global: {
        plugins: [router],
        stubs: { TrendChart: true }
      }
    })
    await flushPromises()
  })

  afterEach(() => {
    wrapper?.unmount()
    vi.clearAllMocks()
  })

  it('paginates history orders table to 10 rows per page', () => {
    const table = wrapper.find('[data-test="history-table"]')
    expect(table.exists()).toBe(true)

    // Page 1 should show exactly 10 rows
    const rows = table.findAll('tbody tr')
    expect(rows).toHaveLength(10)
    expect(wrapper.text()).toContain('TASK-1')
    expect(wrapper.text()).toContain('TASK-10')
    expect(wrapper.text()).not.toContain('TASK-11')

    // Pager should show 2 pages
    expect(wrapper.find('.pager').exists()).toBe(true)
    expect(wrapper.text()).toContain('第 1 / 2 页')
    expect(wrapper.text()).toContain('共 15 笔订单')
  })

  it('changes page when next page button is clicked', async () => {
    const nextBtn = wrapper.findAll('.pager button')[1]
    expect(nextBtn.text()).toContain('下一页')

    await nextBtn.trigger('click')
    await flushPromises()

    // Page 2 should show remaining 5 rows
    const table = wrapper.find('[data-test="history-table"]')
    const rows = table.findAll('tbody tr')
    expect(rows).toHaveLength(5)
    expect(wrapper.text()).toContain('TASK-11')
    expect(wrapper.text()).toContain('TASK-15')
    expect(wrapper.text()).toContain('第 2 / 2 页')
  })
})
