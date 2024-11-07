import { defineConfig, UserConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig(({ mode }) => {

    let outDir = resolve(__dirname, '..', 'VoltProjects.Server', 'wwwroot');
    if(mode === 'production-dist')
        outDir = resolve(__dirname, 'dist');

    //Base vite config
    const config: UserConfig = {
        build: {
            //No minify in dev builds, speeds shit up
            minify: false,
            emptyOutDir: true,
            sourcemap: 'inline',
            rollupOptions: {
                input: {
                    main: resolve(__dirname, 'src/main.ts'),
                    search: resolve(__dirname, 'src/search.ts'),
                    admin: resolve(__dirname, 'src/admin.ts'),
                },
                output: {
                    dir: outDir,
                    entryFileNames: () => 'js/[name].js',
                    chunkFileNames: () => 'js/[name].[hash].js',
                    assetFileNames: () => 'assets/[name][extname]'
                }
            }
        },
    };
    
    //In non-dev builds, we will use terser to minify everything
    if (mode != 'development') {
        console.log('Non-dev build, using terser...');
        config.build!.cssMinify = 'esbuild';
        config.build!.minify = 'terser';
        config.build!.sourcemap = false;
        config.build!.terserOptions = {
            format: {
                comments: false,
            },
            compress: {
                defaults: true,
                drop_console: true,
                drop_debugger: true
            },
            mangle: true,
            ecma: 2020
        };
    }

    return config;
});
