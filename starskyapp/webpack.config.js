const path = require('path');

module.exports = {
  mode: "development",
  devtool: "inline-source-map",
  entry: {
    'reload-redirect': "./src/client/script/reload-redirect.ts"
  },
  output: {
    path: path.resolve("[name].js".replace("src","build")),
    filename: "[name].js",
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