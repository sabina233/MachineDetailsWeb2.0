import { beforeEach, describe, expect, it } from 'vitest'
import {
  filters,
  formatLocalDate,
  getDefaultWeekRange,
  getLocalDaysAgo,
  getLocalToday,
  resetToCurrentWeek
} from './filterStore'

describe('filterStore', () => {
  beforeEach(() => {
    sessionStorage.clear()
    localStorage.clear()
    resetToCurrentWeek()
  })

  it('correctly formats local dates without UTC offset issues', () => {
    const today = getLocalToday()
    const sixDaysAgo = getLocalDaysAgo(6)
    expect(today).toMatch(/^\d{4}-\d{2}-\d{2}$/)
    expect(sixDaysAgo).toMatch(/^\d{4}-\d{2}-\d{2}$/)

    const range = getDefaultWeekRange()
    expect(range.start).toBe(sixDaysAgo)
    expect(range.end).toBe(today)
  })

  it('defaults to rolling week range (6 days ago to today)', () => {
    expect(filters.range.start).toBe(getLocalDaysAgo(6))
    expect(filters.range.end).toBe(getLocalToday())
  })

  it('resets to current week when resetToCurrentWeek is called', () => {
    filters.range.start = '2020-01-01'
    filters.range.end = '2020-01-07'
    resetToCurrentWeek()
    expect(filters.range.start).toBe(getLocalDaysAgo(6))
    expect(filters.range.end).toBe(getLocalToday())
  })
})
