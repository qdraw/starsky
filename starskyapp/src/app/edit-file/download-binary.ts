import { rmSync } from "fs";
import * as path from "path";
import { FileExtensions } from "../../shared/file-extensions";
import { IFileIndexItem } from "../../shared/IFileindexItem";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import UrlQuery from "../config/url-query";
import logger from "../logger/logger";
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

  logger.info("fileOnDisk > " + fileOnDisk);

  try {
    await downloadNetRequest(
      `${(await GetBaseUrlFromSettings()).location}${new UrlQuery().DownloadPhoto(
        lastSubPath
      )}`,
      session,
      fileOnDisk
    );
  } catch (error) {
    logger.info("retry > " + fileOnDisk);
    
    rmSync(fileOnDisk);
      await downloadNetRequest(
        `${(await GetBaseUrlFromSettings()).location}${new UrlQuery().DownloadPhoto(
          lastSubPath
        )}`,
        session,
        fileOnDisk
      );

  }

  return fileOnDisk;
}
