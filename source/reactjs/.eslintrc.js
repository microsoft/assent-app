module.exports = {
    parser: '@typescript-eslint/parser',
    extends: ['plugin:@typescript-eslint/recommended', 'plugin:prettier/recommended'],
    parserOptions: {
        ecmaVersion: 2018,
        sourceType: 'module',
        ecmaFeatures: {
            jsx: true,
        },
    },
    settings: {
        react: {
            version: 'detect',
        },
    },
    plugins: ['react-hooks'],
    rules: {
        '@typescript-eslint/interface-name-prefix': 0,
        'react-hooks/rules-of-hooks': 'error',
        'react-hooks/exhaustive-deps': 'warn',
        //"@typescript-eslint/no-explicit-any": "off",
        '@typescript-eslint/no-use-before-define': 'off',
        'no-empty-function': 'off',
        'prettier/prettier': [
            'error',
            {
                endOfLine: 'auto',
            },
        ],
        '@typescript-eslint/no-empty-function': ['error', { allow: ['arrowFunctions'] }],
    },
};
