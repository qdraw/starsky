const path = require('path');

module.exports = {
  mode: "development",
  devtool: "inline-source-map",
  entry: {
    'reload-redirect': "./src/client/script/reload-redirect.ts",
    'settings': "./src/client/script/settings.ts",
  },
  output: {
    filename: (pathData) => {
      switch (pathData.runtime) {
        case 'settings':
        case 'reload-redirect':
          return path.join('build', 'client', 'script', '[name].js');
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