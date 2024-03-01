import { BrowserWindow } from "electron";
import { RestoreMainWindow } from "../main-window/restore-main-window";
import createCheckForUpdatesContainerWindow from "../updates-warning-window/updates-warning-window";
import { CloseSplash } from "../warmup/splash";
import { WarmupServer } from "../warmup/warmup-server";
import logger from "../logger/logger";

export function RestoreMainWindowAndCloseSplash(baseUrl: string,
  splashWindows: BrowserWindow[]
) {
  // eslint-disable-next-line @typescript-eslint/no-floating-promises
  RestoreMainWindow(baseUrl).then(() => {
    createCheckForUpdatesContainerWindow().catch(() => {});
  });
  CloseSplash(splashWindows[0]);
}

export default function RestoreWarmupMainWindowAndCloseSplash(
  appPort: number,
  splashWindows: BrowserWindow[],
  isRemote: boolean
) {
  if (!isRemote) {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    WarmupServer(appPort).then((result) => {
      if (result) {
        RestoreMainWindowAndCloseSplash(`http://localhost:${appPort}`,splashWindows);    
        return;
      }
    });
    return;
  }
  // TODO TEST
  // RestoreMainWindowAndCloseSplash(splashWindows);
}
