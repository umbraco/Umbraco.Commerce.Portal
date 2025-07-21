import { defineConfig } from "vite";
import tsconfigPaths from "vite-tsconfig-paths";

export default defineConfig({
  build: {
    lib: {
      entry: {
        "ucportal.backoffice": "src/backoffice/index.ts",
        "ucportal:auth": "src/css/index.ts"
      },
      formats: ["es"],
    },
    outDir: "../wwwroot/",
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },
  plugins: [
    tsconfigPaths()
  ]
});
