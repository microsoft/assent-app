module.exports = {
    env: {
        test: {
            plugins: ['@babel/plugin-transform-modules-commonjs','@babel/plugin-syntax-jsx','@babel/plugin-transform-runtime']
        }
    },
    presets: ['@babel/preset-env']
};
