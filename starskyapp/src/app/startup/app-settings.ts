import * as appConfig from "electron-settings";
import logger from "../logger/logger";

function defaultAppSettings(): string {
  appConfig.configure({
    prettify: true,
    fileName: "starksy-app-settings.json"
  });

  const appPath = appConfig.file();
  logger.info("desktop app-settings path :> \n" + appPath);
  return appPath;
}

export default defaultAppSettings;
