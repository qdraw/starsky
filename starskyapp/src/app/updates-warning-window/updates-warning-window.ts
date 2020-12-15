import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import { DifferenceInDate } from "../../shared/date";
import { GetAppVersion } from "../config/get-app-version";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import {
  UpdatePolicyLastCheckedDateSettings,
  UpdatePolicySettings
} from "../config/update-policy-settings.const";
import UrlQuery from "../config/url-query";
import { GetNetRequest } from "../net-request/get-net-request";
import { isPackaged } from "../os-info/is-packaged";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import { updatesWarningWindows } from "./updates-warning-windows.const";
import path = require("path");

/**
 * Skip display of message
 */
export async function SkipDisplayOfUpdate(): Promise<boolean> {
  const localStorageItem = (await appConfig.get(
    UpdatePolicyLastCheckedDateSettings
  )) as string;
  if (!localStorageItem) return false;

  var getItem = parseInt(localStorageItem);
  if (isNaN(getItem)) return false;
  return DifferenceInDate(getItem) < 5760; // 4 days
}

/**
 * true is disabled
 */
export async function isPolicyDisabled(): Promise<boolean> {
  const item = (await appConfig.get(UpdatePolicySettings)) as boolean;
  return item !== true;
}

async function createCheckForUpdatesContainerWindow() {
  const policy = (await isPolicyDisabled()) || (await SkipDisplayOfUpdate());
  if (policy) {
    return;
  }

  setTimeout(
    () =>
      shouldItUpdate()
        .then((shouldItUpdate) => {
          if (shouldItUpdate) {
            checkForUpdatesWindow();
          }
        })
        .catch(() => {}),
    1000
  );
}

/**
 * true when need to update
 */
export async function shouldItUpdate(): Promise<boolean> {
  return new Promise(async function (resolve, reject) {
    let url = (await GetBaseUrlFromSettings()).location;
    url += new UrlQuery().HealthCheckForUpdates(GetAppVersion());
    console.log(url);

    try {
      const result = await GetNetRequest(url);
      console.log(result);

      if (result.statusCode !== 202) {
        resolve(false);
        return;
      }
      resolve(true);
    } catch (error) {
      reject();
    }
  });
}

export async function checkForUpdatesWindow() {
  const mainWindowStateKeeper = await windowStateKeeper("updates-warning");

  let newWindow = new BrowserWindow({
    x: mainWindowStateKeeper.x,
    y: mainWindowStateKeeper.y,
    width: 350,
    height: 300,
    show: true,
    resizable: !isPackaged(),
    webPreferences: {
      enableRemoteModule: false,
      partition: "persist:main",
      contextIsolation: true
    }
  });

  // hides the menu for windows
  newWindow.setMenu(null);

  mainWindowStateKeeper.track(newWindow);

  const location = path.join(
    __dirname,
    "..",
    "..",
    "client/pages/updates-warning/updates-warning.html"
  );

  newWindow.loadFile(location);

  newWindow.once("ready-to-show", () => {
    newWindow.show();
  });

  newWindow.on("closed", () => {
    updatesWarningWindows.delete(newWindow);
    appConfig.set(UpdatePolicyLastCheckedDateSettings, Date.now().toString());
    newWindow = null;
  });

  updatesWarningWindows.add(newWindow);
}

export default createCheckForUpdatesContainerWindow;
