var path = require("path");
var webpack = require("webpack");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
    entry: {
        app: "./app/index.js"
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: "babel-loader",
                    options: {
                        presets: ["@babel/preset-env"]
                        // plugins: [require('babel-plugin-transform-object-rest-spread')]
                    }
                }
            },
            {
                test: /\.css$/,
                use: [
                    //"style-loader",
                    {
                        //loader: "css-loader",
                        loader: MiniCssExtractPlugin.loader,
                        options: {
                            importLoaders: 1,
                            hmr: process.env.NODE_ENV === 'development'
                        }
                    },
                    "css-loader",
                    "postcss-loader"
                ]
            }
        ]
    },

    output: {
        path: path.join(__dirname, "./wwwroot"),
        filename: "[name].js"
    },

    resolve: {
        modules: [path.resolve(__dirname, "app"), "node_modules"]
    },

    plugins: [
        new webpack.ProvidePlugin({
            $: "jquery",
            jQuery: "jquery"
        }),
        new MiniCssExtractPlugin({
            filename: "[name].css",
            chunkFilename: "[id].css",
            ignoreOrder: false // Enable to remove warnings about conflicting order
        })
    ]
};