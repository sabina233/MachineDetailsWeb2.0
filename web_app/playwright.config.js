import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './tests/smoke',
  timeout: 60000,
  retries: 0,
  use: {
    baseURL: 'http://localhost:5200'
  },
  webServer: {
    command: 'dotnet run --project ../web_app/web_app.csproj --urls http://localhost:5200',
    url: 'http://localhost:5200/app/',
    reuseExistingServer: true,
    timeout: 90000
  },
  projects: [{ name: 'chromium', use: { browserName: 'chromium' } }]
})
