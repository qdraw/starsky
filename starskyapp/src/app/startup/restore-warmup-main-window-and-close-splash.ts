import { BrowserWindow } from "electron";
import { appPort } from "../child-process/setup-child-process";
import { restoreMainWindow } from "../main-window/restore-main-window";
import createCheckForUpdatesContainerWindow from "../updates-warning-window/updates-warning-window";
import { CloseSplash } from "../warmup/splash";
import { WarmupServer } from "../warmup/warmup-server";

function RestoreMainWindowAndCloseSplash(splashWindow: BrowserWindow) {
    restoreMainWindow().then(() => {
      createCheckForUpdatesContainerWindow().catch(() => {});
    });
    CloseSplash(splashWindow);
}

export function RestoreWarmupMainWindowAndCloseSplash(splashWindow: BrowserWindow, isRemote : Boolean) {
    if (!isRemote) {    
        WarmupServer(appPort).then(()=>{
            RestoreMainWindowAndCloseSplash(splashWindow);
        });
        return;
    }
    RestoreMainWindowAndCloseSplash(splashWindow);
}