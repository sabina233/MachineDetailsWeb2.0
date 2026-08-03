import { describe, expect, it } from 'vitest'
import { makeApiUrl } from './api'

describe('makeApiUrl', () => {
  it('uses dashboard endpoint and encodes date', () => {
    const url = makeApiUrl('dashboard_summary_Url', { date: '2026-08-03' })
    expect(url).toContain('/api/Data/dashboard/summary')
    expect(url).toContain('date=2026-08-03')
  })

  it('drops empty parameters', () => {
    const url = makeApiUrl('dashboard_liandian_Url', { date: '2026-08-03', category: '' })
    expect(url).not.toContain('category=')
  })
})
