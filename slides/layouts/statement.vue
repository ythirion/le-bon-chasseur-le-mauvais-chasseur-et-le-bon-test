<template>
  <div class="w-full h-full relative statement-slide flex items-center justify-center px-10">
    <div class="accent-corner" />
    <div class="statement-card">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.statement-slide {
  background: var(--sv-green);
}

.accent-corner {
  position: absolute;
  top: 0;
  right: 30%;
  width: 15%;
  height: 10%;
  background: var(--sv-yellow);
}

.statement-card {
  background: var(--sv-cream);
  color: var(--sv-green);
  padding: 3rem 4rem;
  max-width: 56rem;
  text-align: center;
}

.statement-card :deep(h1) {
  font-size: 2.25rem;
  margin-bottom: 0.8rem;
}

.statement-card :deep(p) {
  font-size: 1.3rem;
  line-height: 1.7;
}
</style>
