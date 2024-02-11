import { defineConfig } from 'vite';
import * as path from 'path';

console.log(path.resolve(__dirname, 'src', 'client', 'pages', 'splash', 'splash.html'));

// https://vitejs.dev/config
export default defineConfig({
    root: path.resolve(__dirname, 'src', 'client', 'pages', 'splash'),
    server: {
        port: 9002
    }
});