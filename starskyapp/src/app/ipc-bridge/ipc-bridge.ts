import { app, ipcMain } from "electron";
import * as appConfig from "electron-settings";
import { AppVersionIpcKey } from "../config/app-version-ipc-key.const";
import DefaultImageApplicationSetting from "../config/default-image-application-settings";
import {
  DefaultImageApplicationIpcKey,
  IDefaultImageApplicationProps
} from "../config/default-image-application-settings-ipc-key.const";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import { IlocationUrlSettings } from "../config/IlocationUrlSettings";
import {
  LocationIsRemoteIpcKey,
  LocationUrlIpcKey
} from "../config/location-ipc-keys.const";
import {
  LocationIsRemoteSettingsKey,
  LocationUrlSettingsKey
} from "../config/location-settings.const";
import RememberUrl from "../config/remember-url-settings.const";
import { UpdatePolicyIpcKey } from "../config/update-policy-ipc-key.const";
import { UpdatePolicySettings } from "../config/update-policy-settings.const";
import UrlQuery from "../config/url-query";
import { ipRegex, urlRegex } from "../config/url-regex";
import { fileSelectorWindow } from "../file-selector-window/file-selector-window";
import { SetupFileWatcher } from "../file-watcher/setup-file-watcher";
import logger from "../logger/logger";
import createMainWindow from "../main-window/create-main-window";
import { mainWindows } from "../main-window/main-windows.const";
import { GetNetRequest } from "../net-request/get-net-request";
import { settingsWindows } from "../settings-window/settings-windows.const";

function ipcBridge() {
  // When adding a new key also update preload-main.ts

  ipcMain.on(LocationIsRemoteIpcKey, async (event, args) =>
    LocationIsRemoteCallback(event, args)
  );

  ipcMain.on(AppVersionIpcKey, async (event) => AppVersionCallback(event));

  ipcMain.on(LocationUrlIpcKey, async (event, args: string) =>
    LocationUrlCallback(event, args)
  );

  ipcMain.on(UpdatePolicyIpcKey, async (event, args) =>
    UpdatePolicyCallback(event, args)
  );

  ipcMain.on(DefaultImageApplicationIpcKey, async (event, args) =>
    DefaultImageApplicationCallback(event, args)
  );
}

export async function DefaultImageApplicationCallback(
  event: Electron.IpcMainEvent,
  args: IDefaultImageApplicationProps
) {
  if (!args) {
    const currentSettings = await appConfig.get(DefaultImageApplicationSetting);
    event.reply(DefaultImageApplicationIpcKey, currentSettings);
    return;
  }
  if (args.reset) {
    await appConfig.unset(DefaultImageApplicationSetting);
    event.reply(DefaultImageApplicationIpcKey, false);
    return;
  }

  if (args.showOpenDialog) {
    try {
      const result = await fileSelectorWindow();
      await appConfig.set(DefaultImageApplicationSetting, result[0]);
      event.reply(DefaultImageApplicationIpcKey, result[0]);
    } catch (error) {}
  }
}

export async function LocationIsRemoteCallback(
  event: Electron.IpcMainEvent,
  args: boolean
) {
  if (args !== undefined && args !== null) {
    await closeAndCreateNewWindow();
    await appConfig.set(LocationIsRemoteSettingsKey, args.toString());
    // filewatcher need to be after update/set
    await SetupFileWatcher();
  }

  const currentSettings = await appConfig.get(LocationIsRemoteSettingsKey);

  let isLocationRemote = false;
  if (currentSettings !== undefined && currentSettings !== null) {
    isLocationRemote = currentSettings.toString() === "true";
  }

  event.reply(LocationIsRemoteIpcKey, isLocationRemote);
}

export async function AppVersionCallback(event: Electron.IpcMainEvent) {
  const appVersion = app
    .getVersion()
    .match(new RegExp("^[0-9]+\\.[0-9]+", "ig"));

  event.reply(AppVersionIpcKey, appVersion);
}

export async function LocationUrlCallback(
  event: Electron.IpcMainEvent,
  args: string
) {
  // getting
  if (!args) {
    event.reply(LocationUrlIpcKey, await GetBaseUrlFromSettings());
    return;
  }

  if (
    args.match(urlRegex) ||
    args.match(ipRegex) ||
    args.startsWith("http://localhost:")
  ) {
    // to avoid errors
    var locationUrl = args.replace(/\/$/, "");

    try {
      const response = await GetNetRequest(
        locationUrl + new UrlQuery().HealthApi()
      );
      const responseSettings = {
        location: locationUrl,
        isLocal: false
      } as IlocationUrlSettings;

      logger.info("ipc-bridge response >");
      logger.info(response);

      var locationOk = response.statusCode == 200 || response.statusCode == 503;
      if (locationOk) {
        await appConfig.set(LocationUrlSettingsKey, locationUrl);

        // so you can save change the location
        await SetupFileWatcher();
        setTimeout(async () => {
          await closeAndCreateNewWindow();
        }, 100);
      }

      logger.info("ipc-bridge locationOk >");
      logger.info(locationOk);

      responseSettings.isValid = locationOk;

      event.reply(LocationUrlIpcKey, responseSettings);
    } catch (error) {
      event.reply(LocationUrlIpcKey, {
        isValid: false,
        isLocal: false,
        location: args,
        reason: error
      } as IlocationUrlSettings);
    }
    return;
  }

  event.reply(LocationUrlIpcKey, {
    isValid: false,
    isLocal: false,
    location: args
  } as IlocationUrlSettings);
}

/**
 * to avoid that the session is opened
 */
async function closeAndCreateNewWindow() {
  await appConfig.set(RememberUrl, {});
  mainWindows.forEach((window) => {
    window.close();
  });
  const newWindow = await createMainWindow("");
  newWindow.once("ready-to-show", () => {
    settingsWindows.forEach((window) => {
      window.show();
    });
  });
}

export async function UpdatePolicyCallback(
  event: Electron.IpcMainEvent,
  args: boolean
) {
  if (args === null || args === undefined) {
    if (appConfig.has(UpdatePolicySettings)) {
      const updatePolicy = (await appConfig.get(
        UpdatePolicySettings
      )) as boolean;

      if (updatePolicy !== null && updatePolicy !== undefined) {
        event.reply(UpdatePolicyIpcKey, updatePolicy);
        return;
      }
    }
    event.reply(UpdatePolicyIpcKey, true);
    return;
  }

  appConfig.set(UpdatePolicySettings, args);
  // reset check date for latest version
  // appConfig.delete(CheckForUpdatesLocalStorageName);

  event.reply(UpdatePolicyIpcKey, args);
}

export default ipcBridge;
