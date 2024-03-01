import typescript from '@rollup/plugin-typescript';
import copy from '@rollup-extras/plugin-copy';

export default {
    input: 'src/script.ts',
    output: {
        format: 'cjs'
    },
    plugins: [
        typescript(),
        copy('assets/*')
    ],
};