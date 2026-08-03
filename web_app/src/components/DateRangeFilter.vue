<script setup>
import { ref } from 'vue'

const props = defineProps({
  start: { type: String, default: '' },
  end: { type: String, default: '' }
})

const emit = defineEmits(['query'])

const today = () => new Date().toISOString().slice(0, 10)
const start = ref(props.start || today())
const end = ref(props.end || today())

function submit() {
  emit('query', { start: start.value, end: end.value })
}
</script>

<template>
  <div class="date-filter" data-test="date-filter">
    <label>开始 <input type="date" v-model="start" /></label>
    <label>结束 <input type="date" v-model="end" /></label>
    <button type="button" class="btn" @click="submit">查询</button>
  </div>
</template>
