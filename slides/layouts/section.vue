<template>
  <div class="w-full h-full relative section-slide flex flex-col justify-center px-20 py-16">
    <div class="accent-corner" />
    <div class="relative z-10">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.section-slide {
  background: var(--sv-green);
  color: var(--sv-cream);
}

.section-slide :deep(h1) {
  font-size: 3rem;
  color: var(--sv-cream);
  margin-bottom: 0.6em;
}

.section-slide :deep(p),
.section-slide :deep(li) {
  font-size: 1.15rem;
  max-width: 42rem;
  line-height: 1.65;
}

.section-slide :deep(ul) {
  margin-top: 1em;
}

.accent-corner {
  position: absolute;
  top: 0;
  right: 18%;
  width: 10%;
  height: 12%;
  background: var(--sv-yellow);
}
</style>
