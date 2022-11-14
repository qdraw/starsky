import { BrowserWindow } from "electron";
import { IFileIndexItem } from "src/shared/IFileindexItem";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import UrlQuery from "../config/url-query";
import { createErrorWindow } from "../error-window/create-error-window";
import logger from "../logger/logger";
import { GetNetRequest, IGetNetRequestResponse } from "../net-request/get-net-request";
import { createParentFolders } from "./create-parent-folders";
import { downloadBinary } from "./download-binary";
import { downloadXmpFile } from "./download-xmp-file";
import { IsDetailViewResult } from "./is-detail-view-result";
import { openPath } from "./open-path";

function getFilePathFromWindow(fromMainWindow: BrowserWindow): string {
  const latestPage = fromMainWindow.webContents.getURL();
  const filePath = new URLSearchParams(new URL(latestPage).search).get("f");
  if (!filePath) return null;
  return filePath;
}

async function openWindow(filePathOnDisk: string) {
  try {
    await openPath(filePathOnDisk);
  } catch (error :unknown) {
    // eslint-disable-next-line @typescript-eslint/no-floating-promises
    createErrorWindow(error as string);
    logger.warn("openPath error");
    logger.warn(error);
  }
}

export async function EditFile(fromMainWindow: BrowserWindow) {
  const subPath = new UrlQuery().Index(getFilePathFromWindow(fromMainWindow));
  const url = (await GetBaseUrlFromSettings()).location + subPath;

  let result :IGetNetRequestResponse;
  try {
    result = await GetNetRequest(url, fromMainWindow.webContents.session);
  } catch (error) {
    logger.warn("GetNetRequest error");
    logger.warn(error);
    return;
  }

  if (!IsDetailViewResult(result)) {
    return;
  }

  // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access, @typescript-eslint/no-unnecessary-type-assertion, @typescript-eslint/no-explicit-any
  const fileIndexItem = ((result.data as any).fileIndexItem as IFileIndexItem);

  await createParentFolders(fileIndexItem.parentDirectory);

  await downloadXmpFile(
    fileIndexItem,
    fromMainWindow.webContents.session
  );
  const filePathOnDisk = await downloadBinary(
    fileIndexItem,
    fromMainWindow.webContents.session
  );
  await openWindow(filePathOnDisk);
}
