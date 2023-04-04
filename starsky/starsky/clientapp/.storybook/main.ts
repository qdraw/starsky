import type { StorybookConfig } from "@storybook/react-webpack5";
const config: StorybookConfig = {
  stories: ["../src/**/*.stories.@(js|jsx|ts|tsx)"],
  addons: [
    "@storybook/addon-links",
    "@storybook/addon-essentials",
    "@storybook/preset-create-react-app",
    "@storybook/addon-interactions"
  ],
  framework: {
    name: "@storybook/react-webpack5",
    options: {}
  },
  core: {
    disableTelemetry: true,
    enableCrashReports: false
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
