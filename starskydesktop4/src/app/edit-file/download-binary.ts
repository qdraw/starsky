import { existsSync, renameSync, rmSync } from "fs";
import * as path from "path";
import { IFileIndexItem } from "../../shared/IFileindexItem";
import { FileExtensions } from "../../shared/file-extensions";
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
  const collectionPathsWithoutXmp = fileIndexItem.collectionPaths.filter(
    (x) => !x.endsWith("xmp")
  );
  const lastSubPath = collectionPathsWithoutXmp[collectionPathsWithoutXmp.length - 1];
  const fileName = new FileExtensions().GetFileName(lastSubPath);

  const fileOnDisk = path.join(
    await GetParentDiskPath(fileIndexItem.parentDirectory),
    fileName
  );

  logger.info(`fileOnDisk > ${fileOnDisk}`);

  try {
    await downloadNetRequest(
      `${
        (
          await GetBaseUrlFromSettings()
        ).location
      }${new UrlQuery().DownloadPhoto(lastSubPath)}`,
      session,
      `${fileOnDisk}.tmp`
    );
  } catch (error) {
    logger.info(`retry > ${fileOnDisk}.tmp`);

    if (existsSync(`${fileOnDisk}.tmp`)) {
      rmSync(`${fileOnDisk}.tmp`);
    }
    try {
      await downloadNetRequest(
        `${
          (
            await GetBaseUrlFromSettings()
          ).location
        }${new UrlQuery().DownloadPhoto(lastSubPath)}`,
        session,
        `${fileOnDisk}.tmp`
      );
    } catch (error2: any) {
      // eslint-disable-next-line @typescript-eslint/restrict-template-expressions , @typescript-eslint/no-unsafe-member-access
      logger.info(`error > ${error2.toString()}`);
    }
  }

  if (!existsSync(`${fileOnDisk}.tmp`)) {
    return null;
  }

  renameSync(`${fileOnDisk}.tmp`, fileOnDisk);
  return fileOnDisk;
}
