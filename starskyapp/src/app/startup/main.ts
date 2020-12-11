import { app, BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import { setupChildProcess } from "../child-process/setup-child-process";
import { LocationUrlSettingsKey } from "../config/location-settings.const";
import ipcBridge from "../ipc-bridge/ipc-bridge";
import createMainWindow from "../main-window/create-main-window";
import AppMenu from "../menu/menu";
import defaultAppSettings from "./app-settings";

app.allowRendererProcessReuse = true;

ipcBridge();
defaultAppSettings();
AppMenu();
setupChildProcess();

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on("ready", () => {
  createMainWindow();
  app.on("activate", function () {
    // On macOS it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (BrowserWindow.getAllWindows().length === 0) createMainWindow();
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
app.on("web-contents-created", (event, contents) => {
  contents.on("will-navigate", async (event, navigationUrl) => {
    const parsedUrl = new URL(navigationUrl);

    // for example the upgrade page
    if (navigationUrl.startsWith("file://")) {
      return;
    }

    // // to allow remote connections
    var currentSettings = await appConfig.get(LocationUrlSettingsKey);
    if (
      currentSettings &&
      currentSettings &&
      currentSettings &&
      parsedUrl.origin.startsWith(new URL(currentSettings.toString()).origin)
    ) {
      return;
    }

    if (!parsedUrl.origin.startsWith("http://localhost:")) {
      event.preventDefault();
    }
  });
});
