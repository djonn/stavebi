import typescript from '@rollup/plugin-typescript';

export default {
    input: 'src/script.ts',
    output: {
        dir: '../src/wwwroot',
        format: 'cjs'
    },
    plugins: [typescript()]
};