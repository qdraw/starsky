import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import createMainWindow from "./create-main-window";

export async function restoreMainWindow(): Promise<void> {
  const rememberUrls = await getRememberUrl();

  // remove the config and set it when the new windows open, so the id's are matching
  await appConfig.set(RememberUrl, {});

  let i = 0;
  for (let key of Object.keys(rememberUrls)) {
    await createMainWindow(rememberUrls[key], i * 20);
    i++;
  }
}

async function getRememberUrl(): Promise<any> {
  if (await appConfig.has(RememberUrl)) {
    return (await appConfig.get(RememberUrl)) as object;
  }
  return { 0: "?f=/" };
}
