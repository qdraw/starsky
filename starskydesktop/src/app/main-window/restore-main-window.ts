import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import logger from "../logger/logger";
import CreateMainWindow from "./create-main-window";

async function runCreateWindow(baseUrl: string, rememberUrls: { [key: string]: string }) {
  await Promise.all(
    Object.values(rememberUrls).map(async (url, index) => {
      await CreateMainWindow(baseUrl + url, index * 20);
    })
  );
}

async function getRememberUrl(): Promise<{ [key: string]: string }> {
  const fallbackConfig = { 0: "?f=/" };
  if (await appConfig.has(RememberUrl)) {
    const getConfig = (await appConfig.get(RememberUrl)) as { [key: string]: string };
    if (Object.keys(getConfig).length >= 1) return getConfig;
    return fallbackConfig;
  }
  return fallbackConfig;
}

export async function RestoreMainWindow(baseUrl: string): Promise<void> {
  const rememberUrls = await getRememberUrl();

  logger.info("[RestoreMainWindow] rememberUrls");
  logger.info(rememberUrls);

  // remove the config and set it when the new windows open, so the id's are matching

  try {
    await appConfig.set(RememberUrl, {});
  } catch (error) {
    // nothing in
  }
  await runCreateWindow(baseUrl, rememberUrls);
}
