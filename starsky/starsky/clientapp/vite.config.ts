import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// https://vitejs.dev/config/
/** @type {import('vite').UserConfig} */
export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      external: [
        "src/hooks/___tests___/intersection-observer-mock.ts",
        "src/hooks/___tests___/test-hook.tsx"
      ]
    },
    outDir: "build",
    assetsDir: "assets",
    assetsInlineLimit: 0,
    chunkSizeWarningLimit: 650
  },
  optimizeDeps: {
    include: ["leaflet", "core-js", "react", "react-dom", "react-router-dom"]
  },
  server: {
    proxy: {
      "/starsky/api": "http://localhost:4000",
      "/starsky/realtime": {
        target: "ws://localhost:4000/starsky/realtime",
        ws: true
      }
    }
  }
});
