const defaultUrls = {
  dashboard_summary_Url: '/api/Data/dashboard/summary',
  dashboard_liandian_Url: '/api/Data/dashboard/liandian',
  zpl_daily_summary_Url: '/api/zpl/dashboard/daily-summary'
}

let apiConfig = null

export async function ensureConfig() {
  if (apiConfig) {
    return apiConfig
  }
  try {
    const res = await fetch('/config/ApiUrl.json', { cache: 'no-store' })
    if (res.ok) {
      const json = await res.json()
      apiConfig = json.api ?? {}
    }
  } catch {
    // 配置加载失败时保留默认相对地址
  }
  return apiConfig ?? {}
}

export function makeApiUrl(key, params = {}) {
  const base = (apiConfig && apiConfig[key]) || defaultUrls[key]
  const url = new URL(base, window.location.origin)
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== '') {
      url.searchParams.set(k, String(v))
    }
  }
  return url.toString()
}

async function request(key, params) {
  await ensureConfig()
  const url = makeApiUrl(key, params)
  const res = await fetch(url)
  if (!res.ok) {
    throw new Error(`请求失败: ${url} (${res.status})`)
  }
  return res.json()
}

async function detailsRequest(path, params) {
  await ensureConfig()
  const base = (apiConfig && apiConfig.details_Url) || '/api/Data'
  const url = makeUrl(base + path, params)
  const res = await fetch(url)
  if (!res.ok) {
    throw new Error(`请求失败: ${url} (${res.status})`)
  }
  return res.json()
}

function makeUrl(base, params) {
  const url = new URL(base, window.location.origin)
  for (const [k, v] of Object.entries(params)) {
    if (v !== undefined && v !== null && v !== '') {
      url.searchParams.set(k, String(v))
    }
  }
  return url.toString()
}

export const dashboardApi = {
  getSummary: (date) => request('dashboard_summary_Url', { date }),
  getLiandian: (date, category) => request('dashboard_liandian_Url', { date, category }),
  getZplSummary: (date) => request('zpl_daily_summary_Url', { date })
}

export const authApi = {
  login: async (username, password) => {
    await ensureConfig()
    const url = (apiConfig && apiConfig.login_Url) || '/api/Auth/Login'
    const res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    })
    return res.json().catch(() => ({}))
  }
}

export const machineApi = {
  getRealTime: (machineName) => detailsRequest('/RealTimeData', { machineName }),
  getHistory: (machineName, startDate, endDate) => detailsRequest('/HistoryRecords', { machineName, startDate, endDate }),
  getOrderChart: (machineName, startDate, endDate) => detailsRequest('/OrderProductionChart', { machineName, startDate, endDate }),
  getBrandChart: (machineName, startDate, endDate) => detailsRequest('/BrandProductionChart', { machineName, startDate, endDate })
}

export const zplApi = {
  getHistory: (userName, startDate, endDate, page = 1, pageSize = 10) =>
    request('zpl_history_Url', { userName, startDate, endDate, page, pageSize })
}
