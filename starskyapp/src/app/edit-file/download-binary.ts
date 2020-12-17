import path = require("path");
import { FileExtensions } from "../../shared/file-extensions";
import { IFileIndexItem } from "../../shared/IFileindexItem";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import UrlQuery from "../config/url-query";
import { downloadNetRequest } from "../net-request/download-net-request";
import { GetParentDiskPath } from "./get-parent-disk-path";

export async function downloadBinary(
  fileIndexItem: IFileIndexItem,
  session: Electron.Session
): Promise<string> {
  // get the last of the array
  const lastSubPath =
    fileIndexItem.collectionPaths[fileIndexItem.collectionPaths.length - 1];
  const fileName = new FileExtensions().GetFileName(lastSubPath);

  const fileOnDisk = path.join(
    await GetParentDiskPath(fileIndexItem.parentDirectory),
    fileName
  );

  console.log("fileOnDisk > " + fileOnDisk);

  await downloadNetRequest(
    `${(await GetBaseUrlFromSettings()).location}${new UrlQuery().DownloadPhoto(
      lastSubPath
    )}`,
    session,
    fileOnDisk
  );
  return fileOnDisk;
}
