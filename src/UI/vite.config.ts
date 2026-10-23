import { API_PREFIX } from "./src/constants/apiPrefixes.js";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig, loadEnv, type ProxyOptions } from "vite";

const createServicePrroxy = (prefix: string, targetUrl: string): ProxyOptions => ({
    target: targetUrl,
    changeOrigin: true,
    secure: false,
    rewrite: (path) => path.replace(new RegExp(`^${prefix}`), "/api"),
});

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), "");

    return {
        plugins: [react(), tailwindcss()],
        server: {
            port: 5173,
            proxy: {
                [API_PREFIX.PRODUCT]: createServicePrroxy(API_PREFIX.PRODUCT, env.VITE_PRODUCT_API_URL),
                [API_PREFIX.AUTH]: createServicePrroxy(API_PREFIX.AUTH, env.VITE_AUTH_API_URL),
                [API_PREFIX.ORDER]: createServicePrroxy(API_PREFIX.ORDER, env.VITE_ORDER_API_URL),
            },
        },
    };
});
