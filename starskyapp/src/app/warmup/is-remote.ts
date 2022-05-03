import * as appConfig from "electron-settings";
import { LocationIsRemoteSettingsKey } from "../config/location-settings.const";

export async function IsRemote(): Promise<Boolean> {
  const currentSettings = await appConfig.get(LocationIsRemoteSettingsKey);

  let isLocationRemote = false;
  if (currentSettings !== undefined && currentSettings !== null) {
    isLocationRemote = currentSettings.toString() === "true";
  }
  return isLocationRemote;
}