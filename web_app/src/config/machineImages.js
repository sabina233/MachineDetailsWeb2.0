/**
 * 机台图片与图标配置中心
 * 
 * 使用说明：
 * 1. 后续有各个机台的实际图片时，可将图片放入 public/images/machines/ 目录下（例如 public/images/machines/RFID-601.png）
 * 2. 在下面的 machineImageMap 中配置机台名称与图片路径的对应关系
 * 3. 若机台未配置图片或图片加载失败，系统将自动回退到对应的分类矢量图标（SVG）
 */

// 1. 精确机台图片映射（机台名称 => 图片路径或在线URL）
export const machineImageMap = {
  // 示例：
  // 'RFID-601': '/app/images/machines/RFID-601.png',
  // 'RFID-602': '/app/images/machines/RFID-602.png',
  // 'RFID-801': '/app/images/machines/RFID-801.png',
}

// 2. 机台型号/前缀默认图片映射（型号前缀 => 默认图片路径）
export const machineTypeDefaultImages = {
  // 'RFID-6': '/app/images/machines/default-roll.png',
  // 'RFID-8': '/app/images/machines/default-single.png',
}

/**
 * 获取机台自定义图片路径
 * @param {string} machineName 机台名称
 * @returns {string|null} 图片路径，如未配置则返回 null
 */
export function getMachineImage(machineName) {
  if (!machineName) return null

  // 1. 优先匹配精确机台名
  if (machineImageMap[machineName]) {
    return machineImageMap[machineName]
  }

  // 2. 其次匹配前缀（例如 RFID-6, RFID-8）
  for (const [prefix, url] of Object.entries(machineTypeDefaultImages)) {
    if (machineName.toUpperCase().startsWith(prefix.toUpperCase())) {
      return url
    }
  }

  return null
}

/**
 * 根据机台名称获取机台分类
 * @param {string} machineName
 * @returns {'roll' | 'single' | 'other'}
 */
export function getMachineCategory(machineName) {
  if (/^RFID-6/i.test(machineName || '')) return 'roll'
  if (/^RFID-8/i.test(machineName || '')) return 'single'
  return 'other'
}

/**
 * 获取分类显示中文名
 * @param {string} machineName
 * @returns {string}
 */
export function getMachineCategoryName(machineName) {
  const cat = getMachineCategory(machineName)
  if (cat === 'roll') return '卷状机台'
  if (cat === 'single') return '单片机台'
  return '通用机台'
}
