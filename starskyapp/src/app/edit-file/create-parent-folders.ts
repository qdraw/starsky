import * as fs from "fs";
import * as path from "path";
import { Slugify } from "../../shared/slugify";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import { TempPath } from "../config/temp-path";

export async function GetParentDiskPath(parentSubDir: string) {
  return path.join(
    TempPath(),
    Slugify((await GetBaseUrlFromSettings()).location).replace(/https?/gi, ""),
    parentSubDir
  );
}

export async function createParentFolders(parentSubDir: string) {
  const parentFullFilePath = await GetParentDiskPath(parentSubDir);
  console.log(parentFullFilePath);

  await fs.promises.mkdir(parentFullFilePath, {
    recursive: true
  });
}
