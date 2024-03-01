import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import logger from "../logger/logger";
import createMainWindow from "./create-main-window";

async function runCreateWindow(baseUrl: string, rememberUrls: string[]) {
  let i = 0;
  for (const key of Object.keys(rememberUrls)) {
    const url = baseUrl + rememberUrls[key];
    await createMainWindow(url, i * 20);
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

export async function RestoreMainWindow(baseUrl: string): Promise<void> {
  // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
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
