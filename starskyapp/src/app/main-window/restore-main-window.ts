import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import createMainWindow from "./create-main-window";

export async function restoreMainWindow() {
  const rememberUrls = await getRememberUrl();

  console.log("rememberUrls");
  console.log(rememberUrls);

  for (var key of Object.keys(rememberUrls)) {
    const index = Number(key);
    createMainWindow(rememberUrls[key], index * 20);
  }
}

async function getRememberUrl(): Promise<any> {
  if (await appConfig.has(RememberUrl)) {
    return (await appConfig.get(RememberUrl)) as object;
  }
  return { 0: "?f=/" };
}
