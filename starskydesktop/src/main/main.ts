/* eslint-disable @typescript-eslint/no-floating-promises */
import { app, BrowserWindow } from "electron";
import * as os from "os";
import { setupChildProcess } from "../app/child-process/setup-child-process";
import { MakeLogsPath } from "../app/config/logs-path";
import { MakeTempPath } from "../app/config/temp-path";
import { SetupFileWatcher } from "../app/file-watcher/setup-file-watcher";
import ipcBridge from "../app/ipc-bridge/ipc-bridge";
import createMainWindow from "../app/main-window/create-main-window";
import AppMenu from "../app/menu/app-menu";
import DockMenu from "../app/menu/dock-menu";
import defaultAppSettings from "../app/startup/app-settings";
import RestoreWarmupMainWindowAndCloseSplash from "../app/startup/restore-warmup-main-window-and-close-splash";
import { willNavigateSecurity } from "../app/startup/will-navigate-security";
import { IsRemote } from "../app/warmup/is-remote";
import { SetupSplash } from "../app/warmup/splash";

MakeLogsPath();
ipcBridge();
defaultAppSettings();
setupChildProcess();
MakeTempPath();
SetupFileWatcher();

console.log(`running in: :${os.arch()}`);

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on("ready", () => {
  AppMenu();
  DockMenu();

  IsRemote().then(async (isRemote) => {
    const splashWindow = await SetupSplash();
    RestoreWarmupMainWindowAndCloseSplash(splashWindow, isRemote);
  });

  app.on("activate", () => {
    // On macOS it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (BrowserWindow.getAllWindows().length === 0) {
      createMainWindow("?f=/");
    }
  });
});

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});

// https://github.com/electron/electron/blob/master/docs/tutorial/security.md
app.on("web-contents-created", (_, contents) => {
  contents.on("will-navigate", (event, navigationUrl) => {
    willNavigateSecurity(event, navigationUrl);
  });
});
