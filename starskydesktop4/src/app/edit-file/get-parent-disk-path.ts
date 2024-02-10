import * as path from "path";
import { Slugify } from "../../shared/slugify";
import { GetBaseUrlFromSettings } from "../config/get-base-url-from-settings";
import { TempPath } from "../config/temp-path";

export async function GetParentDiskPath(parentSubDir: string = null) {
  const tempPathIncludingBaseUrl = path.join(
    TempPath(),
    Slugify((await GetBaseUrlFromSettings()).location).replace(/https?/gi, "")
  );

  if (!parentSubDir) {
    return tempPathIncludingBaseUrl;
  }

  return path.join(tempPathIncludingBaseUrl, parentSubDir);
}
