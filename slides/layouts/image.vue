<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps({
  image: {
    type: String,
  },
  backgroundSize: {
    type: String,
    default: 'cover',
  },
  link: {
    type: String,
  },
})

const style = computed(() => ({
  backgroundImage: props.image ? `url("${props.image}")` : undefined,
  backgroundRepeat: 'no-repeat',
  backgroundPosition: 'center',
  backgroundSize: props.backgroundSize,
}))
</script>

<template>
  <div class="slidev-layout w-full h-full relative" :style="style">
    <a v-if="link" :href="link" target="_blank" class="link-overlay absolute inset-0" />
    <slot />
  </div>
</template>

<style scoped>
.link-overlay {
  color: transparent;
  outline: none;
  text-decoration: none;
  border: none;
}
</style>
