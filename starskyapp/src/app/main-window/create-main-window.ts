import { BrowserWindow } from "electron";
import * as path from "path";
import { GetAppVersion } from "../config/get-app-version";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import { getNewFocusedWindow } from "./get-new-focused-window";
import { mainWindows } from "./main-windows.const";
import { onHeaderReceived } from "./on-headers-received";
import { removeRememberUrl, saveRememberUrl } from "./save-remember-url";
import { spellCheck } from "./spellcheck";

async function createMainWindow(
  openSpecificUrl: string,
  offset: number = 0
): Promise<BrowserWindow> {
  const mainWindowStateKeeper = await windowStateKeeper("main");

  let { x, y } = getNewFocusedWindow(
    mainWindowStateKeeper.x - offset,
    mainWindowStateKeeper.y - offset
  );

  let newWindow = new BrowserWindow({
    x,
    y,
    width: mainWindowStateKeeper.width,
    height: mainWindowStateKeeper.height,
    show: false,
    webPreferences: {
      allowRunningInsecureContent: false,
      nodeIntegration: false,
      nativeWindowOpen: true,
      sandbox: false,
      partition: "persist:main",
      contextIsolation: true,
      preload: path.join(__dirname, "..", "..", "preload", "preload-main.js") // use a preload script
    }
  });

  // Add Starsky as user agent also in develop mode
  newWindow.webContents.userAgent =
    newWindow.webContents.userAgent + " starsky/" + GetAppVersion();

  mainWindowStateKeeper.track(newWindow);

  const location = path.join(
    __dirname,
    "..",
    "..",
    "client/pages/redirect/reload-redirect.html"
  );

  newWindow.loadFile(location, {
    query: { "remember-url": openSpecificUrl }
  });

  spellCheck(newWindow);
  onHeaderReceived(newWindow);

  newWindow.once("ready-to-show", () => {
    newWindow.show();
  });

  newWindow.webContents.setWindowOpenHandler(({ url }) => {
    return {
      action: 'allow',
      overrideBrowserWindowOptions: {
        webPreferences: {
          partition: "persist:main",
          contextIsolation: true,
          preload: path.join(__dirname, "..", "..", "preload", "preload-main.js") // use a preload script
        }
      }
    }
  })

  // normal navigations
  newWindow.webContents.on("did-navigate", () => {
    saveRememberUrl(newWindow);
  });

  // hash navigations
  newWindow.webContents.on("did-navigate-in-page", () => {
    saveRememberUrl(newWindow);
  });

  // Emitted when the window is going to be closed
  newWindow.on("close", () => {
    removeRememberUrl(newWindow.id);
  });

  /* when its already closed */
  newWindow.on("closed", () => {
    mainWindows.delete(newWindow);
    newWindow = null;
  });

  mainWindows.add(newWindow);
  return newWindow;
}

export default createMainWindow;
