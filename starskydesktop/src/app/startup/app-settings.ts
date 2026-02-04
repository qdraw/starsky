import * as appConfig from "electron-settings";
import logger from "../logger/logger";

function defaultAppSettings(): string {
  try {
    appConfig.configure({
      prettify: true,
      fileName: "starksy-app-settings.json",
    });
  } catch (error) {
    logger.warn(`defaultAppSettings: unable to set app settings ${error}`);
  }


  const appPath = appConfig.file();
  logger.info(`desktop app-settings path :> \n${appPath}`);
  return appPath;
}

export default defaultAppSettings;
