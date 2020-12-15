import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";

export async function saveRememberUrl(newWindow: BrowserWindow) {
  var url = new URL(newWindow.webContents.getURL()).search;
  console.log(url);
  await appConfig.set(RememberUrl, encodeURI(url));
}
