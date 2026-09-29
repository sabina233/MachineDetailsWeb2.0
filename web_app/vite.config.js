import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [
    vue(),
    {
      name: 'root-redirect',
      configureServer(server) {
        server.middlewares.use((req, res, next) => {
          if (req.url === '/' || req.url === '') {
            res.writeHead(302, { Location: '/app/' })
            res.end()
            return
          }
          next()
        })
      }
    }
  ],
  base: '/app/',
  server: {
    host: '0.0.0.0',
    port: 5173
  },
  build: {
    outDir: 'wwwroot/app',
    emptyOutDir: true
  },
  test: {
    environment: 'jsdom',
    globals: true,
    exclude: ['**/node_modules/**', '**/dist/**', 'tests/**']
  }
})
