import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import * as path from "path";
import { UpdatePolicyLastCheckedDateSettings } from "../config/update-policy-settings.const";
import logger from "../logger/logger";
import { isPackaged } from "../os-info/is-packaged";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import {
  isPolicyDisabled,
  shouldItUpdate,
  SkipDisplayOfUpdate,
} from "./should-it-update";
import { updatesWarningWindows } from "./updates-warning-windows.const";

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
      partition: "persist:main",
      contextIsolation: true,
    },
  });

  // hides the menu for windows
  newWindow.setMenu(null);

  mainWindowStateKeeper.track(newWindow);

  const location = path.join(
    __dirname,
    "client",
    "pages",
    "updates-warning",
    "updates-warning.html"
  );

  await newWindow.loadFile(location);

  newWindow.once("ready-to-show", () => {
    newWindow.show();
  });

  newWindow.on("closed", () => {
    updatesWarningWindows.delete(newWindow);
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    appConfig.set(UpdatePolicyLastCheckedDateSettings, Date.now().toString());
    newWindow = null;
  });

  updatesWarningWindows.add(newWindow);
}

async function createCheckForUpdatesContainerWindow(
  intervalSpeed = 1000
): Promise<boolean> {
  // eslint-disable-next-line @typescript-eslint/no-misused-promises, no-async-promise-executor
  return new Promise(async (resolve, reject) => {
    const policy = (await isPolicyDisabled()) || (await SkipDisplayOfUpdate());
    if (policy) {
      reject("disabled");
      return;
    }

    setTimeout(
      // eslint-disable-next-line @typescript-eslint/no-misused-promises
      () => shouldItUpdate()
        .then(async (shouldItUpdate1) => {
          if (shouldItUpdate1) {
            await checkForUpdatesWindow();
          }
          resolve(shouldItUpdate1);
        })
        .catch((error) => {
          // fails for some random reason
          try {
            logger.warn(error);
          } catch (error1: unknown) {
            // nothing
          }
          reject(error);
        }),
      intervalSpeed
    );
  });
}

export default createCheckForUpdatesContainerWindow;
