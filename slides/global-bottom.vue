<script setup>
import { computed } from 'vue'
import { useNav } from '@slidev/client'

const { currentLayout, currentFrontmatter } = useNav()

const textColor = computed(() =>
  currentFrontmatter.value.codeSlide ? 'var(--sv-green)' : 'var(--sv-cream)'
)
</script>

<template>
  <div
    v-if="currentLayout !== 'cover'"
    class="absolute inset-x-0 bottom-0 flex justify-between items-center px-8 py-3 pointer-events-none z-40 text-sm font-semibold"
    :style="{ color: textColor }"
  >
    <span>#sharingiscaring</span>
    <span>Yoan Thirion</span>
  </div>
</template>
