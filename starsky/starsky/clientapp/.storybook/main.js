module.exports = {
  stories: ["../src/**/*.stories.tsx"],
  addons: [
    "@storybook/preset-create-react-app"
    // "@storybook/addon-actions",
    // "@storybook/addon-links"
  ],
  core: {
    builder: "webpack5",
    disableTelemetry: true,
    enableCrashReports: false
  },
  webpackFinal: async (config) => {
    // build-storybook url
    config.output.publicPath = "/";
    return config;
  }
};
