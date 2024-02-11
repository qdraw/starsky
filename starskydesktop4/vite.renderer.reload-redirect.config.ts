import { defineConfig } from 'vite';
import * as path from 'path';

console.log(path.resolve(__dirname, 'src', 'client', 'pages', 'redirect', 'redirect.html'));

// https://vitejs.dev/config
export default defineConfig({
    root: path.resolve(__dirname, 'src', 'client', 'pages', 'redirect'),
    server: {
        port: 9001
    }
});