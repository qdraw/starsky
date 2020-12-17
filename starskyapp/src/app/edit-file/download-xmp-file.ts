import * as path from "path";
import { IFileIndexItem } from "../../shared/IFileindexItem";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import UrlQuery from "../config/url-query";
import { downloadNetRequest } from "../net-request/download-net-request";
import { GetParentDiskPath } from "./get-parent-disk-path";

export async function downloadXmpFile(
  fileIndexItem: IFileIndexItem,
  session: Electron.Session
): Promise<string> {
  if (fileIndexItem.sidecarExtensionsList.length <= 0) {
    return;
  }

  const ext = fileIndexItem.sidecarExtensionsList[0];

  const sidecarFileOnDisk = path.join(
    await GetParentDiskPath(fileIndexItem.parentDirectory),
    fileIndexItem.fileCollectionName + "." + ext
  );

  const sideCarSubPath =
    fileIndexItem.parentDirectory +
    "/" +
    fileIndexItem.fileCollectionName +
    "." +
    ext;

  console.log(sideCarSubPath);

  await downloadNetRequest(
    `${
      (await GetBaseUrlFromSettings()).location
    }${new UrlQuery().DownloadSidecarFile(sideCarSubPath)}`,
    session,
    sidecarFileOnDisk
  );
  return sidecarFileOnDisk;
}
