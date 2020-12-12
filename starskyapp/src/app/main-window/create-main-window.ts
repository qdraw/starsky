import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import * as path from "path";
import RememberUrl from "../config/remember-url-settings.const";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import { getNewFocusedWindow } from "./get-new-focused-window";
import { mainWindows } from "./main-windows.const";
import { onHeaderReceived } from "./on-headers-received";
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

  mainWindowStateKeeper.track(newWindow);

  const rememberUrl = await getRememberUrl(relativeUrl);

  const location = path.join(
    __dirname,
    "..",
    "..",
    "client/pages/redirect/reload-redirect.html"
  );
  console.log("location " + location);

  newWindow.loadFile(location, {
    query: { "remember-url": rememberUrl }
  });

  spellCheck(newWindow);
  onHeaderReceived(newWindow);

  newWindow.once("ready-to-show", () => {
    newWindow.show();
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
