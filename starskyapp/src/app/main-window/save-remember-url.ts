import { BrowserWindow } from "electron";
import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";

export async function saveRememberUrl(newWindow: BrowserWindow) {
  const url = new URL(newWindow.webContents.getURL());

  if (url.protocol.startsWith("file:")) {
    return;
  }

  const currentObject = (await appConfig.get(RememberUrl)) as any;

  const relativeUrl = url.pathname + url.search;
  const newlyAddedObject = {
    [newWindow.id]: relativeUrl
  };

  // new users
  if (!currentObject) {
    await appConfig.set(RememberUrl, newlyAddedObject);
    return;
  }

  const combinedObject = { ...currentObject, ...newlyAddedObject };
  console.log("--> merge ", combinedObject);

  await appConfig.set(RememberUrl, combinedObject);
}

export async function removeRememberUrl(id: number) {
  const currentObject = (await appConfig.get(RememberUrl)) as any;
  delete currentObject[id];
  await appConfig.set(RememberUrl, currentObject);
}
