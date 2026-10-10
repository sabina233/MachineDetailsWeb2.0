<script setup>
import { computed, ref, watch } from 'vue'
import { getRegionImage, regionCodeAliases, getRegionName } from '../config/regionImages'

const props = defineProps({
  code: {
    type: String,
    required: true
  },
  name: {
    type: String,
    default: ''
  },
  size: {
    type: String,
    default: 'md' // 'sm' | 'md' | 'lg'
  },
  alt: {
    type: String,
    default: ''
  },
  customImage: {
    type: String,
    default: ''
  }
})

const hasPrimaryError = ref(false)
const hasAliasError = ref(false)

const cleanCode = computed(() => String(props.code || '').trim().toUpperCase())
const displayName = computed(() => props.name || getRegionName(cleanCode.value))
const aliasCode = computed(() => regionCodeAliases[cleanCode.value] || null)

// 首选图片路径
const primaryImgUrl = computed(() => {
  if (props.customImage) return props.customImage
  return getRegionImage(cleanCode.value)
})

// 别名备用图片路径（例如 HP 优先找 HP.png，失败后可尝试 US.png）
const aliasImgUrl = computed(() => {
  if (props.customImage) return null
  if (!aliasCode.value) return null
  return getRegionImage(aliasCode.value)
})

// 当前应尝试的图片 URL
const currentImgUrl = computed(() => {
  if (!hasPrimaryError.value && primaryImgUrl.value) {
    return primaryImgUrl.value
  }
  if (!hasAliasError.value && aliasImgUrl.value && aliasImgUrl.value !== primaryImgUrl.value) {
    return aliasImgUrl.value
  }
  return null
})

watch(() => [props.code, props.customImage], () => {
  hasPrimaryError.value = false
  hasAliasError.value = false
})

function onImgError() {
  if (!hasPrimaryError.value) {
    hasPrimaryError.value = true
  } else {
    hasAliasError.value = true
  }
}
</script>

