import { BrowserWindow } from "electron";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import UrlQuery from "../config/url-query";
import { downloadNetRequest } from "../net-request/download-net-request";
import {
  GetNetRequest,
  IGetNetRequestResponse
} from "../net-request/get-net-request";
import {
  createParentFolders,
  GetParentDiskPath
} from "./create-parent-folders";
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

    await createParentFolders(result.data.fileIndexItem.parentDirectory);
    await downloadXmpFile(result, fromMainWindow.webContents.session);
  } catch (error) {}
}

async function downloadXmpFile(
  result: IGetNetRequestResponse,
  session: Electron.Session
) {
  const ext = result.data.fileIndexItem.sidecarExtensionsList[0];

  const sidecarFileOnDisk = path.join(
    await GetParentDiskPath(result.data.fileIndexItem.parentDirectory),
    result.data.fileIndexItem.fileCollectionName + "." + ext
  );

  const sideCarSubPath =
    result.data.fileIndexItem.parentDirectory +
    "/" +
    result.data.fileIndexItem.fileCollectionName +
    "." +
    ext;

  await downloadNetRequest(
    `${
      (await GetBaseUrlFromSettings()).location
    }/starsky/api/download-sidecar?isThumbnail=false&f=${sideCarSubPath}`,
    session,
    sidecarFileOnDisk
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
