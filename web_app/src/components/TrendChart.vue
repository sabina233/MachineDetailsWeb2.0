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
        ctx.font = "600 11px -apple-system, 'Segoe UI', 'Microsoft YaHei', sans-serif"
        ctx.textAlign = 'center'
        ctx.fillText(String(value), bar.x, bar.y - 6)
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
  chart = new Chart(ctx, {
    type: 'bar',
    plugins: [valueLabels],
    data: {
      labels: props.labels,
      datasets: [
        {
          label: props.label,
          data: props.values,
          backgroundColor: '#0A84FF',
          borderRadius: 6,
          maxBarThickness: 34
        }
      ]
    },
      options: {
      responsive: true,
      maintainAspectRatio: false,
      layout: { padding: { top: 18 } },
      plugins: {
        legend: { labels: { color: '#475569' } }
      },
      scales: {
        x: { grid: { color: '#EFF3F9' }, ticks: { color: '#64748B' } },
        y: { grid: { color: '#EFF3F9' }, ticks: { color: '#64748B' } }
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
  <div class="chart card" data-test="trend-chart">
    <canvas ref="canvas"></canvas>
  </div>
</template>
