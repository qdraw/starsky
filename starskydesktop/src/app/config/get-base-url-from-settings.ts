import * as appConfig from "electron-settings";
import { IlocationUrlSettings } from "./IlocationUrlSettings";
import {
  LocationIsRemoteSettingsKey,
  LocationLocalHostSettingsKey,
  LocationUrlSettingsKey
} from "./location-settings.const";

export async function GetBaseUrlFromSettings(): Promise<IlocationUrlSettings> {
  const isRemoteString = (await appConfig.get(
    LocationIsRemoteSettingsKey
  )) as string;
  const isRemote = isRemoteString !== "false";

  const currentSettings = {
    location: await appConfig.get(LocationUrlSettingsKey),
    isValid: null,
    isLocal: false
  } as IlocationUrlSettings;

  const defaultLocalLocation = {
    isValid: null,
    isLocal: true,
    location: await appConfig.get(LocationLocalHostSettingsKey) // `http://localhost:${appPort}`
  } as IlocationUrlSettings;

  if (isRemote === false) {
    return defaultLocalLocation;
  }
  if (!currentSettings.location) {
    return defaultLocalLocation;
  }

  return currentSettings;
}
