// (randomly disabled sometimes) eslint-disable-next-line import/no-unresolved
import CopyPlugin from 'copy-webpack-plugin';
import path from 'path';
import TsconfigPathsPlugin from 'tsconfig-paths-webpack-plugin';
import { fileURLToPath } from 'url';
import webpack from 'webpack';
import { merge } from 'webpack-merge';

/* eslint-disable no-underscore-dangle */
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
/* eslint-enable */

const isEnvProduction = process.env.NODE_ENV === 'production';
const isEnvDevelopment = process.env.NODE_ENV === 'development';

const commonConfig = {
  devtool: isEnvDevelopment ? 'source-map' : false,
  mode: isEnvProduction ? 'production' : 'development',
  output: { path: path.join(__dirname, 'dist') },
  node: { __dirname: false, __filename: false },
  plugins: [
    new webpack.NormalModuleReplacementPlugin(/^\S+\/\S+\.js$/, (resource) => {
      // eslint-disable-next-line no-param-reassign
      resource.request = resource.request.replace(/\.js$/, '');
    }),
  ],
  resolve: {
    extensions: ['.js', '.json', '.ts', '.tsx'],
    plugins: [new TsconfigPathsPlugin({
      configFile: './tsconfig.json',
      extensions: ['.js', '.json', '.ts', '.tsx'],
    })],
  },
  module: {
    rules: [
      {
        test: /\.(ts|tsx)$/,
        exclude: /node_modules/,
        loader: 'ts-loader',
      },
      {
        test: /\.(scss|css)$/,
        use: ['style-loader', 'css-loader'],
      },
      {
        test: /\.(jpg|png|svg|ico|icns)$/,
        loader: 'file-loader',
        options: {
          name: '[path][name].[ext]',
        },
      },
    ],
  },
  externals: {
    fsevents: "require('fsevents')",
    fs: "require('fs')",
    path: "require('path')",
    child_process: "require('child_process')",
  },
};

const mainConfig = merge(commonConfig, {
  entry: './src/main/main.ts',
  target: 'electron-main',
  output: { filename: 'main.bundle.js' },
  plugins: [
    new CopyPlugin({
      patterns: [
        {
          from: 'package.json',
          to: 'package.json',
          transform: (content, _path) => {
            const jsonContent = JSON.parse(content);
            const electronVersion = jsonContent.devDependencies.electron;

            delete jsonContent.devDependencies;
            delete jsonContent.optionalDependencies;
            delete jsonContent.scripts;
            delete jsonContent.build;

            jsonContent.main = './main.bundle.js';
            jsonContent.scripts = { start: 'electron ./main.bundle.js' };
            jsonContent.devDependencies = { electron: electronVersion };

            return JSON.stringify(jsonContent, undefined, 2);
          },
        },
      ],
    }),
  ],
});

const preloadConfig = merge(commonConfig, {
  entry: './src/preload/preload-main.ts',
  target: 'electron-preload',
  output: { filename: 'preload-main.bundle.js' },
});

const clientConfig = merge(commonConfig, {
  entry: {
    'reload-redirect': "./src/client/script/reload-redirect.ts",
    settings: "./src/client/script/settings.ts",
    'focus-button-autoclose': "./src/client/script/focus-button-autoclose.ts",
    'before-build': "./src/setup/before-build.ts",

  },
  output: {
    filename: (pathData) => {
      switch (pathData.runtime) {
        case 'focus-button-autoclose':
        case 'settings':
        case 'reload-redirect':
          return path.join('dist', 'client', 'script', '[name].js');
        case 'before-build':
          return path.join('dist', 'setup', '[name].js');
        default:
          return '[name].js';
      }
    },
    path: __dirname,
  },
  plugins: [
    new CopyPlugin({
      patterns: [
        {
          from: "src/client/css",
          to: "dist/client/css",
        },
        {
          from: "src/client/fonts",
          to: "dist/client/fonts",
        },
        {
          from: "src/client/images",
          to: "dist/client/images",
        },
        {
          from: "src/client/pages",
          to: "dist/client/pages",
        },
      ],
    }),
  ],
});

export default [mainConfig, preloadConfig, clientConfig];
