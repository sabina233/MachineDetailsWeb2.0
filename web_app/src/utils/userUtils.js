/**
 * 用户数据处理工具函数
 */

/**
 * 忽略大小写对用户列表去重并合并产量
 * 针对机台登录时将用户名输入成不同大小写（如 ADMIN vs admin）的实际情况，
 * 将其识别为同一个用户并合并其结案数量。
 * 
 * @param {Array} users 原始用户数据列表 [{ userName, quantity, orderNo, region, timestamp }, ...]
 * @returns {Array} 去重合并后的用户列表，按产量降序排列
 */
export function deduplicateUsers(users = []) {
  if (!Array.isArray(users)) return []

  const map = new Map()

  for (const item of users) {
    if (!item) continue
    const rawName = String(item.userName || '').trim()
    if (!rawName) continue

    const key = rawName.toLowerCase()
    const qty = Number(item.quantity) || 0

    if (!map.has(key)) {
      map.set(key, {
        ...item,
        userName: rawName,
        quantity: qty,
        region: (item.region || '').trim(),
        aliases: [rawName]
      })
    } else {
      const existing = map.get(key)
      existing.quantity += qty

      if (!existing.aliases.includes(rawName)) {
        existing.aliases.push(rawName)
      }

      // 如果当前名字包含大写字母而之前保存的都是小写，优先保留大写或驼峰显示
      const currentHasUpper = /[A-Z]/.test(rawName)
      const existingHasUpper = /[A-Z]/.test(existing.userName)
      if (currentHasUpper && !existingHasUpper) {
        existing.userName = rawName
      }

      // 补充缺失的 region
      if (!existing.region && item.region) {
        existing.region = String(item.region).trim()
      }

      // 保留最新的时间戳与订单号
      if (item.timestamp && (!existing.timestamp || new Date(item.timestamp) > new Date(existing.timestamp))) {
        existing.timestamp = item.timestamp
        if (item.orderNo) existing.orderNo = item.orderNo
      }
    }
  }

  return [...map.values()].sort((a, b) => b.quantity - a.quantity)
}
