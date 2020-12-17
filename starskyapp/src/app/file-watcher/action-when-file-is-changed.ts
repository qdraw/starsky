import { session } from "electron";
import * as fs from "fs";
import { Stats } from "fs";
import * as path from "path";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import { GetParentDiskPath } from "../edit-file/get-parent-disk-path";
import { uploadNetRequest } from "../net-request/upload-net-request";
import { CheckFileExist } from "./check-file-exist";

export async function ActionWhenFileIsChanged(
  filePathOnDisk: string,
  stats: Stats
) {
  if (
    filePathOnDisk.endsWith(".info") ||
    filePathOnDisk.endsWith(".DS_Store")
  ) {
    return;
  }

  const infoFilePath = filePathOnDisk + ".info";

  if (await CheckFileExist(infoFilePath)) {
    const oldByteSize = await fs.promises.readFile(infoFilePath, {
      encoding: "utf-8"
    });
    if (oldByteSize === stats.size.toString()) {
      return;
    }
  }

  const toSubPath = filePathOnDisk
    .replace(await GetParentDiskPath(), "")
    .replace(path.sep, "/");

  const url = toSubPath.endsWith(".xmp")
    ? (await GetBaseUrlFromSettings()).location + "/starsky/api/upload-sidecar"
    : (await GetBaseUrlFromSettings()).location + "/starsky/api/upload";

  uploadNetRequest(
    url,
    toSubPath,
    filePathOnDisk,
    session.fromPartition("persist:main")
  );
}
