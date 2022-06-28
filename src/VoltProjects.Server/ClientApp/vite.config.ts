import { defineConfig, UserConfig } from 'vite'
import { terser } from 'rollup-plugin-terser';
import { resolve } from 'path'

export default defineConfig(({command, mode}) => {
  let config: UserConfig = {
    build: {
      rollupOptions: {
        input: {
            main: resolve(__dirname, 'src/main.ts')
        },
        output: {
            dir: resolve(__dirname, '..', 'wwwroot', 'dist'),
            entryFileNames: () => 'site.js',
            assetFileNames: () => '[name][extname]',
            sourcemap: false
        },
          plugins: [terser({
              format: {
                  comments: false
              },
              compress: {
                  defaults: true,
                  drop_console: true,
                  drop_debugger: true
              },
              ecma: 2020
          })]
      },
    }
  }

  return config;
})
