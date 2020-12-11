import { BrowserWindow, Menu, MenuItem } from "electron";
import * as appConfig from "electron-settings";
import * as path from "path";
import RememberUrl from "../config/remember-url-settings.const";
import { windowStateKeeper } from "../window-state-keeper/window-state-keeper";
import { mainWindows } from "./main-windows.const";

async function createMainWindow(relativeUrl: string = null) {
  const mainWindowStateKeeper = await windowStateKeeper("main");

  let x = mainWindowStateKeeper.x;
  let y = mainWindowStateKeeper.y;

  const currentWindow = BrowserWindow.getFocusedWindow();

  if (currentWindow) {
    const [currentWindowX, currentWindowY] = currentWindow.getPosition();
    x = currentWindowX + 10;
    y = currentWindowY + 10;
  }

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
  const reloadRedirectPage = path.join(
    "..",
    "..",
    "client",
    "pages",
    "redirect",
    "reload-redirect.html"
  );
  newWindow.loadFile(reloadRedirectPage, {
    query: { "remember-url": rememberUrl }
  });

  newWindow.webContents.session.webRequest.onHeadersReceived(
    (res, callback) => {
      // var currentSettings = appConfig.get("remote_settings_" + isPackaged());
      // var localhost = "http://localhost:9609 "; // with space on end

      // let whitelistDomain = localhost;
      // if (currentSettings && currentSettings.location) {
      //   whitelistDomain = !currentSettings.location ? localhost : localhost + new URL(currentSettings.location).origin;
      // }

      // // When change also check if CSPMiddleware needs to be updated
      // var csp = "default-src 'none'; img-src 'self' file://* https://www.openstreetmap.org https://tile.openstreetmap.org https://*.tile.openstreetmap.org "
      //  + whitelistDomain + "; " +      "style-src file://* unsafe-inline https://www.openstreetmap.org " + whitelistDomain
      //   + "; script-src 'self' file://* https://az416426.vo.msecnd.net; " +
      //   "connect-src 'self' https://dc.services.visualstudio.com/v2/track https://*.in.applicationinsights.azure.com//v2/track " + whitelistDomain + "; " +
      //   "font-src file://* " + whitelistDomain + "; media-src " + whitelistDomain + ";";

      // if (!res.url.startsWith('devtools://') && !res.url.startsWith('http://localhost:3000/')  ) {
      //   res.responseHeaders["Content-Security-Policy"] = csp;
      // }

      callback({ cancel: false, responseHeaders: res.responseHeaders });
    }
  );

  // Spellcheck
  const session = newWindow.webContents.session;
  session.setSpellCheckerLanguages(["nl-NL", "en-GB"]);

  newWindow.webContents.on("context-menu", (_e, params) => {
    if (params.dictionarySuggestions && params.dictionarySuggestions.length) {
      const objMenu = new Menu();
      const objMenuHead = new MenuItem({
        label: "Corrections",
        enabled: false
      });
      objMenu.append(objMenuHead);
      const objMenuSep = new MenuItem({
        type: "separator"
      });
      objMenu.append(objMenuSep);
      params.dictionarySuggestions.map((strSuggestion) => {
        const objMenuItem = new MenuItem({
          click(_this, objWindow) {
            objWindow.webContents.insertText(strSuggestion);
            objMenu.closePopup(this);
          },
          label: strSuggestion
        });
        objMenu.append(objMenuItem);
      });
      objMenu.popup({
        window: newWindow,
        x: params.x,
        y: params.y
      });
    }
  });
  // end spell check

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
