import { defineConfig } from 'vite';
export default defineConfig({
    build: {
        lib: {
            entry: ["src/dashboardtraining.ts", "src/icons-dictionary.ts", "src/propertyeditortraining.ts", "src/advancedpropertyeditortraining.ts"], // your web component source file
            formats: ["es"]
        },
        outDir: "dist", // your web component will be saved in this location
        sourcemap: true,
        rollupOptions: {
            external: [/^@umbraco/],
        },
    },
});