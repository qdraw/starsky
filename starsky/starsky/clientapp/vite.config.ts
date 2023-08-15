import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// https://vitejs.dev/config/
/** @type {import('vite').UserConfig} */
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: "build",
    assetsDir: "assets",
    assetsInlineLimit: 0,
    chunkSizeWarningLimit: 600
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
