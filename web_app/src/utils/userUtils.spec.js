import { describe, expect, it } from 'vitest'
import { deduplicateUsers } from './userUtils'

describe('deduplicateUsers', () => {
  it('deduplicates users case-insensitively and sums up their quantities', () => {
    const rawUsers = [
      { userName: 'ADMIN', quantity: 200, region: 'CN', orderNo: 'ORD-1', timestamp: '2026-08-01 10:00:00' },
      { userName: 'admin', quantity: 50, region: 'CN', orderNo: 'ORD-2', timestamp: '2026-08-01 11:00:00' },
      { userName: 'JohnDoe', quantity: 120, region: 'VN', orderNo: 'ORD-3', timestamp: '2026-08-01 09:00:00' },
      { userName: 'johndoe', quantity: 30, region: 'VN', orderNo: 'ORD-4', timestamp: '2026-08-01 12:00:00' }
    ]

    const result = deduplicateUsers(rawUsers)

    expect(result).toHaveLength(2)
    // ADMIN has 200 + 50 = 250
    expect(result[0].userName).toBe('ADMIN')
    expect(result[0].quantity).toBe(250)
    expect(result[0].region).toBe('CN')
    expect(result[0].orderNo).toBe('ORD-2')

    // JohnDoe has 120 + 30 = 150
    expect(result[1].userName).toBe('JohnDoe')
    expect(result[1].quantity).toBe(150)
    expect(result[1].region).toBe('VN')
  })

  it('handles empty or malformed user arrays gracefully', () => {
    expect(deduplicateUsers([])).toEqual([])
    expect(deduplicateUsers(null)).toEqual([])
    expect(deduplicateUsers([null, { userName: '' }])).toEqual([])
  })
})
