import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";

export async function saveRememberUrl(newWindow: BrowserWindow) {
  const url = new URL(newWindow.webContents.getURL()).search;
  const currentObject = (await appConfig.get(RememberUrl)) as any;

  const newlyAddedObject = {
    [newWindow.id]: url
  };

  // new users
  if (!currentObject) {
    console.log("--> new ");

    await appConfig.set(RememberUrl, newlyAddedObject);
    return;
  }
  console.log("--> merge ");

  const combinedObject = { ...currentObject, ...newlyAddedObject };
  await appConfig.set(RememberUrl, combinedObject);
}

export async function removeRememberUrl(newWindow: BrowserWindow) {
  const currentObject = (await appConfig.get(RememberUrl)) as any;
  delete currentObject[newWindow.id];
  await appConfig.set(RememberUrl, currentObject);
}
