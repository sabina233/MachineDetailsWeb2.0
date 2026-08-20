import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import RegionMachinesView from './RegionMachinesView.vue'
import { machineApi } from '../services/api'

vi.mock('../services/api', () => ({
  machineApi: {
    getMachines: vi.fn()
  }
}))

const routes = [
  { path: '/liandian/:category/region/:countryCode', component: RegionMachinesView },
  { path: '/liandian/:category', component: { template: '<div />' } },
  { path: '/liandian/:category/detail', component: { template: '<div />' } }
]

describe('RegionMachinesView', () => {
  let wrapper
  let router

  beforeEach(async () => {
    machineApi.getMachines.mockResolvedValue([
      {
        machineName: 'RFID-601',
        countryCode: 'CN',
        state: 1,
        taskName: 'TASK-A1',
        speed: 120,
        setSpeed: 150,
        productQuantity: 500,
        timestamp: '2026-08-20T10:00:00'
      },
      {
        machineName: 'RFID-801',
        countryCode: 'CN',
        state: 0,
        taskName: 'TASK-B2',
        speed: 0,
        setSpeed: 100,
        productQuantity: 200,
        timestamp: '2026-08-20T09:30:00'
      },
      {
        machineName: 'RFID-602',
        countryCode: 'US',
        state: 1,
        taskName: 'TASK-C3',
        speed: 80,
        setSpeed: 100,
        productQuantity: 300,
        timestamp: '2026-08-20T11:00:00'
      }
    ])

    router = createRouter({ history: createMemoryHistory(), routes })
    await router.push('/liandian/roll/region/CN')
    await router.isReady()
    wrapper = mount(RegionMachinesView, { global: { plugins: [router] } })
    await flushPromises()
  })

  afterEach(() => {
    wrapper?.unmount()
    vi.clearAllMocks()
  })

  it('renders independent block cards for the selected region', () => {
    expect(machineApi.getMachines).toHaveBeenCalled()
    const cards = wrapper.findAll('[data-test="machine-card"]')
    expect(cards).toHaveLength(2) // RFID-601 and RFID-801 for CN
    expect(wrapper.text()).toContain('RFID-601')
    expect(wrapper.text()).toContain('RFID-801')
    expect(wrapper.text()).not.toContain('RFID-602')
  })

  it('displays correct machine statistics and data', () => {
    expect(wrapper.text()).toContain('700') // 500 + 200 total quantity
    expect(wrapper.text()).toContain('生产中')
    expect(wrapper.text()).toContain('已停机')
    expect(wrapper.text()).toContain('TASK-A1')
  })

  it('navigates to detail view when a machine card is clicked', async () => {
    const pushSpy = vi.spyOn(router, 'push')
    const firstCard = wrapper.find('[data-test="machine-card"]')
    await firstCard.trigger('click')

    expect(pushSpy).toHaveBeenCalledWith({
      path: '/liandian/roll/detail',
      query: { machineName: 'RFID-601', countryCode: 'CN' }
    })
  })
})
