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
      // IMPORTANT: Order matters in Vite proxy config. More specific patterns should come first.
      // Proxy tenant-specific API paths BEFORE the global /starsky/api rule
      // (e.g., /main/api -> http://localhost:4000/main/api)
      "^/[a-z][a-z0-9-]*/api": "http://localhost:4000",
      // Proxy tenant-specific realtime paths
      // (e.g., /main/realtime -> ws://localhost:4000/main/realtime)
      "^/[a-z][a-z0-9-]*/realtime": {
        target: "ws://localhost:4000",
        ws: true
      },
      // Global API paths: /starsky/api/* after tenant-specific patterns have been checked
      "/starsky/api": "http://localhost:4000",
      "/starsky/realtime": {
        target: "ws://localhost:4000",
        ws: true
      }
    }
  }
});
