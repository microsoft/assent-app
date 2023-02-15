const path = require('path');
const webpack = require('webpack');
//const { globals, staticComponents, dynamicComponents } = require('./config');

function stringifyConfigValues(config) {
    const result = {};
    Object.keys(config).forEach((key) => {
        result[key] = JSON.stringify(config[key]);
    });

    return result;
}

const NodePolyfillPlugin = require('node-polyfill-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = (env) => {
    const NODE_ENV = env && env.NODE_ENV ? env.NODE_ENV : 'local';
    console.log('Environment', NODE_ENV);
    const config = require(path.join(__dirname, 'config', NODE_ENV));
    return [
        {
            name: 'static',
            target: 'web',
            mode: 'local',
            devtool: 'source-map',
            entry: config.staticComponents,
            output: {
                path: path.join(__dirname, 'public', 'bundles'),
                filename: '[name].[contenthash].js',
                clean: true,
            },
            externals: {
                react: 'React',
                'react-dom': 'ReactDOM',
                'react-router-dom': 'ReactRouterDOM',
                'styled-components': 'styled',
            },
            module: {
                rules: [
                    {
                        test: /\.tsx?$/,
                        loader: 'ts-loader',
                        exclude: /node_modules/,
                        options: {
                            transpileOnly: true,
                        },
                    },
                    {
                        test: /\.(png|jpg|gif)$/,
                        // eslint-disable-next-line prettier/prettier
                        use: [
                            'file-loader',
                        ],
                    },
                    {
                        test: /\.css$/i,
                        use: ['style-loader', 'css-loader'],
                    },
                    {
                        test: /\.svg$/,
                        use: [
                            {
                                loader: 'svg-url-loader',
                                options: {
                                    limit: 10000,
                                },
                            },
                        ],
                    },
                ],
            },
            resolve: {
                extensions: ['.ts', '.tsx', '.js'],
                modules: [path.resolve(__dirname, 'node_modules'), 'node_modules'],
                fallback: { buffer: require.resolve('buffer/'), fs: false },
            },
            plugins: [
                new webpack.DefinePlugin(stringifyConfigValues(config.globals)),
                new NodePolyfillPlugin(),
                new HtmlWebpackPlugin({
                    template: 'public/index_template.html',
                    filename: '../index.html',
                }),
            ],
            devServer: {
                //contentBase: path.join(__dirname, 'public'),
                compress: true,
                port: 9000,
                historyApiFallback: true,
                //writeToDisk: true,
                https: true,
                devMiddleware: {
                    writeToDisk: true,
                },
                open: true,
            },
        },
        //commenting out dynamic component webpack config until we integrate microfrontends through cdn
        /* {
            name: 'dynamic',
            target: 'web',
            mode: 'local',
            devtool: 'source-map',
            entry: config.dynamicComponents,
            externals: {
                react: 'React',
                'react-dom': 'ReactDOM',
                'react-router-dom': 'ReactRouterDOM',
                'styled-components': 'styled'
            },
            module: {
                rules: [
                    {
                        test: /\.tsx?$/,
                        loader: 'ts-loader',
                        exclude: /node_modules/,
                        options: {
                            transpileOnly: true
                        }
                    },
                    {
                    test: /\.(png|jpg|gif)$/,
                        // eslint-disable-next-line prettier/prettier
                        use: [
                        'file-loader'
                        ],
                    },
                    {
                        test: /\.css$/i,
                        use: ['style-loader', 'css-loader']
                },
                {
                    test: /\.svg$/,
                    use: [
                        {
                        loader: 'svg-url-loader',
                        options: {
                            limit: 10000,
                        },
                        },
                    ],
                }
                ]
            },
            output: {
                path: path.join(__dirname, 'public', 'bundles'),
                library: ['__WIDGETS__', '[name]'],
                libraryTarget: 'umd'
            },
            plugins: [new webpack.DefinePlugin(stringifyConfigValues(config.globals))],
            resolve: {
                extensions: ['.ts', '.tsx', '.js'],
                modules: [path.resolve(__dirname, 'node_modules'), 'node_modules']
            }
        } */
    ];
};
