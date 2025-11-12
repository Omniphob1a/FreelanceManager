import { defineConfig } from 'vite'
import { resolve } from 'path'

export default defineConfig({
  root: resolve(__dirname, 'public'),
  publicDir: resolve(__dirname, 'public'),
  server: {
    port: 3000,
    proxy: {
      // все запросы к /api/... будут проксироваться на бэк
      '/api': {
        target: 'http://localhost:5000', 
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/api/, '/api') 
      }
    }
  },
  build: {
    outDir: resolve(__dirname, 'dist'),
    emptyOutDir: true,
    rollupOptions: {
      input: {
        main: resolve(__dirname, 'public/index.html'),
      },
      output: {
        entryFileNames: 'js/[name].js',
      }
    },
  },
})
