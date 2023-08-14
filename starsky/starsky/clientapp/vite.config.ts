import react from "@vitejs/plugin-react-swc";
import { defineConfig } from "vite";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: "./build",
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
