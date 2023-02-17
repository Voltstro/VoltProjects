import {defineConfig, UserConfig } from 'vite'
import { webfontDownload } from 'vite-plugin-webfont-dl';
import { resolve } from 'path'

export default defineConfig(({mode}) => {

    //Base vite config
    let config: UserConfig = {
        plugins: [webfontDownload(["https://fonts.googleapis.com/css2?family=Roboto:wght@400;900&display=swap"])],
        build: {
            //No minify in dev builds, speeds shit up
            minify: false,
            emptyOutDir: true,
            rollupOptions: {
                input: {
                    main: resolve(__dirname, 'src/main/main.ts'),
                    hero: resolve(__dirname, 'src/hero/hero.ts')
                },
                output: {
                    dir: resolve(__dirname, '..', 'VoltProjects.Server', 'wwwroot'),
                    entryFileNames: () => 'js/[name].js',
                    chunkFileNames: () => 'js/[name].[hash].js',
                    assetFileNames: () => 'assets/[name][extname]',
                    sourcemap: false,
                }
            }
        }
    }
    
    //In non-dev builds, we will use terser to minify everything
    if (mode != 'development') {
        console.log('Non-dev build, using terser...')
        config.build!.minify = 'terser';
        config.build!.terserOptions = {
            format: {
                comments: false
            },
            compress: {
                defaults: true,
                drop_console: true,
                drop_debugger: true
            },
            mangle: {
                eval: true,
                keep_classnames: false,
                keep_fnames: false,
                module: false,
                properties: false,
                safari10: false,
                toplevel: false,
            },
            ecma: 2020
        };
    }

    return config;
})
