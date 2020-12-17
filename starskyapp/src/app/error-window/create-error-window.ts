import { BrowserWindow } from "electron";
import * as path from "path";
import { isPackaged } from "../os-info/is-packaged";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import { errorWindows } from "./error-windows.const";

export const createErrorWindow = async (error: string) => {
  const mainWindowStateKeeper = await windowStateKeeper("error");

  let newWindow = new BrowserWindow({
    x: mainWindowStateKeeper.x,
    y: mainWindowStateKeeper.y,
    width: 350,
    height: 300,
    show: true,
    resizable: !isPackaged(),
    webPreferences: {
      enableRemoteModule: false,
      contextIsolation: true
    }
  });

  // hides the menu for windows
  newWindow.setMenu(null);

  mainWindowStateKeeper.track(newWindow);

  const errorPage = path.join(
    __dirname,
    "..",
    "..",
    "client",
    "pages",
    "error",
    "error.html"
  );
  newWindow.loadFile(errorPage, { query: { error } });

  newWindow.once("ready-to-show", () => {
    newWindow.show();
  });

  newWindow.on("closed", () => {
    errorWindows.delete(newWindow);
    newWindow = null;
  });

  errorWindows.add(newWindow);
  return newWindow;
};
