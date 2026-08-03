<script setup>
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { Chart, registerables } from 'chart.js'

Chart.register(...registerables)

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
    data: {
      labels: props.labels,
      datasets: [{ label: props.label, data: props.values, backgroundColor: '#2f6fed' }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { labels: { color: '#ffffff' } }
      },
      scales: {
        x: { ticks: { color: '#9fa3c9' } },
        y: { ticks: { color: '#9fa3c9' } }
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
