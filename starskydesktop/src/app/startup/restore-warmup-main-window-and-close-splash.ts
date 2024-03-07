import { BrowserWindow } from "electron";
import global, { SharedSettings } from "../global/global";
import logger from "../logger/logger";
import { RestoreMainWindow } from "../main-window/restore-main-window";
import createCheckForUpdatesContainerWindow from "../updates-warning-window/updates-warning-window";
import { CloseSplash } from "../warmup/splash";
import { WarmupServer } from "../warmup/warmup-server";

export function RestoreMainWindowAndCloseSplash(baseUrl: string, splashWindows: BrowserWindow[]) {
  return new Promise((resolve, reject) => {
    RestoreMainWindow(baseUrl)
      .then(() => {
        createCheckForUpdatesContainerWindow()
          .catch(() => {})
          .then(() => resolve)
          .catch(() => reject);
      })
      .then(() => {
        CloseSplash(splashWindows[0]);
      })
      .catch(() => reject);
  });
}

export default async function RestoreWarmupMainWindowAndCloseSplash(
  appPort: number,
  splashWindows: BrowserWindow[]
): Promise<number> {
  return new Promise((resolve, reject) => {
    logger.info(`[RestoreWarmupMainWindowAndCloseSplash] appPort:: ${appPort}`);
    const { baseUrl } = global.shared as SharedSettings;

    WarmupServer(appPort)
      .then(async (result) => {
        logger.info(`[RestoreWarmupMainWindowAndCloseSplash] result:: ${result}`);

        if (result) {
          await RestoreMainWindowAndCloseSplash(baseUrl, splashWindows);
          resolve(appPort);
          return;
        }
        reject(new Error("WarmupServer failed"));
      })
      .catch(() => {});
    // resolve(true)
    // TODO TEST
    // RestoreMainWindowAndCloseSplash(splashWindows);
  });
}
