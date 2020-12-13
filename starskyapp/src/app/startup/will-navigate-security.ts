import * as appConfig from "electron-settings";
import { LocationUrlSettingsKey } from "../config/location-settings.const";

export async function willNavigateSecurity(
  event: Electron.Event,
  navigationUrl: string
) {
  const parsedUrl = new URL(navigationUrl);

  // for example the upgrade page
  if (navigationUrl.startsWith("file://")) {
    return;
  }

  // // to allow remote connections
  var currentSettings = await appConfig.get(LocationUrlSettingsKey);
  if (
    currentSettings &&
    currentSettings &&
    currentSettings &&
    parsedUrl.origin.startsWith(new URL(currentSettings.toString()).origin)
  ) {
    return;
  }

  if (!parsedUrl.origin.startsWith("http://localhost:")) {
    event.preventDefault();
  }
}
