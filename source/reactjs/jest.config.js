const { jsWithBabel: tsjPreset } = require('ts-jest/presets');

module.exports = {
    transform: {
        ...tsjPreset.transform
    },
    globals: 
    {
        __APP_NAME__: "__APP_NAME__",
        __CLIENT_ID__: "__CLIENT_ID__",
        __AUTHORITY__: "__AUTHORITY__",
        __INSTRUMENTATION_KEY__: "__INSTRUMENTATION_KEY__",
        __ENV_NAME__: "__ENV_NAME__",
        crypto: require('crypto')
    },
    collectCoverageFrom: [
        "**/src/**/*.ts?(x)",
        "!**/src/**/*.{test}.ts?(x)",
        "!**/src/**/*.d.ts",
        "!**/node_modules/**",
        "!**/coverage/**",
        "!**/public/**",
        "!**/index.js"
    ],
    collectCoverage: false,
    testRegex: '(/tests/.*\\.(test)\\.(jsx?|tsx?|js?|ts?))$',
    moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx'],
    testPathIgnorePatterns: ['node_modules'],
    transformIgnorePatterns: ['node_modules/?!(office-ui-fabric-react)'],
    moduleNameMapper: {},
    moduleDirectories: ['node_modules', 'src'],
    modulePathIgnorePatterns: [],
    setupFiles: ['<rootDir>/src/tests/test.config.ts'],
    reporters: [ "default", "jest-junit" ]
};
