import { BrowserWindow } from "electron";
import * as path from "path";
import { GetAppVersion } from "../config/get-app-version";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import { getNewFocusedWindow } from "./get-new-focused-window";
import { mainWindows } from "./main-windows.const";
import { removeRememberUrl, saveRememberUrl } from "./save-remember-url";
import { spellCheck } from "./spellcheck";

async function createMainWindow(openSpecificUrl: string, offset = 0): Promise<BrowserWindow> {
  const mainWindowStateKeeper = await windowStateKeeper("main");

  const { x, y } = getNewFocusedWindow(
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
      sandbox: false,
      webviewTag: true,
      spellcheck: true,
      partition: "persist:main",
      contextIsolation: true,
      preload: path.join(__dirname, "preload-main.bundle.js"), // use a preload script preload-main.js
    },
  });

  // Add Starsky as user agent also in develop mode
  newWindow.webContents.userAgent = `${newWindow.webContents.userAgent} starsky/${GetAppVersion()}`;

  mainWindowStateKeeper.track(newWindow);

  const location = path.join(__dirname, "client/pages/redirect/reload-redirect.html");

  await newWindow.loadFile(location, {
    query: { "remember-url": openSpecificUrl },
  });

  spellCheck(newWindow);

  newWindow.once("ready-to-show", () => {
    newWindow.show();
  });

  newWindow.webContents.setWindowOpenHandler(({ url }) => {
    console.log(url);

    return {
      action: "allow",
      overrideBrowserWindowOptions: {
        webPreferences: {
          devTools: true, // allow
          partition: "persist:main",
          contextIsolation: true,
          preload: path.join(__dirname, "preload-main.bundle.js"), // use a preload script
        },
      },
    };
  });

  // normal navigations
  newWindow.webContents.on("did-navigate", () => {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    saveRememberUrl(newWindow);
  });

  // hash navigations
  newWindow.webContents.on("did-navigate-in-page", () => {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    saveRememberUrl(newWindow);
  });

  // Emitted when the window is going to be closed
  newWindow.on("close", () => {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
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
