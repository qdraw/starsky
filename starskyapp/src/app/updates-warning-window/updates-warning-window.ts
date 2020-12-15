import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import { UpdatePolicyLastCheckedDateSettings } from "../config/update-policy-settings.const";
import { isPackaged } from "../os-info/is-packaged";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import {
  isPolicyDisabled,
  shouldItUpdate,
  SkipDisplayOfUpdate
} from "./should-it-update";
import { updatesWarningWindows } from "./updates-warning-windows.const";

import path = require("path");

async function createCheckForUpdatesContainerWindow(
  intervalSpeed = 1000
): Promise<boolean> {
  return new Promise(async function (resolve, reject) {
    const policy = (await isPolicyDisabled()) || (await SkipDisplayOfUpdate());
    console.log(policy);

    if (policy) {
      reject("disabled");
      return;
    }

    setTimeout(
      () =>
        shouldItUpdate()
          .then(async (shouldItUpdate) => {
            if (shouldItUpdate) {
              await checkForUpdatesWindow();
            }
            resolve(shouldItUpdate);
          })
          .catch((error) => {
            console.log(error);
            reject(error);
          }),
      intervalSpeed
    );
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
