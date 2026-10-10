/**
 * 区域与国旗图片配置中心
 * 
 * 使用说明：
 * 1. 后续有各个区域/国家的实际国旗或区域图片时，可直接放入 public/images/flags/ 目录下
 *    例如：public/images/flags/CN.png、public/images/flags/VN.png、public/images/flags/US.png
 * 2. 也可在下方的 regionImageMap 中显式配置特定映射（支持本地路径或在线 CDN 地址）
 * 3. 若未放置对应图片或图片加载失败，系统将自动回退到内置的优美国旗/区域徽章矢量图标（SVG）
 */

// 1. 区域中文名称统一映射字典
export const regionNames = {
  CN: '中国',
  US: '美国',
  HP: '美国',
  JP: '日本',
  FR: '法国',
  KR: '韩国',
  VN: '越南',
  GD: '广东',
  HK: '香港',
  TW: '台湾',
  CB: '柬埔寨',
  ID: '印尼',
  TH: '泰国',
  IN: '印度',
  IND: '印度',
  BD: '孟加拉',
  PK: '巴基斯坦',
  SLR: '斯里兰卡',
  TK: '土耳其',
  GB: '英国',
  UK: '英国',
  DE: '德国',
  GER: '德国',
  GT: '危地马拉'
}

// 2. 厂区特定代码到标准 ISO 国家代码的别名映射（用于自动回退找到对应国旗）
export const regionCodeAliases = {
  HP: 'US',
  GER: 'DE',
  UK: 'GB',
  IND: 'IN',
  SLR: 'LK',
  TK: 'TR',
  CB: 'KH'
}

// 3. 用户显式自定义图片路径映射（优先级最高）
// 如需指定特定后缀或在线地址，可在此处配置
export const regionImageMap = {
  // 示例：
  // CN: '/app/images/flags/CN.png',
  // VN: '/app/images/flags/VN.png',
  // US: '/app/images/flags/US.png',
  // GD: '/app/images/flags/GD.png',
}

/**
 * 获取区域或国旗图片的默认基础目录
 */
export function getFlagBaseDir() {
  const base = (typeof import.meta !== 'undefined' && import.meta.env?.BASE_URL) || '/app/'
  return base.replace(/\/$/, '') + '/images/flags'
}

/**
 * 获取指定区域/国家代码对应的图片路径
 * @param {string} rawCode 区域代码（例如 CN, VN, HP, US, GD）
 * @returns {string|null} 图片 URL 路径
 */
export function getRegionImage(rawCode) {
  if (!rawCode) return null
  const code = String(rawCode).trim().toUpperCase()

  // 1. 优先使用用户显式配置的映射
  if (regionImageMap[code]) {
    return regionImageMap[code]
  }

  // 2. 如果存在别名映射且配置了显式图片
  const alias = regionCodeAliases[code]
  if (alias && regionImageMap[alias]) {
    return regionImageMap[alias]
  }

  // 3. 默认尝试规范约定的公用路径：/app/images/flags/{CODE}.png
  const baseDir = getFlagBaseDir()
  return `${baseDir}/${code}.png`
}

/**
 * 获取区域中文显示名称
 * @param {string} code 区域代码
 * @returns {string} 区域名称（若无映射则返回原始代码）
 */
export function getRegionName(code) {
  if (!code) return '未知区域'
  const c = String(code).trim().toUpperCase()
  return regionNames[c] || regionNames[code] || code
}
