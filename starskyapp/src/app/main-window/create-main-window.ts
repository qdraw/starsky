import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import * as path from "path";
import { GetAppVersion } from "../config/get-app-version";
import RememberUrl from "../config/remember-url-settings.const";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import { getNewFocusedWindow } from "./get-new-focused-window";
import { mainWindows } from "./main-windows.const";
import { onHeaderReceived } from "./on-headers-received";
import { saveRememberUrl } from "./save-remember-url";
import { spellCheck } from "./spellcheck";

async function createMainWindow(relativeUrl: string = null) {
  const mainWindowStateKeeper = await windowStateKeeper("main");

  let { x, y } = getNewFocusedWindow(
    mainWindowStateKeeper.x,
    mainWindowStateKeeper.y
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
      enableRemoteModule: false,
      partition: "persist:main",
      contextIsolation: true,
      preload: path.join(__dirname, "..", "..", "preload", "preload-main.js") // use a preload script
    }
  });

  // Add Starsky as user agent also in develop mode
  newWindow.webContents.userAgent =
    newWindow.webContents.userAgent + " starsky/" + GetAppVersion();

  mainWindowStateKeeper.track(newWindow);

  const rememberUrl = await getRememberUrl(relativeUrl);

  const location = path.join(
    __dirname,
    "..",
    "..",
    "client/pages/redirect/reload-redirect.html"
  );

  newWindow.loadFile(location, {
    query: { "remember-url": rememberUrl }
  });

  spellCheck(newWindow);
  onHeaderReceived(newWindow);

  newWindow.once("ready-to-show", () => {
    newWindow.show();
  });

  newWindow.webContents.on("did-navigate", () => {
    saveRememberUrl(newWindow);
  });

  newWindow.on("closed", () => {
    mainWindows.delete(newWindow);
    newWindow = null;
  });

  mainWindows.add(newWindow);
  return newWindow;
}

async function getRememberUrl(relativeUrl: string | null): Promise<string> {
  if (relativeUrl !== null) {
    return relativeUrl;
  }

  if (await appConfig.has(RememberUrl)) {
    return (await appConfig.get(RememberUrl)).toString();
  }
  return "";
}

export default createMainWindow;
