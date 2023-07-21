import * as appConfig from "electron-settings";
import { LocationIsRemoteSettingsKey } from "../config/location-settings.const";
/* eslint-disable @typescript-eslint/no-base-to-string */

export async function IsRemote(): Promise<boolean> {
  const currentSettings = await appConfig.get(LocationIsRemoteSettingsKey);

  let isLocationRemote = false;
  if (currentSettings !== undefined && currentSettings !== null) {
    isLocationRemote = currentSettings.toString() === "true";
  }
  return isLocationRemote;
}