<template>
  <div
    class="region-flag-badge"
    :class="[`region-flag-badge--${size}`]"
    :title="displayName"
  >
    <!-- 真实图片（用户自定义或公用目录放置的图片） -->
    <img
      v-if="currentImgUrl && (!hasPrimaryError || (!hasAliasError && aliasImgUrl))"
      :src="currentImgUrl"
      :alt="alt || displayName"
      class="region-flag-badge__img"
      @error="onImgError"
    />

    <!-- 优雅 SVG 矢量国旗/区域徽章 Fallback -->
    <div v-else class="region-flag-badge__fallback">
      <!-- 中国 (CN / GD / HK / TW) -->
      <svg
        v-if="cleanCode === 'CN' || cleanCode === 'GD' || cleanCode === 'HK' || cleanCode === 'TW'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#de2910" />
        <polygon points="8,5 9.5,9.5 14,9.5 10.5,12 12,16.5 8,14 4,16.5 5.5,12 2,9.5 6.5,9.5" fill="#ffde00" />
        <circle cx="16" cy="6" r="1.2" fill="#ffde00" />
        <circle cx="18" cy="9" r="1.2" fill="#ffde00" />
        <circle cx="18" cy="13" r="1.2" fill="#ffde00" />
        <circle cx="16" cy="16" r="1.2" fill="#ffde00" />
      </svg>

      <!-- 越南 (VN) -->
      <svg
        v-else-if="cleanCode === 'VN'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#da251d" />
        <polygon points="20,7 22.5,14 30,14 24,18 26.5,25 20,20.5 13.5,25 16,18 10,14 17.5,14" fill="#ffff00" />
      </svg>

      <!-- 美国 (US / HP) -->
      <svg
        v-else-if="cleanCode === 'US' || cleanCode === 'HP'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#b22234" />
        <line x1="0" y1="4" x2="40" y2="4" stroke="#ffffff" stroke-width="2.5" />
        <line x1="0" y1="9" x2="40" y2="9" stroke="#ffffff" stroke-width="2.5" />
        <line x1="0" y1="14" x2="40" y2="14" stroke="#ffffff" stroke-width="2.5" />
        <line x1="0" y1="19" x2="40" y2="19" stroke="#ffffff" stroke-width="2.5" />
        <line x1="0" y1="24" x2="40" y2="24" stroke="#ffffff" stroke-width="2.5" />
        <rect width="18" height="15" rx="3" fill="#3c3b6e" />
        <circle cx="5" cy="5" r="1.2" fill="#ffffff" />
        <circle cx="13" cy="5" r="1.2" fill="#ffffff" />
        <circle cx="9" cy="8" r="1.2" fill="#ffffff" />
        <circle cx="5" cy="11" r="1.2" fill="#ffffff" />
        <circle cx="13" cy="11" r="1.2" fill="#ffffff" />
      </svg>

      <!-- 菲律宾 (PH) -->
      <svg
        v-else-if="cleanCode === 'PH'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="14" rx="4" fill="#0038a8" />
        <rect y="14" width="40" height="14" rx="4" fill="#ce1126" />
        <polygon points="0,0 20,14 0,28" fill="#ffffff" />
        <circle cx="7" cy="14" r="3" fill="#fcd116" />
      </svg>

      <!-- 印度尼西亚 (ID) -->
      <svg
        v-else-if="cleanCode === 'ID'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="14" rx="4" fill="#ff0000" />
        <rect y="14" width="40" height="14" rx="4" fill="#ffffff" />
      </svg>

      <!-- 印度 (IN / IND) -->
      <svg
        v-else-if="cleanCode === 'IN' || cleanCode === 'IND'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="9.3" rx="4" fill="#ff9933" />
        <rect y="9.3" width="40" height="9.3" fill="#ffffff" />
        <rect y="18.6" width="40" height="9.4" rx="4" fill="#138808" />
        <circle cx="20" cy="14" r="3.5" fill="none" stroke="#000080" stroke-width="1" />
      </svg>

      <!-- 孟加拉 (BD) -->
      <svg
        v-else-if="cleanCode === 'BD'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#006a4e" />
        <circle cx="18" cy="14" r="8" fill="#f42a41" />
      </svg>

      <!-- 巴基斯坦 (PK) -->
      <svg
        v-else-if="cleanCode === 'PK'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#01411c" />
        <rect width="10" height="28" rx="4" fill="#ffffff" />
        <circle cx="25" cy="14" r="6" fill="#ffffff" />
        <circle cx="27" cy="13" r="5" fill="#01411c" />
        <polygon points="26,10 27,13 30,13 28,15 29,18 26,16 23,18 24,15 22,13 25,13" fill="#ffffff" />
      </svg>

      <!-- 柬埔寨 (KH / CB) -->
      <svg
        v-else-if="cleanCode === 'KH' || cleanCode === 'CB'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="7" rx="4" fill="#032ea1" />
        <rect y="7" width="40" height="14" fill="#e00025" />
        <rect y="21" width="40" height="7" rx="4" fill="#032ea1" />
        <rect x="15" y="10" width="10" height="8" rx="1" fill="#ffffff" />
      </svg>

      <!-- 泰国 (TH) -->
      <svg
        v-else-if="cleanCode === 'TH'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="5" rx="4" fill="#a51931" />
        <rect y="5" width="40" height="5" fill="#f4f5f8" />
        <rect y="10" width="40" height="8" fill="#2d2a4a" />
        <rect y="18" width="40" height="5" fill="#f4f5f8" />
        <rect y="23" width="40" height="5" rx="4" fill="#a51931" />
      </svg>

      <!-- 土耳其 (TR / TK) -->
      <svg
        v-else-if="cleanCode === 'TR' || cleanCode === 'TK'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#e30a17" />
        <circle cx="17" cy="14" r="7" fill="#ffffff" />
        <circle cx="19" cy="14" r="5.6" fill="#e30a17" />
        <polygon points="23,14 26,15 24,12 26,10 23,11 21,9 22,12 20,14 23,13" fill="#ffffff" />
      </svg>

      <!-- 斯里兰卡 (LK / SLR) -->
      <svg
        v-else-if="cleanCode === 'LK' || cleanCode === 'SLR'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#ffbe29" />
        <rect x="4" y="4" width="7" height="20" fill="#00534e" />
        <rect x="11" y="4" width="7" height="20" fill="#eb7400" />
        <rect x="20" y="4" width="16" height="20" fill="#8d153a" />
        <circle cx="28" cy="14" r="4" fill="#ffbe29" />
      </svg>

      <!-- 英国 (GB / UK) -->
      <svg
        v-else-if="cleanCode === 'GB' || cleanCode === 'UK'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#012169" />
        <path d="M0,0 L40,28 M40,0 L0,28" stroke="#ffffff" stroke-width="4" />
        <path d="M0,0 L40,28 M40,0 L0,28" stroke="#c8102e" stroke-width="2" />
        <path d="M20,0 V28 M0,14 H40" stroke="#ffffff" stroke-width="7" />
        <path d="M20,0 V28 M0,14 H40" stroke="#c8102e" stroke-width="4" />
      </svg>

      <!-- 日本 (JP) -->
      <svg
        v-else-if="cleanCode === 'JP'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#ffffff" stroke="#e2e8f0" stroke-width="1" />
        <circle cx="20" cy="14" r="7" fill="#bc002d" />
      </svg>

      <!-- 德国 (DE / GER) -->
      <svg
        v-else-if="cleanCode === 'DE' || cleanCode === 'GER'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#ffce00" />
        <rect width="40" height="9.5" rx="3" fill="#000000" />
        <rect y="9.3" width="40" height="9.5" fill="#dd0000" />
      </svg>

      <!-- 法国 (FR) -->
      <svg
        v-else-if="cleanCode === 'FR'"
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <rect width="40" height="28" rx="4" fill="#ed2939" />
        <rect width="26.6" height="28" rx="4" fill="#ffffff" />
        <rect width="13.3" height="28" rx="4" fill="#002395" />
      </svg>

      <!-- 通用国旗徽章（展示区域代码） -->
      <svg
        v-else
        viewBox="0 0 40 28"
        class="flag-svg"
      >
        <defs>
          <linearGradient id="flagBg" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stop-color="#0a84ff" />
            <stop offset="100%" stop-color="#0056b3" />
          </linearGradient>
        </defs>
        <rect width="40" height="28" rx="4" fill="url(#flagBg)" />
        <rect x="2" y="2" width="36" height="24" rx="3" fill="none" stroke="rgba(255,255,255,0.25)" stroke-width="1" />
        <text
          x="20"
          y="18"
          text-anchor="middle"
          fill="#ffffff"
          font-size="10"
          font-weight="bold"
          letter-spacing="0.5"
          font-family="system-ui, sans-serif"
        >
          {{ cleanCode.slice(0, 3) }}
        </text>
      </svg>
    </div>
  </div>
</template>

<style scoped>
.region-flag-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
  border-radius: 8px;
  background: #f8fafc;
  box-shadow: 0 2px 6px rgba(15, 23, 42, 0.08);
  border: 1px solid rgba(148, 163, 184, 0.2);
  flex-shrink: 0;
  transition: transform 0.25s ease, box-shadow 0.25s ease;
}

.region-flag-badge:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(15, 23, 42, 0.12);
}

.region-flag-badge--sm {
  width: 32px;
  height: 22px;
  border-radius: 5px;
}

.region-flag-badge--md {
  width: 48px;
  height: 34px;
  border-radius: 7px;
}

.region-flag-badge--lg {
  width: 64px;
  height: 44px;
  border-radius: 9px;
}

.region-flag-badge__img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.region-flag-badge__fallback {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.flag-svg {
  width: 100%;
  height: 100%;
  display: block;
}
</style>
