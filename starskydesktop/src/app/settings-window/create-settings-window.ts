import { BrowserWindow } from "electron";
import * as path from "path";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import { settingsWindows } from "./settings-windows.const";

export const createSettingsWindow = async () => {
  const mainWindowStateKeeper = await windowStateKeeper("settings");

  let newWindow = new BrowserWindow({
    x: mainWindowStateKeeper.x,
    y: mainWindowStateKeeper.y,
    width: 350,
    height: 500,
    show: true,
    resizable: true,
    webPreferences: {
      partition: "persist:main",
      contextIsolation: true,
      preload: path.join(__dirname, "preload-main.bundle.js"), // use a preload script
    },
  });

  // hides the menu for windows
  newWindow.setMenu(null);

  mainWindowStateKeeper.track(newWindow);

  const settingsPage = path.join(
    __dirname,
    "client",
    "pages",
    "settings",
    "settings.html",
  );

  await newWindow.loadFile(settingsPage);

  newWindow.once("ready-to-show", () => {
    newWindow.show();
  });

  newWindow.on("closed", () => {
    settingsWindows.delete(newWindow);
    newWindow = null;
  });

  settingsWindows.add(newWindow);
  return newWindow;
};

export default createSettingsWindow;
