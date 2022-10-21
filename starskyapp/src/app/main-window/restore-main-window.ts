import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import logger from "../logger/logger";
import createMainWindow from "./create-main-window";

export async function restoreMainWindow(): Promise<void> {
  const rememberUrls = await getRememberUrl();

  logger.info("[restoreMainWindow] rememberUrls");
  logger.info(rememberUrls);

  // remove the config and set it when the new windows open, so the id's are matching

  try {
    await appConfig.set(RememberUrl, {});
  } catch (error) {}
  runCreateWindow(rememberUrls);
}

async function runCreateWindow(rememberUrls: any) {
  let i = 0;
  for (const key of Object.keys(rememberUrls)) {
    await createMainWindow(rememberUrls[key], i * 20);
    i++;
  }
}

async function getRememberUrl(): Promise<any> {
  const fallbackConfig = { 0: "?f=/" };
  if (await appConfig.has(RememberUrl)) {
    const getConfig = (await appConfig.get(RememberUrl)) as object;
    if (Object.keys(getConfig).length >= 1) return getConfig;
    return fallbackConfig;
  }
  return fallbackConfig;
}
