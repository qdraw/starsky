import * as appConfig from "electron-settings";
import { LocationUrlSettingsKey } from "../config/location-settings.const";
/* eslint-disable @typescript-eslint/no-base-to-string */

export async function willNavigateSecurity(
  event: Electron.Event,
  navigationUrl: string,
): Promise<boolean> {
  const parsedUrl = new URL(navigationUrl);

  // for example the upgrade page
  if (navigationUrl.startsWith("file://")) {
    return true;
  }

  // // to allow remote connections
  const currentSettings = await appConfig.get(LocationUrlSettingsKey);
  if (
    currentSettings
    && currentSettings
    && currentSettings
    && parsedUrl.origin.startsWith(new URL(currentSettings.toString()).origin)
  ) {
    return true;
  }

  if (parsedUrl.origin.startsWith("http://localhost:")) {
    return true;
  }

  event.preventDefault();
  return false;
}

export default willNavigateSecurity;
