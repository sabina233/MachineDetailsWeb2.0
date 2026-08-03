import { expect, test } from '@playwright/test'

const fakeSummary = {
  totalQuantity: 100,
  roll: {
    category: 'roll',
    activeSeconds: 3600,
    totalQuantity: 60,
    machineCount: 2,
    machines: [
      {
        machineName: 'RFID-601',
        activeSeconds: 1800,
        productQuantity: 60,
        state: 1,
        timestamp: '2026-08-03T10:00:00'
      }
    ]
  },
  single: {
    category: 'single',
    activeSeconds: 1800,
    totalQuantity: 30,
    machineCount: 1,
    machines: []
  },
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

async function seedAuthAndApi(page) {
  await page.goto('/app/')
  await page.evaluate(() => {
    const payload = btoa(JSON.stringify({ exp: Math.floor(Date.now() / 1000) + 3600 }))
    localStorage.setItem('authToken', `x.${payload}.y`)
  })
  await page.route('**/config/ApiUrl.json', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ api: {} })
    })
  )
  await page.route('**/api/Data/dashboard/summary*', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(fakeSummary)
    })
  )
}

test('login routes to dashboard and exposes three production sections', async ({ page }) => {
  await seedAuthAndApi(page)

  await page.goto('/app/dashboard')

  await expect(page.getByText('今日总产量')).toBeVisible()
  await expect(page.getByText('联点 · 卷状机台')).toBeVisible()
  await expect(page.getByText('联点 · 单片机台')).toBeVisible()
  await expect(page.getByText('ZPL')).toBeVisible()
  await expect(page.locator('[data-test="category-section"]')).toHaveCount(3)
})

test('deep link to dashboard redirects to login when unauthenticated', async ({ page }) => {
  await page.goto('/app/')
  await page.evaluate(() => localStorage.removeItem('authToken'))

  await page.goto('/app/dashboard')

  await expect(page.locator('[data-test="login-form"]')).toBeVisible()
  expect(new URL(page.url()).pathname).toBe('/app/login')
})
