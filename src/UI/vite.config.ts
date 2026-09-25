import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true, // Bắt buộc dùng đúng port 5173; nếu port bị kẹt, Vite sẽ báo lỗi ngay lập tức thay vì tự nhảy sang port khác
  },
});
