import { BrowserWindow } from "electron";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import UrlQuery from "../config/url-query";
import {
  GetNetRequest,
  IGetNetRequestResponse
} from "../net-request/get-net-request";

export async function EditFile(fromMainWindow: BrowserWindow) {
  const url =
    GetBaseUrlFromSettings() +
    new UrlQuery().Index(getFilePathFromWindow(fromMainWindow));
  try {
    const result = await GetNetRequest(url, fromMainWindow.webContents.session);

    if (filterResult(result)) {
      console.log("true");
    }
  } catch (error) {}
}
function filterResult(result: IGetNetRequestResponse) {
  return (
    !result.data ||
    !result.data.fileIndexItem ||
    !result.data.fileIndexItem.status ||
    result.data.fileIndexItem.status === "Ok" ||
    result.data.fileIndexItem.status === "Default"
  );
}

function getFilePathFromWindow(fromMainWindow: BrowserWindow): string {
  var latestPage = fromMainWindow.webContents.getURL();
  var filePath = new URLSearchParams(new URL(latestPage).search).get("f");
  if (!filePath) return null;
  return filePath;
}
