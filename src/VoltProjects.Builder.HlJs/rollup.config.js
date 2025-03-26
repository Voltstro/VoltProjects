import { nodeResolve } from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import esbuild from 'rollup-plugin-esbuild';

export default {
  input: 'index.js',
  output: {
    dir: 'dist',
    format: 'module',
    compact: true
  },
  plugins: [nodeResolve(), commonjs(), esbuild({minify: true, drop: ['console']})]
};