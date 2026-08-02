import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// GitHub Pages serves at https://<user>.github.io/FitCore/
export default defineConfig({
  base: '/FitCore/',
  plugins: [react(), tailwindcss()],
})
