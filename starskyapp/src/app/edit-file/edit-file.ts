import { BrowserWindow } from "electron";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import UrlQuery from "../config/url-query";
import {
  GetNetRequest,
  IGetNetRequestResponse
} from "../net-request/get-net-request";
import path = require("path");

export async function EditFile(fromMainWindow: BrowserWindow) {
  const url =
    (await GetBaseUrlFromSettings()).location +
    new UrlQuery().Index(getFilePathFromWindow(fromMainWindow));
  console.log(url);

  try {
    const result = await GetNetRequest(url, fromMainWindow.webContents.session);

    if (!filterResult(result)) {
      return;
    }
    console.log("-- it ok");

    // download xmp file
    // doDownloadRequest(
    //   fromMainWindow,
    //   "download-sidecar",
    //   parentFullFilePathHelper(sidecarFile),
    //   sidecarFile
    // );
  } catch (error) {}
}

function getXmpFile(result: IGetNetRequestResponse) {
  const ext = result.data.fileIndexItem.sidecarExtensionsList[0];
  const sidecarFile = path.join(
    result.data.fileIndexItem.parentDirectory,
    result.data.fileIndexItem.fileCollectionName + "." + ext
  );
}

function filterResult(result: IGetNetRequestResponse) {
  return (
    result.statusCode === 200 &&
    result.data &&
    result.data.fileIndexItem &&
    result.data.fileIndexItem.status &&
    result.data.fileIndexItem.collectionPaths &&
    result.data.fileIndexItem.sidecarExtensionsList &&
    (result.data.fileIndexItem.status === "Ok" ||
      result.data.fileIndexItem.status === "Default")
  );
}

function getFilePathFromWindow(fromMainWindow: BrowserWindow): string {
  var latestPage = fromMainWindow.webContents.getURL();
  var filePath = new URLSearchParams(new URL(latestPage).search).get("f");
  if (!filePath) return null;
  return filePath;
}
