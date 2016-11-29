module.exports = {
  entry: {
    main: "./temp/src/main",
    renderer: "./temp/src/renderer"
  },
  output: {
    filename: "[name].js",
    path: "./app/js",
    libraryTarget: "commonjs2"
  },
  target: "node",
  node: {
    __dirname: false,
    __filename: false
  },
  externals: {
    electron: true
  },
  devtool: "source-map",
  module: {
    preLoaders: [{
      loader: "source-map-loader",
      exclude: /node_modules/,
      test: /\.js$/
    }]
  }
};
