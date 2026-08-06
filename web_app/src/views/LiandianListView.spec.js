import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import LiandianListView from './LiandianListView.vue'
import { machineApi } from '../services/api'

vi.mock('../services/api', () => ({
  machineApi: {
    getMachines: vi.fn()
  }
}))

const routes = [
  { path: '/liandian/:category', component: LiandianListView },
  { path: '/liandian/:category/region/:countryCode', component: { template: '<div />' } }
]

describe('LiandianListView', () => {
  let wrapper

  beforeEach(async () => {
    machineApi.getMachines.mockResolvedValue([
      { machineName: 'RFID-601', countryCode: 'CN', state: 1, taskName: 'A100', productQuantity: 240 },
      { machineName: 'RFID-602', countryCode: 'US', state: 1, taskName: 'C300', productQuantity: 100 },
      { machineName: 'RFID-801', countryCode: 'JP', state: 1, taskName: 'S100', productQuantity: 45 }
    ])

    const router = createRouter({ history: createMemoryHistory(), routes })
    await router.push('/liandian/roll')
    await router.isReady()
    wrapper = mount(LiandianListView, { global: { plugins: [router] } })
    await flushPromises()
  })

  afterEach(() => {
    wrapper?.unmount()
    vi.clearAllMocks()
  })

  it('groups all DataLatest machines into region cards by countryCode', () => {
    expect(machineApi.getMachines).toHaveBeenCalled()
    expect(wrapper.findAll('.region-card')).toHaveLength(3)
    expect(wrapper.text()).toContain('中国')
    expect(wrapper.text()).toContain('美国')
    expect(wrapper.text()).toContain('日本')
  })
})
