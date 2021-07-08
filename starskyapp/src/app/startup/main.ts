import { app, BrowserWindow } from "electron";
import { setupChildProcess } from "../child-process/setup-child-process";
import { MakeLogsPath } from "../config/logs-path";
import { MakeTempPath } from "../config/temp-path";
import { SetupFileWatcher } from "../file-watcher/setup-file-watcher";
import ipcBridge from "../ipc-bridge/ipc-bridge";
import createMainWindow from "../main-window/create-main-window";
import { restoreMainWindow } from "../main-window/restore-main-window";
import AppMenu from "../menu/app-menu";
import DockMenu from "../menu/dock-menu";
import createCheckForUpdatesContainerWindow from "../updates-warning-window/updates-warning-window";
import defaultAppSettings from "./app-settings";
import { willNavigateSecurity } from "./will-navigate-security";

app.allowRendererProcessReuse = true;

MakeLogsPath();
ipcBridge();
defaultAppSettings();
setupChildProcess();
MakeTempPath();
SetupFileWatcher();

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on("ready", () => {
  AppMenu();
  DockMenu();
  restoreMainWindow().then(() => {
    createCheckForUpdatesContainerWindow().catch(() => {});
  });
  app.on("activate", function () {
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
  contents.on("will-navigate", async (event, navigationUrl) => {
    await willNavigateSecurity(event, navigationUrl);
  });
});
