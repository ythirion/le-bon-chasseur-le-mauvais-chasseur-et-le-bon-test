<template>
  <div class="w-full h-full relative">
    <img src="/cover.webp" class="absolute inset-0 w-full h-full object-cover" />
    <div class="relative z-10 w-full h-full">
      <slot />
    </div>
  </div>
</template>
