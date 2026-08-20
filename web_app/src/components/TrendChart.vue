<script setup>
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

// 在柱顶绘制数值标签
const valueLabels = {
  id: 'valueLabels',
  afterDatasetsDraw(chart) {
    const { ctx } = chart
    chart.data.datasets.forEach((dataset, datasetIndex) => {
      const meta = chart.getDatasetMeta(datasetIndex)
      meta.data.forEach((bar, index) => {
        const value = dataset.data[index]
        if (value === null || value === undefined || value === 0) {
          return
        }
        ctx.save()
        ctx.fillStyle = '#0F3D66'
        ctx.font = "600 11px -apple-system, BlinkMacSystemFont, 'Segoe UI', 'PingFang SC', sans-serif"
        ctx.textAlign = 'center'
        ctx.fillText(String(value), bar.x, bar.y - 7)
        ctx.restore()
      })
    })
  }
}

const props = defineProps({
  labels: { type: Array, default: () => [] },
  values: { type: Array, default: () => [] },
  label: { type: String, default: '产量' }
})

const canvas = ref(null)
let chart = null

function render() {
  if (!canvas.value) {
    return
  }
  const ctx = canvas.value.getContext('2d')
  if (!ctx) {
    return
  }
  if (chart) {
    chart.destroy()
  }

  // 创建柱状图渐变填充
  const gradient = ctx.createLinearGradient(0, 0, 0, 240)
  gradient.addColorStop(0, '#0A84FF')
  gradient.addColorStop(1, '#60A5FA')

  chart = new Chart(ctx, {
    type: 'bar',
    plugins: [valueLabels],
    data: {
      labels: props.labels,
      datasets: [
        {
          label: props.label,
          data: props.values,
          backgroundColor: gradient,
          hoverBackgroundColor: '#0070e0',
          borderRadius: 8,
          borderSkipped: false,
          maxBarThickness: 36
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      layout: { padding: { top: 22, bottom: 4 } },
      plugins: {
        legend: {
          labels: {
            color: '#475569',
            font: { family: "-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif", weight: '600' }
          }
        },
        tooltip: {
          backgroundColor: 'rgba(15, 23, 42, 0.88)',
          padding: 10,
          cornerRadius: 8,
          titleFont: { size: 12, weight: 'bold' },
          bodyFont: { size: 12 }
        }
      },
      scales: {
        x: {
          grid: { color: 'rgba(226, 232, 240, 0.6)' },
          ticks: { color: '#64748B', font: { size: 11 } }
        },
        y: {
          grid: { color: 'rgba(226, 232, 240, 0.6)' },
          ticks: { color: '#64748B', font: { size: 11 } }
        }
      },
      animation: {
        duration: 500,
        easing: 'easeOutQuart'
      }
    }
  })
}

onMounted(render)
watch(() => [props.labels, props.values], render, { deep: true })
onBeforeUnmount(() => {
  if (chart) {
    chart.destroy()
  }
})
</script>

<template>
  <div class="chart card fade-up" data-test="trend-chart">
    <canvas ref="canvas"></canvas>
  </div>
</template>
