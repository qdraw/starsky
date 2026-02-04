/* eslint-disable @typescript-eslint/no-misused-promises, @typescript-eslint/comma-dangle */
import { app, ipcMain } from "electron";
import * as appConfig from "electron-settings";
import { IlocationUrlSettings } from "../config/IlocationUrlSettings";
import { AppVersionIpcKey } from "../config/app-version-ipc-key.const";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import { LocationIsRemoteIpcKey, LocationUrlIpcKey } from "../config/location-ipc-keys.const";
import {
  LocationIsRemoteSettingsKey,
  LocationUrlSettingsKey,
} from "../config/location-settings.const";
import RememberUrl from "../config/remember-url-settings.const";
import { UpdatePolicyIpcKey } from "../config/update-policy-ipc-key.const";
import { UpdatePolicySettings } from "../config/update-policy-settings.const";
import UrlQuery from "../config/url-query";
import { ipRegex, urlRegex } from "../config/url-regex";
import { SetupFileWatcher } from "../file-watcher/setup-file-watcher";
import logger from "../logger/logger";
import createMainWindow from "../main-window/create-main-window";
import { mainWindows } from "../main-window/main-windows.const";
import { GetNetRequest } from "../net-request/get-net-request";
import { settingsWindows } from "../settings-window/settings-windows.const";
import { IsRemote } from "../warmup/is-remote";

export async function UpdatePolicyCallback(event: Electron.IpcMainEvent, args: boolean) {
  if (args === null || args === undefined) {
    if (await appConfig.has(UpdatePolicySettings)) {
      const updatePolicy = (await appConfig.get(UpdatePolicySettings)) as boolean;

      if (updatePolicy !== null && updatePolicy !== undefined) {
        event.reply(UpdatePolicyIpcKey, updatePolicy);
        return;
      }
    }
    event.reply(UpdatePolicyIpcKey, true);
    return;
  }

  await appConfig.set(UpdatePolicySettings, args);
  // reset check date for latest version
  // appConfig.delete(CheckForUpdatesLocalStorageName);

  event.reply(UpdatePolicyIpcKey, args);
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

export async function LocationIsRemoteCallback(event: Electron.IpcMainEvent, args: boolean) {
  if (args !== undefined && args !== null) {
    await appConfig.set(LocationIsRemoteSettingsKey, args.toString());
    // filewatcher need to be after update/set
    await SetupFileWatcher();
    await closeAndCreateNewWindow();
  }

  event.reply(LocationIsRemoteIpcKey, await IsRemote());
}

export function AppVersionCallback(event: Electron.IpcMainEvent) {
  const appVersion = app.getVersion().match(/^\d+\.\d+/gi);

  event.reply(AppVersionIpcKey, appVersion);
}

export async function LocationUrlCallback(event: Electron.IpcMainEvent, args: string) {
  // getting
  if (!args) {
    event.reply(LocationUrlIpcKey, await GetBaseUrlFromSettings());
    return;
  }

  if (args.match(urlRegex) || args.match(ipRegex) || args.startsWith("http://localhost:")) {
    console.log("ipc-bridge start update");

    // to avoid errors
    const locationUrl = args.replace(/\/$/, "");

    try {
      const response = await GetNetRequest(locationUrl + new UrlQuery().HealthApi());
      const responseSettings = {
        location: locationUrl,
        isLocal: false,
      } as IlocationUrlSettings;

      logger.info("ipc-bridge response >");
      // eslint-disable-next-line @typescript-eslint/no-unsafe-argument
      logger.info(response.data);

      const locationOk = response.statusCode === 200 || response.statusCode === 503;
      if (locationOk) {
        await appConfig.set(LocationUrlSettingsKey, locationUrl);

        // so you can save change the location
        await SetupFileWatcher();
        // eslint-disable-next-line @typescript-eslint/no-misused-promises
        setTimeout(async () => {
          await closeAndCreateNewWindow();
        }, 100);
      }

      logger.info("ipc-bridge locationOk >");
      logger.info(locationOk.toString());

      responseSettings.isValid = locationOk;

      event.reply(LocationUrlIpcKey, responseSettings);
    } catch (error: unknown) {
      event.reply(LocationUrlIpcKey, {
        isValid: false,
        isLocal: false,
        location: args,
        reason: error,
      } as IlocationUrlSettings);
    }
    return;
  }

  console.log(
    // eslint-disable-next-line @typescript-eslint/restrict-template-expressions
    `ipc-bridge ${args.match(urlRegex)}  ${args.match(ipRegex)} ${args.startsWith(
      "http://localhost:"
    )}`
  );

  event.reply(LocationUrlIpcKey, {
    isValid: false,
    isLocal: false,
    location: args,
  } as IlocationUrlSettings);
}

function ipcBridge() {
  // When adding a new key also update preload-main.ts

  ipcMain.on(LocationIsRemoteIpcKey, async (event, args: boolean) => LocationIsRemoteCallback(event, args));

  ipcMain.on(AppVersionIpcKey, (event) => AppVersionCallback(event));

  ipcMain.on(LocationUrlIpcKey, async (event, args: string) => LocationUrlCallback(event, args));

  ipcMain.on(UpdatePolicyIpcKey, async (event, args: boolean) => UpdatePolicyCallback(event, args));
}

export default ipcBridge;
