import { BrowserWindow } from "electron";
import path = require("path");

export function SetupSplash() :BrowserWindow {
  const splash = new BrowserWindow({
    width: 300,
    height: 200,
    transparent: true,
    frame: false,
    alwaysOnTop: true
  });

  splash.loadFile(path.join(__dirname,
    "..",
    "..",
    "client/pages/splash/splash.html"
    ));

  splash.center();

  splash.show();

  return splash;
};

export function CloseSplash(splash: BrowserWindow) : void {
  splash.close();
};


// console.log('-djfnlksdlksfd');

// await RetryGetNetRequest(`http://localhost:${appPort}/api/health`)

// // newWindow.once("ready-to-show",async () => {
// // newWindow.show();
// // });
