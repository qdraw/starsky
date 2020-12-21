import * as appConfig from "electron-settings";
import { appPort } from "../child-process/setup-child-process";
import { IlocationUrlSettings } from "./IlocationUrlSettings";
import {
  LocationIsRemoteSettingsKey,
  LocationUrlSettingsKey
} from "./location-settings.const";

export async function GetBaseUrlFromSettings(): Promise<IlocationUrlSettings> {
  const isRemote = (await appConfig.get(
    LocationIsRemoteSettingsKey
  )) as boolean;

  const currentSettings = {
    location: await appConfig.get(LocationUrlSettingsKey),
    isValid: null,
    isLocal: false
  } as IlocationUrlSettings;

  if (!isRemote || !currentSettings.location) {
    return {
      isValid: null,
      isLocal: true,
      location: `http://localhost:${appPort}`
    } as IlocationUrlSettings;
  }
  return currentSettings;
}
