import * as appConfig from "electron-settings";

function defaultAppSettings() {
  appConfig.configure({
    prettify: true,
    fileName: "starksy-app-settings.json"
  });
  console.log(appConfig.file());
}

export default defaultAppSettings;
