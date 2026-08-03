import { beforeEach, describe, expect, it } from 'vitest'
import router from './index'

describe('router auth guard', () => {
  beforeEach(() => {
    localStorage.removeItem('authToken')
  })

  it('redirects unauthenticated users to login', async () => {
    await router.push('/dashboard')
    expect(router.currentRoute.value.path).toBe('/login')
  })
})
