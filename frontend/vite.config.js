import { defineConfig } from 'vite'
import { resolve } from 'path'
import { cpSync, existsSync } from 'fs'

const publicRoot = resolve(__dirname, 'public')

/** Копируем HTML-фрагменты маршрутизации; иначе при publicDir === root сборка затирает обработанный index.html сырцом и снова подключает /js/app.js с import CSS → MIME error под nginx. */
function copyPartialsPlugin() {
  return {
    name: 'copy-partials',
    closeBundle() {
      const src = resolve(publicRoot, 'partials')
      const dest = resolve(__dirname, 'dist/partials')
      if (existsSync(src)) {
        cpSync(src, dest, { recursive: true })
      }
    },
  }
}

/** Локальная разработка: `npm run dev` → :3000, gateway обычно :5000 */
export default defineConfig({
  root: publicRoot,
  publicDir: false,
  plugins: [copyPartialsPlugin()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, '/api'),
      },
    },
  },
  build: {
    outDir: resolve(__dirname, 'dist'),
    emptyOutDir: true,
    rollupOptions: {
      input: {
        main: resolve(publicRoot, 'index.html'),
      },
      output: {
        entryFileNames: 'js/[name].js',
      },
    },
  },
})
