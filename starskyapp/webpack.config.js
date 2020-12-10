const path = require('path');

module.exports = {
  mode: "development",
  devtool: "inline-source-map",
  entry: {
    'reload-redirect': "./src/client/script/reload-redirect.ts",
  },
  output: {
    filename: (pathData) => {
      switch (pathData.runtime) {
        case 'reload-redirect':
          return path.join('build', 'client', 'script', '[name].js');
        case 'preload-main':
          return path.join('build', 'preload', '[name].js');
        default:
          return '[name].js';
        }
    },
    path: __dirname,
  },
  resolve: {
    // Add ".ts" and ".tsx" as resolvable extensions.
    extensions: [".ts", ".tsx", ".js"],
  },
  module: {
    rules: [
      // all files with a `.ts` or `.tsx` extension will be handled by `ts-loader`
      { 
          test: /\.tsx?$/, 
          loader: "ts-loader" ,
          options: {
            configFile: "tsconfig.client.json"
          }
        },
    ],
  },
  plugins: [
  ],

};