<script setup>
defineProps({
  rows: { type: Array, default: () => [] },
  category: { type: String, default: 'machine' }
})

const emit = defineEmits(['row-click'])

function formatSeconds(seconds) {
  const s = Number(seconds) || 0
  const h = Math.floor(s / 3600)
  const m = Math.floor((s % 3600) / 60)
  return h > 0 ? `${h}小时${m}分` : `${m}分`
}

function formatTs(timestamp) {
  return timestamp ? String(timestamp).replace('T', ' ').slice(0, 19) : '-'
}
</script>

<template>
  <div class="table-wrap">
    <table class="production-table" data-test="production-table">
      <thead>
        <tr>
          <template v-if="category === 'zpl'">
            <th>用户</th>
            <th>订单号</th>
            <th>数量</th>
            <th>区域</th>
            <th>更新时间</th>
          </template>
          <template v-else>
            <th>机台</th>
            <th>产量</th>
            <th>有效时长</th>
            <th>状态</th>
            <th>最新时间</th>
          </template>
        </tr>
      </thead>
      <tbody>
        <template v-if="category === 'zpl'">
          <tr
            v-for="row in rows"
            :key="row.userName"
            class="table-row"
            @click="emit('row-click', row)"
          >
            <td>{{ row.userName }}</td>
            <td>{{ row.orderNo }}</td>
            <td>{{ row.quantity }}</td>
            <td>{{ row.region || '-' }}</td>
            <td>{{ formatTs(row.timestamp) }}</td>
          </tr>
        </template>
        <template v-else>
          <tr
            v-for="row in rows"
            :key="row.machineName"
            class="table-row"
            @click="emit('row-click', row)"
          >
            <td>{{ row.machineName }}</td>
            <td>{{ row.productQuantity }}</td>
            <td>{{ formatSeconds(row.activeSeconds) }}</td>
            <td>
              <span class="status" :class="row.state === 1 ? 'status--on' : 'status--off'">
                {{ row.state === 1 ? '开启' : '关闭' }}
              </span>
            </td>
            <td>{{ formatTs(row.timestamp) }}</td>
          </tr>
        </template>
        <tr v-if="!rows.length">
          <td colspan="5" class="empty">暂无数据</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
