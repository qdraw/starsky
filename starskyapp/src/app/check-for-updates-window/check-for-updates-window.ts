import * as appConfig from "electron-settings";
import { DifferenceInDate } from "../../shared/date";
import {
  LastCheckedDateSettings,
  UpdatePolicySettings
} from "../config/update-policy-settings.const";

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
  if ((await isPolicyEnabled()) || (await SkipDisplayOfUpdate())) {
    return;
  }
  setTimeout(() => {}, 5000);
}

function createCheckForUpdatesWindow() {
  // 202
}

export default createCheckForUpdatesContainerWindow;
