import { BrowserWindow } from "electron";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import UrlQuery from "../config/url-query";
import { createErrorWindow } from "../error-window/create-error-window";
import logger from "../logger/logger";
import { GetNetRequest } from "../net-request/get-net-request";
import { createParentFolders } from "./create-parent-folders";
import { downloadBinary } from "./download-binary";
import { downloadXmpFile } from "./download-xmp-file";
import { IsDetailViewResult } from "./is-detail-view-result";
import { openPath } from "./open-path";

export async function EditFile(fromMainWindow: BrowserWindow) {
  const url = (await GetBaseUrlFromSettings()).location
    + new UrlQuery().Index(getFilePathFromWindow(fromMainWindow));

  let result;
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

  await createParentFolders(result.data.fileIndexItem.parentDirectory);

  await downloadXmpFile(
    result.data.fileIndexItem,
    fromMainWindow.webContents.session
  );
  const filePathOnDisk = await downloadBinary(
    result.data.fileIndexItem,
    fromMainWindow.webContents.session
  );

  try {
    await openPath(filePathOnDisk);
  } catch (error) {
    createErrorWindow(error);
    logger.warn("openPath error");
    logger.warn(error);
  }
}

function getFilePathFromWindow(fromMainWindow: BrowserWindow): string {
  const latestPage = fromMainWindow.webContents.getURL();
  const filePath = new URLSearchParams(new URL(latestPage).search).get("f");
  if (!filePath) return null;
  return filePath;
}
