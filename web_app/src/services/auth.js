const TOKEN_KEY = 'authToken'

function parseJwt(token) {
  try {
    const payload = token.split('.')[1]
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    return JSON.parse(atob(normalized))
  } catch {
    return null
  }
}

export function isAuthenticated() {
  const token = localStorage.getItem(TOKEN_KEY)
  if (!token) {
    return false
  }
  const payload = parseJwt(token)
  if (!payload || typeof payload.exp !== 'number' || payload.exp * 1000 < Date.now()) {
    logout()
    return false
  }
  return true
}

export function login(token) {
  localStorage.setItem(TOKEN_KEY, token)
}

export function logout() {
  localStorage.removeItem(TOKEN_KEY)
}
