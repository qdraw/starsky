const config = {
  stories: ["../src/**/*.stories.@(js|jsx|ts|tsx)"],
  addons: [
    "@storybook/addon-links",
    "@storybook/addon-essentials",
    "@storybook/preset-create-react-app",
    "@storybook/addon-interactions"
  ],
  core: {
    disableTelemetry: true,
    enableCrashReports: false,
    builder: '@storybook/builder-vite', // 👈 The builder enabled here.
  },
  docs: {
    autodocs: "tag"
  },
  staticDirs: ["../public"],
  webpackFinal: async (config) => {
    // build-storybook url
    //config.output!.publicPath = "/";
    return config;
  },
  features: {
    storyStoreV7: true
  }
};
export default config;
