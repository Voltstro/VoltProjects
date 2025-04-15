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
                    public: resolve(__dirname, 'src/public/index.ts'),
                    admin: resolve(__dirname, 'src/admin/index.ts'),
                    index: resolve(__dirname, 'src/scss/index.scss')
                },
                output: {
                    dir: outDir,
                    entryFileNames: () => 'js/[name].js',
                    chunkFileNames: () => 'js/[name].[hash].js',
                    assetFileNames: () => 'assets/[name][extname]'
                },
            }
        },
        css: {
            preprocessorOptions: {
                scss: {
                    quietDeps: true,
                    silenceDeprecations: [
                        'import',
                        'mixed-decls'
                    ]
                }
            }
        }
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
