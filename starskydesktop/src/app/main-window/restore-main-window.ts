import * as appConfig from "electron-settings";
import RememberUrl from "../config/remember-url-settings.const";
import logger from "../logger/logger";
import createMainWindow from "./create-main-window";

async function runCreateWindow(rememberUrls: any) {
  let i = 0;
  // eslint-disable-next-line no-restricted-syntax, @typescript-eslint/no-unsafe-argument
  for (const key of Object.keys(rememberUrls)) {
    // eslint-disable-next-line no-await-in-loop, @typescript-eslint/no-unsafe-argument, @typescript-eslint/no-unsafe-member-access
    await createMainWindow(rememberUrls[key], i * 20);
    // eslint-disable-next-line no-plusplus
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

export async function restoreMainWindow(): Promise<void> {
  // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
  const rememberUrls = await getRememberUrl();

  logger.info("[restoreMainWindow] rememberUrls");
  logger.info(rememberUrls);

  // remove the config and set it when the new windows open, so the id's are matching

  try {
    await appConfig.set(RememberUrl, {});
  } catch (error) {
    // nothing in
  }
  await runCreateWindow(rememberUrls);
}
