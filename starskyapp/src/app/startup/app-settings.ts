import * as appConfig from "electron-settings";

function defaultAppSettings(): string {
  appConfig.configure({
    prettify: true,
    fileName: "starksy-app-settings.json"
  });

  const appPath = appConfig.file();
  console.log("app-settings path :> \n" + appPath);
  return appPath;
}

export default defaultAppSettings;
