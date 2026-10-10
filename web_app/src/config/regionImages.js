/**
 * 区域与国旗图片配置中心
 * 
 * 使用说明：
 * 1. 后续有各个区域/国家的实际国旗或区域图片时，可直接放入 public/images/flags/ 目录下
 *    例如：public/images/flags/CN.png、public/images/flags/VN.png、public/images/flags/US.png
 * 2. 也可以在下方的 regionImageMap 中显式配置特定映射（支持本地路径或在线 CDN 地址）
 * 3. 实际生产环境中 ZPL 的原始厂区/区域（Region 字段）与目标国家和国旗的对应关系定义在 zplRegionMapping 中，
 *    后续若新增厂区，直接在 zplRegionMapping 中添加一行即可！
 */

// 1. 区域中文名称统一映射字典（代码 => 中文名称）
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
  KH: '柬埔寨',
  ID: '印尼',
  TH: '泰国',
  IN: '印度',
  IND: '印度',
  BD: '孟加拉',
  PK: '巴基斯坦',
  SLR: '斯里兰卡',
  LK: '斯里兰卡',
  TK: '土耳其',
  TR: '土耳其',
  GB: '英国',
  UK: '英国',
  DE: '德国',
  GER: '德国',
  GT: '危地马拉',
  PH: '菲律宾'
}

// 2. 国家名称到标准国旗图标/图片代号映射
export const countryFlagMap = {
  '中国': 'CN',
  '越南': 'VN',
  '美国': 'US',
  '英国': 'GB',
  '斯里兰卡': 'LK',
  '孟加拉': 'BD',
  '柬埔寨': 'KH',
  '印度尼西亚': 'ID',
  '印尼': 'ID',
  '印度': 'IN',
  '土耳其': 'TR',
  '泰国': 'TH',
  '菲律宾': 'PH',
  '巴基斯坦': 'PK',
  '日本': 'JP',
  '韩国': 'KR',
  '法国': 'FR',
  '德国': 'DE',
  '广东': 'GD',
  '香港': 'HK',
  '台湾': 'TW',
  '危地马拉': 'GT'
}

// 3. 厂区特定代码到标准 ISO 国家代码的别名映射（用于自动回退找到对应国旗）
export const regionCodeAliases = {
  HP: 'US',
  GER: 'DE',
  UK: 'GB',
  IND: 'IN',
  SLR: 'LK',
  TK: 'TR',
  CB: 'KH'
}

// 4. 用户显式自定义图片路径映射（优先级最高）
// 如需指定特定后缀或在线地址，可在此处配置，例如：CN: '/app/images/flags/CN.png'
export const regionImageMap = {}

/**
 * 5. 【核心配置】ZPL 生产环境实际区域对应关系与国旗配置
 * 
 * 键（Key）：机台数据库中 ZPL 回传的原始区域字段字符串（Region）
 * 值（Value）：
 *   - 可以直接写国家名称（字符串，如 '越南'、'中国'），系统会自动匹配对应国旗
 *   - 也可以写成对象格式，灵活自定义所属地区/国旗代号/独立图片路径：
 *     { country: '越南', flag: 'VN', image: '/app/images/flags/custom.png' }
 * 
 * 后续需要添加新的区域对应关系时，直接在下方添加对应键值即可！
 */
export const zplRegionMapping = {
  // 越南 (Vietnam)
  'Vietnam Maxim': { country: '越南', flag: 'VN' },
  'IPPS-VN': { country: '越南', flag: 'VN' },

  // 中国 (China)
  'Shanghai Maxim': { country: '中国', flag: 'CN' },
  'Shanghai/Zhejiang Maxim': { country: '中国', flag: 'CN' },
  'IPPS-Puma-FHKAS': { country: '中国', flag: 'CN' },
  'IPPS-Puma-FCNFF': { country: '中国', flag: 'CN' },

  // 斯里兰卡 (Sri Lanka)
  'SriLanka Maxim': { country: '斯里兰卡', flag: 'LK' },

  // 孟加拉 (Bangladesh)
  'Bangladesh Maxim': { country: '孟加拉', flag: 'BD' },

  // 柬埔寨 (Cambodia)
  'Cambodia Maxim': { country: '柬埔寨', flag: 'KH' },

  // 印度尼西亚 (Indonesia)
  'Indonesia Maxim': { country: '印度尼西亚', flag: 'ID' },

  // 印度 (India)
  'India Maxim': { country: '印度', flag: 'IN' },

  // 土耳其 (Turkey)
  'Turkey Maxim': { country: '土耳其', flag: 'TR' },

  // 泰国 (Thailand)
  'Thailand Maxim': { country: '泰国', flag: 'TH' },

  // 菲律宾 (Philippines)
  'IPPS-PH': { country: '菲律宾', flag: 'PH' },

  // 巴基斯坦 (Pakistan)
  'Pakistan Maxim': { country: '巴基斯坦', flag: 'PK' },

  // 美国 (USA)
  'IPPS-Dicks-BCS CC Creations': { country: '美国', flag: 'US' },
  'IPPS-Dicks-BCS Staples Promotional Products': { country: '美国', flag: 'US' },

  // 英国 (UK)
  'UK Maxim': { country: '英国', flag: 'GB' }
}

/**
 * 解析 ZPL 原始区域名称，获取归属地区/国家、国旗代码以及自定义图片
 * @param {string} rawRegion ZPL 记录中的 Region 原始字符串
 * @returns {{ country: string, flag: string, rawRegion: string, image: string|null }}
 */
export function resolveZplRegion(rawRegion) {
  if (!rawRegion) {
    return { country: '未知区域', flag: 'OTHER', rawRegion: '', image: null }
  }
  const trimmed = String(rawRegion).trim()

  // 1. 精确匹配 zplRegionMapping
  const matched = zplRegionMapping[trimmed]
  if (matched) {
    if (typeof matched === 'string') {
      const flag = countryFlagMap[matched] || 'OTHER'
      return { country: matched, flag, rawRegion: trimmed, image: null }
    }
    return {
      country: matched.country || trimmed,
      flag: matched.flag || countryFlagMap[matched.country] || 'OTHER',
      rawRegion: trimmed,
      image: matched.image || null
    }
  }

  // 2. 尝试大小写不敏感匹配
  const lowerKey = trimmed.toLowerCase()
  for (const [key, val] of Object.entries(zplRegionMapping)) {
    if (key.toLowerCase() === lowerKey) {
      if (typeof val === 'string') {
        const flag = countryFlagMap[val] || 'OTHER'
        return { country: val, flag, rawRegion: trimmed, image: null }
      }
      return {
        country: val.country || trimmed,
        flag: val.flag || countryFlagMap[val.country] || 'OTHER',
        rawRegion: trimmed,
        image: val.image || null
      }
    }
  }

  // 3. 尝试模糊匹配已知国家名称
  for (const [cName, fCode] of Object.entries(countryFlagMap)) {
    if (trimmed.includes(cName) || trimmed.toUpperCase().includes(fCode)) {
      return { country: cName, flag: fCode, rawRegion: trimmed, image: null }
    }
  }

  return { country: trimmed, flag: 'OTHER', rawRegion: trimmed, image: null }
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
