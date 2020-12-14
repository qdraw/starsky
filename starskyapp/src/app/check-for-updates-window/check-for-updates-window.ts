import * as appConfig from "electron-settings";
import { DifferenceInDate } from "../../shared/date";
import { GetAppVersion } from "../config/get-app-version";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import {
  LastCheckedDateSettings,
  UpdatePolicySettings
} from "../config/update-policy-settings.const";
import UrlQuery from "../config/url-query";
import { GetNetRequest } from "./get-net-request";

/**
 * Skip display of message
 */
export async function SkipDisplayOfUpdate(): Promise<boolean> {
  const localStorageItem = (await appConfig.get(
    LastCheckedDateSettings
  )) as string;
  if (!localStorageItem) return false;

  var getItem = parseInt(localStorageItem);
  if (isNaN(getItem)) return false;
  return DifferenceInDate(getItem) < 5760; // 4 days
}

export async function isPolicyEnabled(): Promise<boolean> {
  const item = (await appConfig.get(UpdatePolicySettings)) as string;
  return item === "true";
}

async function createCheckForUpdatesContainerWindow() {
  const test = await isPolicyEnabled();
  const t2 = await SkipDisplayOfUpdate();

  if ((await isPolicyEnabled()) || (await SkipDisplayOfUpdate())) {
    return;
  }
  console.log(test, t2);

  setTimeout(
    () =>
      shouldItUpdate()
        .then((shouldItUpdate) => {
          console.log(shouldItUpdate);
        })
        .catch(() => {}),
    5000
  );
}

async function shouldItUpdate(): Promise<boolean> {
  return new Promise(async function (resolve, reject) {
    let url = (await GetBaseUrlFromSettings()).location;
    url += new UrlQuery().HealthCheckForUpdates(GetAppVersion());
    console.log(url);

    try {
      const result = await GetNetRequest(url);
      console.log(result);

      if (result.statusCode !== 202) {
        resolve(false);
        return;
      }
      resolve(true);
    } catch (error) {
      reject();
    }
  });
}

export default createCheckForUpdatesContainerWindow;
