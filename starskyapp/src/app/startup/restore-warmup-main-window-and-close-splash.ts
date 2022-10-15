import { BrowserWindow } from "electron";
import { appPort } from "../child-process/setup-child-process";
import { restoreMainWindow } from "../main-window/restore-main-window";
import createCheckForUpdatesContainerWindow from "../updates-warning-window/updates-warning-window";
import { CloseSplash } from "../warmup/splash";
import { WarmupServer } from "../warmup/warmup-server";

export function RestoreMainWindowAndCloseSplash(splashWindow: BrowserWindow) {
  // eslint-disable-next-line @typescript-eslint/no-floating-promises
  restoreMainWindow().then(() => {
    createCheckForUpdatesContainerWindow().catch(() => {});
  });
  CloseSplash(splashWindow);
}

export default function RestoreWarmupMainWindowAndCloseSplash(splashWindow: BrowserWindow, isRemote : boolean) {
  if (!isRemote) {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    WarmupServer(appPort).then(() => {
      RestoreMainWindowAndCloseSplash(splashWindow);
    });
    return;
  }
  RestoreMainWindowAndCloseSplash(splashWindow);
}
