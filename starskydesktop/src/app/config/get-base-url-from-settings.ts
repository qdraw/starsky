import * as appConfig from "electron-settings";
import global, { SharedSettings } from "../global/global";
import { IlocationUrlSettings } from "./IlocationUrlSettings";
import { LocationIsRemoteSettingsKey, LocationUrlSettingsKey } from "./location-settings.const";

export async function GetBaseUrlFromSettings(): Promise<IlocationUrlSettings> {
  const isRemoteString = (await appConfig.get(LocationIsRemoteSettingsKey)) as string;
  const isRemote = isRemoteString !== "false";

  const currentSettings = {
    location: await appConfig.get(LocationUrlSettingsKey),
    isValid: null,
    isLocal: false,
  } as IlocationUrlSettings;

  const defaultLocalLocation = {
    isValid: null,
    isLocal: true,
    location: `http://localhost:${(global.shared as SharedSettings).port}`,
  } as IlocationUrlSettings;

  if (isRemote === false) {
    return defaultLocalLocation;
  }
  if (!currentSettings.location) {
    return defaultLocalLocation;
  }

  return currentSettings;
}
