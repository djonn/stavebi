import typescript from '@rollup/plugin-typescript';

export default {
    input: 'src/script.ts',
    output: {
        format: 'cjs'
    },
    plugins: [typescript()]
};