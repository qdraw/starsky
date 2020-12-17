import * as fs from "fs";
import { Stats } from "fs";
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

  // const url = toSubPath.endsWith(".xmp")
  //   ? (await GetBaseUrlFromSettings()) + "/starsky/api/upload-sidecar"
  //   : (await GetBaseUrlFromSettings()) + "/starsky/api/upload";

  // uploadNetRequest();
}
