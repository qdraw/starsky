import { BrowserWindow } from "electron";
import global, { SharedSettings } from "../global/global";
import { restoreMainWindow } from "../main-window/restore-main-window";
import createCheckForUpdatesContainerWindow from "../updates-warning-window/updates-warning-window";
import { CloseSplash } from "../warmup/splash";
import { WarmupServer } from "../warmup/warmup-server";

export function RestoreMainWindowAndCloseSplash(splashWindows: BrowserWindow[]) {
  // eslint-disable-next-line @typescript-eslint/no-floating-promises
  restoreMainWindow().then(() => {
    createCheckForUpdatesContainerWindow().catch(() => {});
  });
  CloseSplash(splashWindows[0]);
}

export default function RestoreWarmupMainWindowAndCloseSplash(
  splashWindows: BrowserWindow[],
  isRemote: boolean
) {
  if (!isRemote) {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    WarmupServer((global.shared as SharedSettings).port).then(() => {
      RestoreMainWindowAndCloseSplash(splashWindows);
    });
    return;
  }
  RestoreMainWindowAndCloseSplash(splashWindows);
}
