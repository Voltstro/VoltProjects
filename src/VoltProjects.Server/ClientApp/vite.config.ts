import {defineConfig, UserConfig} from 'vite'
import {resolve} from 'path'

export default defineConfig(({mode}) => {

    //Base vite config
    let config: UserConfig = {
        build: {
            //No minify in dev builds, speeds shit up
            minify: false,
            rollupOptions: {
                input: {
                    main: resolve(__dirname, 'src/main/main.ts'),
                    hero: resolve(__dirname, 'src/hero/hero.ts')
                },
                output: {
                    dir: resolve(__dirname, '..', 'wwwroot', 'dist'),
                    entryFileNames: () => '[name].js',
                    assetFileNames: () => '[name][extname]',
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
            ecma: 2020
        };
    }

    return config;
})
