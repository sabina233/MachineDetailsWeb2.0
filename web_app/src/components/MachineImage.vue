<script setup>
import { computed, ref, watch } from 'vue'
import { getMachineCategory, getMachineImage } from '../config/machineImages'

const props = defineProps({
  machineName: {
    type: String,
    required: true
  },
  alt: {
    type: String,
    default: ''
  }
})

const hasError = ref(false)

const customImgUrl = computed(() => getMachineImage(props.machineName))
const category = computed(() => getMachineCategory(props.machineName))

watch(() => props.machineName, () => {
  hasError.value = false
})

function onError() {
  hasError.value = true
}
</script>

<template>
  <div class="machine-image-wrapper" :class="`machine-image-wrapper--${category}`">
    <!-- 用户自定义图片 -->
    <img
      v-if="customImgUrl && !hasError"
      :src="customImgUrl"
      :alt="alt || machineName"
      class="machine-image__img"
      @error="onError"
    />

    <!-- 优雅 Fallback: 矢量设备图标 -->
    <div v-else class="machine-image__fallback">
      <!-- 卷状机台图标 (Roll-to-roll) -->
      <svg
        v-if="category === 'roll'"
        class="machine-image__icon"
        viewBox="0 0 64 64"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <rect x="6" y="10" width="52" height="44" rx="8" class="svg-frame" stroke-width="2.5" />
        <circle cx="20" cy="28" r="8" class="svg-roller" stroke-width="2.5" />
        <circle cx="20" cy="28" r="3" class="svg-accent-fill" />
        <circle cx="44" cy="36" r="8" class="svg-roller" stroke-width="2.5" />
        <circle cx="44" cy="36" r="3" class="svg-accent-fill" />
        <path d="M20 20C32 20 32 44 44 44" class="svg-tape" stroke-width="2.5" stroke-linecap="round" />
        <line x1="12" y1="46" x2="28" y2="46" class="svg-detail" stroke-width="2" stroke-linecap="round" />
        <circle cx="48" cy="18" r="2" class="svg-dot" />
        <circle cx="52" cy="18" r="2" class="svg-dot-active" />
      </svg>

      <!-- 单片机台图标 (Single Sheet / Piece) -->
      <svg
        v-else-if="category === 'single'"
        class="machine-image__icon"
        viewBox="0 0 64 64"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <rect x="6" y="10" width="52" height="44" rx="8" class="svg-frame" stroke-width="2.5" />
        <rect x="18" y="20" width="28" height="18" rx="3" class="svg-roller" stroke-width="2.5" />
        <path d="M22 26H36" class="svg-detail" stroke-width="2" stroke-linecap="round" />
        <path d="M22 31H30" class="svg-detail" stroke-width="2" stroke-linecap="round" />
        <path d="M12 44H52" class="svg-tape" stroke-width="2.5" stroke-linecap="round" />
        <circle cx="44" cy="29" r="3" class="svg-accent-fill" />
        <circle cx="48" cy="18" r="2" class="svg-dot" />
        <circle cx="52" cy="18" r="2" class="svg-dot-active" />
      </svg>

      <!-- 通用工控机台图标 (General Machine) -->
      <svg
        v-else
        class="machine-image__icon"
        viewBox="0 0 64 64"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <rect x="8" y="8" width="48" height="48" rx="8" class="svg-frame" stroke-width="2.5" />
        <circle cx="32" cy="28" r="11" class="svg-roller" stroke-width="2.5" />
        <path d="M32 21V28L37 31" class="svg-accent" stroke-width="2.5" stroke-linecap="round" />
        <rect x="16" y="44" width="32" height="4" rx="2" class="svg-detail" stroke-width="1.5" />
        <circle cx="16" cy="16" r="2" class="svg-dot-active" />
      </svg>

      <!-- 类别光标提示 -->
      <div class="machine-image__badge-overlay">
        <span>{{ category === 'roll' ? '卷状设备' : category === 'single' ? '单片设备' : '智能机台' }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.machine-image-wrapper {
  position: relative;
  width: 100%;
  height: 120px;
  border-radius: 12px;
  overflow: hidden;
  background: linear-gradient(135deg, #f0f7ff 0%, #e2efff 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(10, 132, 255, 0.08);
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.machine-image-wrapper--single {
  background: linear-gradient(135deg, #f3f7fd 0%, #e5eefb 100%);
  border-color: rgba(59, 130, 246, 0.1);
}

.machine-image-wrapper--other {
  background: linear-gradient(135deg, #f8fafc 0%, #eef2f6 100%);
}

.machine-image__img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.machine-image__fallback {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  padding: 12px;
}

.machine-image__icon {
  width: 76px;
  height: 76px;
  filter: drop-shadow(0 4px 8px rgba(10, 132, 255, 0.15));
  transition: transform 0.35s cubic-bezier(0.16, 1, 0.3, 1);
}

.svg-frame {
  stroke: #0a84ff;
  fill: #ffffff;
  fill-opacity: 0.85;
}

.svg-roller {
  stroke: #0f3d66;
  fill: #eaf4ff;
}

.svg-accent-fill {
  fill: #0a84ff;
}

.svg-tape {
  stroke: #0a84ff;
}

.svg-accent {
  stroke: #0a84ff;
}

.svg-detail {
  stroke: #94a3b8;
  fill: #94a3b8;
}

.svg-dot {
  fill: #cbd5e1;
}

.svg-dot-active {
  fill: #16a34a;
}

.machine-image__badge-overlay {
  position: absolute;
  bottom: 8px;
  right: 8px;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(6px);
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 600;
  color: #0f3d66;
  border: 1px solid rgba(226, 232, 240, 0.8);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.04);
}
</style>
